using ApplicationLayer.Interfaces;
using ApplicationLayer.ViewModels;
using BusinessLayer.Entities;
using BusinessLayer.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceLayer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ServiceLayer.PageModels;

public partial class MainPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly IDatabaseService _dbService;
    private bool _isBusy;
    private string _userName;
    private int _contractDays;
    private int _absenceDays;
    private int _pendingTripsCount;
    private int _pendingAbsencesCount;
    private int _approvedAbsencesCount;
    private int _approvedTripsCount;
    private bool _noAbsences;
    private bool _noBusinessTrips;
    //calendar fields
    private DateTime _currentDate;
    private bool _isDaySelected;
    private string _selectedDayTitle;
    private CalendarDay _selectedDay;
    private DateTime _selectedHolidayDate = DateTime.Today;
    private string _holidayName;
    private bool _isHolidayDialogVisible;
    private ObservableCollection<HolidayDay> _holidayDays;
    private List<DateTime> _customHolidays = new();

    public event PropertyChangedEventHandler PropertyChanged;

    [ObservableProperty]
    public ObservableCollection<AbsenceViewModel> recentAbsences = new();

    [ObservableProperty]
    public ObservableCollection<BusinessTripViewModel> recentBusinessTrips = new();
    public ObservableCollection<CalendarDay> CalendarDays { get; } = new();
    public ObservableCollection<BusinessTripViewModel> SelectedDayTrips { get; } = new();
    public ObservableCollection<AbsenceViewModel> SelectedDayAbsences { get; } = new();

    public string CurrentMonthYear => _currentDate.ToString("MMMM yyyy");

    public string UserName
    {
        get => _userName;
        set
        {
            _userName = value;
            OnPropertyChanged();
        }
    }

    public int ContractDays
    {
        get => _contractDays;
        set
        {
            _contractDays = value;
            OnPropertyChanged();
        }
    }

    public int AbsenceDays
    {
        get => _absenceDays;
        set
        {
            _absenceDays = value;
            OnPropertyChanged();
        }
    }

    public int PendingTripsCount
    {
        get => _pendingTripsCount;
        set
        {
            _pendingTripsCount = value;
            OnPropertyChanged();
        }
    }

    public int ApprovedTripsCount
    {
        get => _approvedTripsCount;
        set
        {
            _approvedTripsCount = value;
            OnPropertyChanged();
        }
    }
    public int PendingAbsencesCount
    {
        get => _pendingAbsencesCount;
        set
        {
            _pendingAbsencesCount = value;
            OnPropertyChanged();
        }
    }
    public int ApprovedAbsencesCount
    {
        get => _approvedAbsencesCount;
        set
        {
            _approvedAbsencesCount = value;
            OnPropertyChanged();
        }
    }
    public bool NoAbsences
    {
        get => _noAbsences;
        set
        {
            _noAbsences = value;
            OnPropertyChanged();
        }
    }

    public bool NoBusinessTrips
    {
        get => _noBusinessTrips;
        set
        {
            _noBusinessTrips = value;
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
    //Calendar properties
    public bool IsDaySelected
    {
        get => _isDaySelected;
        set
        {
            _isDaySelected = value;
            OnPropertyChanged();
        }
    }

    public string SelectedDayTitle
    {
        get => _selectedDayTitle;
        set
        {
            _selectedDayTitle = value;
            OnPropertyChanged();
        }
    }

    public CalendarDay SelectedDay
    {
        get => _selectedDay;
        set
        {
            _selectedDay = value;
            OnPropertyChanged();
        }
    }

    public DateTime SelectedHolidayDate
    {
        get => _selectedHolidayDate;
        set
        {
            _selectedHolidayDate = value;
            OnPropertyChanged();
        }
    }

    public string HolidayName
    {
        get => _holidayName;
        set
        {
            _holidayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsHolidayDialogVisible
    {
        get => _isHolidayDialogVisible;
        set
        {
            _isHolidayDialogVisible = value;
            OnPropertyChanged();
        }
    }

    public ICommand RequestAbsenceCommand { get; }
    public ICommand RequestBusinessTripCommand { get; }
    public ICommand ViewAllAbsencesCommand { get; }
    public ICommand ViewAllBusinessTripsCommand { get; }
    //Month navigation commands
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand NavigateToUsersCommand { get; }
    public ICommand NavigateToAbsencesCommand { get; }
    public ICommand NavigateToTripsCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand AddHolidayCommand { get; }
    public ICommand DeleteCustomHolidayCommand { get; }
    public ICommand ShowAddHolidayDialogCommand { get; }
    public ICommand HideAddHolidayDialogCommand { get; }

    private List<BusinessTrip> _allBusinessTrips = new();
    private List<Absence> _allAbsences = new();


    public MainPageModel(IDatabaseService dbService)
    {
        _dbService = dbService;
        _currentDate = DateTime.Now;

        //Calendar initialization
        PreviousMonthCommand = new Command(async () => await NavigateMonth(-1));
        NextMonthCommand = new Command(async () => await NavigateMonth(1));
        NavigateToUsersCommand = new Command(async () => await NavigateToUsersAsync());
        NavigateToAbsencesCommand = new Command(async () => await NavigateToAbsencesAsync());
        NavigateToTripsCommand = new Command(async () => await NavigateToTripsAsync());
        AddHolidayCommand = new Command(async () => await AddHolidayAsync());
        DeleteCustomHolidayCommand = new Command(async () => await DeleteCustomHolidayAsync());
        ShowAddHolidayDialogCommand = new Command(() => IsHolidayDialogVisible = true);
        HideAddHolidayDialogCommand = new Command(() => IsHolidayDialogVisible = false);


        RequestAbsenceCommand = new Command(async () => await RequestAbsenceAsync());
        RequestBusinessTripCommand = new Command(async () => await RequestBusinessTripAsync());
        ViewAllAbsencesCommand = new Command(async () => await ViewAllAbsencesAsync());
        ViewAllBusinessTripsCommand = new Command(async () => await ViewAllBusinessTripsAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
        MessagingCenter.Subscribe<DatabaseService>(this, "AbsenceCreated", async (sender) =>
        {
            AbsenceDays = DatabaseService.User.AbsenceDays;
            OnPropertyChanged(nameof(AbsenceDays));
        });
    }

    public async Task LoadDataAsync()
    {
        try
        {
            IsBusy = true;

            if (DatabaseService.User != null)
            {
                UserName = DatabaseService.User.Name;
                ContractDays = DatabaseService.User.ContractDays;
                AbsenceDays = DatabaseService.User.AbsenceDays;
                var businessTrips = DatabaseService.User.BusinessTrips ?? new List<BusinessTrip>();
                var recentTrips = businessTrips.OrderByDescending(t => t.Created).Take(5).ToList();

                PendingTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Pending);
                ApprovedTripsCount = businessTrips.Count(t => t.Status == BusinessTripStatus.Approved);

                RecentBusinessTrips.Clear();
                foreach (var trip in recentTrips)
                {
                    RecentBusinessTrips.Add(new BusinessTripViewModel(trip));
                }

                NoBusinessTrips = !recentTrips.Any();

                var absences = DatabaseService.User.Absences ?? new List<Absence>();
                var recentAbsences = absences.OrderByDescending(a => a.Created).Take(5).ToList();

                RecentAbsences.Clear();
                foreach (var absence in recentAbsences)
                {
                    RecentAbsences.Add(new AbsenceViewModel(absence));
                }
                ApprovedAbsencesCount = RecentAbsences.Count(t => t.Status == AbsenceStatus.Approved);
                PendingAbsencesCount = RecentAbsences.Count(t => t.Status == AbsenceStatus.Pending);
                NoAbsences = !recentAbsences.Any();
                //Calendar
                IsBusy = true;
                _allBusinessTrips = await _dbService.GetAllBusinessTripsAsync();
                _allAbsences = await _dbService.GetAllAbsencesAsync();
                _holidayDays = new ObservableCollection<HolidayDay>(await _dbService.GetAllHolidayDaysAsync());
                _customHolidays = _holidayDays.Select(h => h.Date.Date).ToList();
                GenerateCalendar();
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно зареждане на данните: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RequestAbsenceAsync()
    {
        await Shell.Current.GoToAsync("AbsencePage");
    }

    private async Task RequestBusinessTripAsync()
    {
        await Shell.Current.GoToAsync("request");
    }

    private async Task ViewAllAbsencesAsync()
    {
        await Shell.Current.GoToAsync("AllAbsencesPage");
    }

    private async Task ViewAllBusinessTripsAsync()
    {
        await Shell.Current.GoToAsync("//businesstrips");
    }
    [RelayCommand]
    private async Task AbsenceTapped(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            await Shell.Current.GoToAsync("AbsenceDetailsPage", new Dictionary<string, object>
            {
                ["Absence"] = absence
            });
        }
    }
    [RelayCommand]
    private async Task BusinessTripTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            await Shell.Current.GoToAsync("//businesstripdetails", new Dictionary<string, object>
            {
                ["BusinessTrip"] = businessTrip
            });
        }
    }
    private async Task LogoutAsync()
    {
        try
        {
            DatabaseService.User = null;
            await Shell.Current.GoToAsync("//register");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно излизане: {ex.Message}", "OK");
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    //Calendar methods
    [RelayCommand]
    private async Task ItemTapped(BusinessTripViewModel businessTrip)
    {
        if (businessTrip != null)
        {
            await Shell.Current.GoToAsync("//businesstripdetails", new Dictionary<string, object>
            {
                ["BusinessTrip"] = businessTrip
            });
        }
    }
    [RelayCommand]
    public async void SelectAbsence(AbsenceViewModel absence)
    {
        if (absence != null)
        {
            await Shell.Current.GoToAsync("AbsenceDetailsPage", new Dictionary<string, object>
            {
                ["Absence"] = absence
            });
        }
    }
    private async Task NavigateMonth(int direction)
    {
        _currentDate = _currentDate.AddMonths(direction);

        OnPropertyChanged(nameof(CurrentMonthYear));
        GenerateCalendar();
    }

    private void GenerateCalendar()
    {

        CalendarDays.Clear();

        var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
        var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

        // Add empty days for the beginning of the month
        for (int i = 0; i < firstDayOfWeek; i++)
        {
            CalendarDays.Add(new CalendarDay { IsEmpty = true });
        }

        // Add days of the month
        for (int day = 1; day <= lastDayOfMonth.Day; day++)
        {
            var date = new DateTime(_currentDate.Year, _currentDate.Month, day);
            var dayTrips = _allBusinessTrips.Where(t =>
                date >= t.StartDate.Date && date <= t.EndDate.Date).ToList();

            var dayAbsences = _allAbsences.Where(a =>
                date >= a.StartDate.Date && date <= a.StartDate.AddDays(a.DaysCount - 1).Date).ToList();
            var holidayDay = _holidayDays.FirstOrDefault(h => h.Date.Date == date.Date);

            var calendarDay = new CalendarDay
            {
                Date = date,
                DayNumber = day.ToString(),
                IsCurrentMonth = true,
                IsToday = date.Date == DateTime.Today,
                IsHoliday = holidayDay is not null,
                HolidayName = holidayDay?.Name ?? string.Empty,
                IsOfficialHoliday = holidayDay is not null && !holidayDay.IsCustom,
                IsCustomHoliday = holidayDay is not null && holidayDay.IsCustom,
                HasBusinessTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Approved),
                HasPendingTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Pending),
                HasRejectedTrips = dayTrips.Any(t => t.Status == BusinessTripStatus.Rejected),
                BusinessTrips = dayTrips,
                HasApprovedAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Approved),
                HasPendingAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Pending),
                HasRejectedAbsences = dayAbsences.Any(a => a.Status == AbsenceStatus.Rejected),
                Absences = dayAbsences,
            };

            CalendarDays.Add(calendarDay);
        }

        var remainingCells = 42 - CalendarDays.Count;
        for (int i = 0; i < remainingCells; i++)
        {
            CalendarDays.Add(new CalendarDay { IsEmpty = true });
        }
    }

    private async Task AddHolidayAsync()
    {
        if (string.IsNullOrWhiteSpace(HolidayName))
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", "Моля въведете име на почивния ден!", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            _customHolidays.Add(SelectedHolidayDate.Date);
            await _dbService.CreateHolidayDayAsync(new HolidayDay()
            {
                Name = HolidayName,
                Date = SelectedHolidayDate.Date,
                IsCustom = true
            });
            _holidayDays = new ObservableCollection<HolidayDay>(await _dbService.GetAllHolidayDaysAsync());
            GenerateCalendar();
            IsHolidayDialogVisible = false;
            HolidayName = string.Empty;
            await Application.Current.MainPage.DisplayAlert("Успех", "Почивния ден бе успешно добавен!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно добавяне на почивен ден: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteCustomHolidayAsync()
    {
        if (SelectedDay == null || !SelectedDay.IsCustomHoliday)
            return;

        try
        {
            IsBusy = true;
            _customHolidays.Remove(SelectedDay.Date.Date);
            GenerateCalendar();
            IsDaySelected = false;
            await Application.Current.MainPage.DisplayAlert("Success", "Почивният ден бе успешно изтрит!", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Грешка", $"Неуспешно изтриване: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SelectDay(CalendarDay day)
    {
        if (day == null || day.IsEmpty || !day.IsCurrentMonth)
            return;
        if (SelectedDay is not null)
        {
            SelectedDay.IsSelected = false;
            int index = CalendarDays.IndexOf(SelectedDay);
            if (index >= 0)
            {
                CalendarDays[index] = SelectedDay;
            }
        }
        {
            day.IsSelected = true;
            int index = CalendarDays.IndexOf(day);
            if (index >= 0)
            {
                CalendarDays[index] = day;
            }
        }
        SelectedDay = day;
        IsDaySelected = true;
        SelectedDayTitle = day.Date.ToString("dddd, MMMM dd, yyyy");

        // Update business trips for selected day
        SelectedDayTrips.Clear();
        foreach (var trip in day.BusinessTrips)
        {
            SelectedDayTrips.Add(new BusinessTripViewModel(trip));
        }

        // Update absences for selected day
        SelectedDayAbsences.Clear();
        foreach (var absence in day.Absences)
        {
            SelectedDayAbsences.Add(new AbsenceViewModel(absence));
        }
    }

    private async Task NavigateToUsersAsync()
    {
        await Shell.Current.GoToAsync("//AdminUsersPage");
    }

    private async Task NavigateToAbsencesAsync()
    {
        await Shell.Current.GoToAsync("//AdminAllAbsencesPage");
    }

    private async Task NavigateToTripsAsync()
    {
        await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
    }

}

