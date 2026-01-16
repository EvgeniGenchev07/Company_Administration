using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace App.PageModels;

public partial class AdminAllBusinessTripsPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly DatabaseService _dbService;
    private bool _isBusy;
    private bool _isRefreshing;

    [ObservableProperty]
    private ObservableCollection<BusinessTripViewModel> _filteredBusinessTrips = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime? _startDateFilter;

    [ObservableProperty]
    private DateTime? _endDateFilter;

    [ObservableProperty]
    private string _projectFilter = string.Empty;

    [ObservableProperty]
    private string _destinationFilter = string.Empty;

    [ObservableProperty]
    private byte? _statusFilter;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private bool _hasResults = true;

    // Status options for filter
    [ObservableProperty]
    private ObservableCollection<StatusOption> _statusOptions = new()
        {
            new StatusOption { Value = null, DisplayName = "Всички" },
            new StatusOption { Value = 0, DisplayName = "Чакаща" },
            new StatusOption { Value = 1, DisplayName = "Одобрена" },
            new StatusOption { Value = 2, DisplayName = "Отхвърлена" },
            new StatusOption { Value = 3, DisplayName = "Завършена" }
        };

    [ObservableProperty]
    private StatusOption _selectedStatusOption;

    public event PropertyChangedEventHandler PropertyChanged;
    [ObservableProperty]
    public ObservableCollection<BusinessTripViewModel> businessTrips = new();

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

    public int TotalTrips => BusinessTrips.Count;
    public int PendingTrips => BusinessTrips.Count(t => t.Status == BusinessTripStatus.Pending);
    public int ApprovedTrips => BusinessTrips.Count(t => t.Status == BusinessTripStatus.Approved);
    public int RejectedTrips => BusinessTrips.Count(t => t.Status == BusinessTripStatus.Rejected);

    public ICommand ApproveTripCommand { get; }
    public ICommand RejectTripCommand { get; }
    public ICommand SummaryCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminAllBusinessTripsPageModel(DatabaseService dbService)
    {
        _dbService = dbService;

        ApproveTripCommand = new Command<BusinessTripViewModel>(async (trip) => await ApproveTripAsync(trip));
        RejectTripCommand = new Command<BusinessTripViewModel>(async (trip) => await RejectTripAsync(trip));
        SummaryCommand = new Command(async () => await SummaryAsync());
        CancelCommand = new Command(async () => await CancelAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());

    }

    internal async Task LoadBusinessTripsAsync()
    {
        try
        {
            IsBusy = true;

            var trips = await _dbService.GetAllBusinessTripsAsync();

            BusinessTrips.Clear();
            foreach (var trip in trips)
            {
                BusinessTrips.Add(new BusinessTripViewModel(trip));
            }

            OnPropertyChanged(nameof(TotalTrips));
            OnPropertyChanged(nameof(PendingTrips));
            OnPropertyChanged(nameof(ApprovedTrips));
            OnPropertyChanged(nameof(RejectedTrips));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на командировки: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    [RelayCommand]
    private async Task ItemTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            BusinessTripDetailsPage.SelectedBusinessTrip = businessTrip;
            await Shell.Current.GoToAsync("//businesstripdetails");
        }
    }
    private async Task ApproveTripAsync(BusinessTripViewModel trip)
    {
        if (trip == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърдете одобрение",
            $"Искате ли да одобрите молбата за командировка?",
            "Одобри",
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;

                // Call API to approve business trip
                var success = await _dbService.ApproveBusinessTripAsync(trip.Id);
                if (success)
                {
                    trip.Status = BusinessTripStatus.Approved;
                    int index = BusinessTrips.IndexOf(trip);
                    if (index >= 0)
                    {
                        BusinessTrips[index] = trip;
                    }
                    OnPropertyChanged(nameof(PendingTrips));
                    OnPropertyChanged(nameof(ApprovedTrips));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Командировката бе одобрена успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно одобряване на командировка", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно одобряване на командировка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task RejectTripAsync(BusinessTripViewModel trip)
    {
        if (trip == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Потвърдете отхвърляне",
            $"Искате ли да отхвърлите молбата за командировка?",
            "Отхвърли",
            "Отказ");

        if (confirm)
        {
            try
            {
                IsBusy = true;

                var success = await _dbService.RejectBusinessTripAsync(trip.Id);
                if (success)
                {
                    trip.Status = BusinessTripStatus.Rejected;
                    int index = BusinessTrips.IndexOf(trip);
                    if (index >= 0)
                    {
                        BusinessTrips[index] = trip;
                    }
                    OnPropertyChanged(nameof(PendingTrips));
                    OnPropertyChanged(nameof(RejectedTrips));
                    await Application.Current.MainPage.DisplayAlert("Успех", "Командировката бе откзана успешно", "OK");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Грешка", "Неуспешно отхвърляне на командировка", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно отхвърляне на командировка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("//AdminPage");
    }

    private async Task SummaryAsync()
    {
        await Shell.Current.GoToAsync("//BusinessTripsSummaryPage");
    }

    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadBusinessTripsAsync();
        IsRefreshing = false;
    }

    private void ApplyFilters()
    {
        if (BusinessTrips == null) return;

        var filtered = BusinessTrips.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(trip =>
                (trip.ProjectName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (trip.Task?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (trip.CarTripDestination?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (trip.CarBrand?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (trip.CarModel?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        if (StartDateFilter.HasValue)
        {
            filtered = filtered.Where(trip => trip.StartDate >= StartDateFilter.Value);
        }

        if (EndDateFilter.HasValue)
        {
            filtered = filtered.Where(trip => trip.EndDate <= EndDateFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(ProjectFilter))
        {
            filtered = filtered.Where(trip =>
                trip.ProjectName?.Contains(ProjectFilter, StringComparison.OrdinalIgnoreCase) ?? false
            );
        }

        if (!string.IsNullOrWhiteSpace(DestinationFilter))
        {
            filtered = filtered.Where(trip =>
                trip.CarTripDestination?.Contains(DestinationFilter, StringComparison.OrdinalIgnoreCase) ?? false
            );
        }

        if (SelectedStatusOption?.Value.HasValue == true)
        {
            var statusValue = (BusinessTripStatus)SelectedStatusOption.Value.Value;
            filtered = filtered.Where(trip => trip.Status == statusValue);
        }

        FilteredBusinessTrips.Clear();
        foreach (var trip in filtered.OrderByDescending(t => t.StartDate))
        {
            FilteredBusinessTrips.Add(trip);
        }
        HasResults = FilteredBusinessTrips.Count > 0;
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        StartDateFilter = null;
        EndDateFilter = null;
        ProjectFilter = string.Empty;
        DestinationFilter = string.Empty;
        SelectedStatusOption = StatusOptions[0];
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnStartDateFilterChanged(DateTime? value) => ApplyFilters();
    partial void OnEndDateFilterChanged(DateTime? value) => ApplyFilters();
    partial void OnProjectFilterChanged(string value) => ApplyFilters();
    partial void OnDestinationFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedStatusOptionChanged(StatusOption value) => ApplyFilters();


    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}