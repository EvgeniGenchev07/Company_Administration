using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class AllAbsencesPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;
    private bool _isBusy;
    private bool _isRefreshing;
    private int _totalAbsences;
    private int _pendingAbsences;
    private int _approvedAbsences;
    private int _rejectedAbsences;
    private string _search;
    private int _selectedYear;
    private string _selectedMonth;

    public event PropertyChangedEventHandler PropertyChanged;

    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> allAbsences = new();

    [ObservableProperty]
    public ObservableCollection<int> availableYears = new();

    [ObservableProperty]
    public ObservableCollection<string> availableMonths = new();

    private List<AbsenceViewModel> _originalAbsences = new();

    public string Search
    {
        get => _search;
        set
        {
            _search = value;
            OnPropertyChanged();
            FilterAbsence();
        }
    }

    public int SelectedYear
    {
        get => _selectedYear;
        set
        {
            _selectedYear = value;
            OnPropertyChanged();
            FilterAbsence();
        }
    }

    public string SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            _selectedMonth = value;
            OnPropertyChanged();
            FilterAbsence();
        }
    }

    public int TotalAbsences
    {
        get => _totalAbsences;
        set
        {
            _totalAbsences = value;
            OnPropertyChanged();
        }
    }

    public int PendingAbsences
    {
        get => _pendingAbsences;
        set
        {
            _pendingAbsences = value;
            OnPropertyChanged();
        }
    }

    public int ApprovedAbsences
    {
        get => _approvedAbsences;
        set
        {
            _approvedAbsences = value;
            OnPropertyChanged();
        }
    }

    public int RejectedAbsences
    {
        get => _rejectedAbsences;
        set
        {
            _rejectedAbsences = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public bool HasNoResults => !(AllAbsences.Any());

    public ICommand BackCommand { get; }
    public ICommand RefreshCommand { get; }

    public AllAbsencesPageModel(DatabaseService dbService)
    {
        _dbService = dbService;

        BackCommand = new Command(async () => await BackAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

        AvailableYears = new ObservableCollection<int>(Enumerable.Range(DateTime.Now.Year - 5, 10));
        SelectedYear = DateTime.Now.Year;

        AvailableMonths = new ObservableCollection<string>(
            CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12));
        AvailableMonths.Add("Всички месеци");
        SelectedMonth = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[DateTime.Now.Month - 1];

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            if (App.User != null)
            {
                var absences = await _dbService.GetUserAbsencesAsync(App.User.Id);
                var sortedAbsences = absences.OrderByDescending(a => a.Created).ToList();

                _originalAbsences = sortedAbsences.Select(a => new AbsenceViewModel(a)).ToList();
                AllAbsences.Clear();
                foreach (var absence in _originalAbsences)
                {
                    AllAbsences.Add(absence);
                }
                UpdateStatistics();
                FilterAbsence();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на отсъствията: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateStatistics()
    {
        TotalAbsences = AllAbsences.Count;
        PendingAbsences = AllAbsences.Count(a => a.Status == AbsenceStatus.Pending);
        ApprovedAbsences = AllAbsences.Count(a => a.Status == AbsenceStatus.Approved);
        RejectedAbsences = AllAbsences.Count(a => a.Status == AbsenceStatus.Rejected);
        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(AllAbsences));
    }

    private void FilterAbsence()
    {
        try
        {
            IsBusy = true;
            ObservableCollection<AbsenceViewModel> filtered = new ObservableCollection<AbsenceViewModel>(_originalAbsences);

            if (SelectedYear > 0)
            {
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Year == SelectedYear || t.EndDate.Year == SelectedYear));
            }
            if (!string.IsNullOrEmpty(SelectedMonth) && SelectedMonth != "Всички месеци")
            {
                var monthIndex = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, SelectedMonth) + 1;
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.StartDate.Month == monthIndex || t.EndDate.Month == monthIndex));
            }

            if (!string.IsNullOrEmpty(Search))
            {
                filtered = new ObservableCollection<AbsenceViewModel>(
                    filtered.Where(t => t.TypeText.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                                      t.StatusText.Contains(Search, StringComparison.OrdinalIgnoreCase)));
            }

            AllAbsences.Clear();
            foreach (var item in filtered)
            {
                AllAbsences.Add(item);
            }

            UpdateStatistics();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDataAsync();
        IsRefreshing = false;
    }

    private async Task BackAsync()
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    [RelayCommand]
    public async void SelectAbsence(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            AbsenceDetailsPage.SelectedAbsence = absence;
            await Shell.Current.GoToAsync("AbsenceDetailsPage");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}