using App.Services;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace App.PageModels
{
    public partial class RequestPageModel : ObservableObject
    {
        private readonly DatabaseService _dbService;

        [ObservableProperty]
        string _organization = "ЕНЕРГИЙНА АГЕНЦИЯ-ПЛОВДИВ";

        [ObservableProperty]
        string _documentNumber = "16";

        [ObservableProperty]
        DateTime _documentDate = DateTime.Now;

        [ObservableProperty]
        string _project = "Стопанска дейност";

        [ObservableProperty]
        string _employeeName = "Десислава Николова";

        [ObservableProperty]
        string _destinationCity = "София";

        [ObservableProperty]
        string _returnCity = "Пловдив";

        [ObservableProperty]
        int _durationDays = 1;

        [ObservableProperty]
        DateTime _tripStartDate = DateTime.Now;

        [ObservableProperty]
        DateTime _tripEndDate = DateTime.Now;

        [ObservableProperty]
        string _task = "участие в работни срещи";

        [ObservableProperty]
        decimal _dailyAllowanceRate = 40.00m;

        [ObservableProperty]
        decimal _accommodationAllowanceRate = 0.00m;

        [ObservableProperty]
        decimal _additionalExpenses = 0.00m;

        [ObservableProperty]
        string _vehicleType = "Opel";

        [ObservableProperty]
        string _vehicleModel = "Zafira";

        [ObservableProperty]
        string _fuelType = "D";

        [ObservableProperty]
        decimal _fuelConsumption = 10.00m;

        [ObservableProperty]
        decimal _totalExpenses = 40.00m;

        // Dropdown options
        [ObservableProperty]
        ObservableCollection<string> _cities = new()
        {
            "София", "Пловдив", "Варна", "Бургас", "Русе",
            "Стара Загора", "Плевен", "Велико Търново", "Сливен"
        };

        [ObservableProperty]
        ObservableCollection<string> _fuelTypes = new()
        {
            "Дизел", "Бензин", "Газ", "Ток"
        };

        [ObservableProperty]
        ObservableCollection<string> _filteredCities = new();

        [ObservableProperty]
        private bool _isBusy;

        public RequestPageModel(DatabaseService dbService)
        {
            _dbService = dbService;
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (App.User != null)
            {
                EmployeeName = App.User.Name;
            }
        }

        // Commands
        [RelayCommand]
        private async Task Back()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        [RelayCommand]
        private void FilterCities(string searchText)
        {


            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var city in Cities)
                {
                    FilteredCities.Add(city);
                }
            }
            else
            {
                foreach (var city in Cities.Where(c =>
                    c.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                {
                    FilteredCities.Add(city);
                }
            }
        }

        [RelayCommand]
        private void CalculateTotalExpenses()
        {
            TotalExpenses = DailyAllowanceRate * DurationDays +
                           AccommodationAllowanceRate * DurationDays +
                           AdditionalExpenses;
        }

        [RelayCommand]
        private async Task SubmitRequest()
        {
            // Validate form
            if (string.IsNullOrWhiteSpace(EmployeeName) ||
                string.IsNullOrWhiteSpace(DestinationCity))
            {
                await Shell.Current.DisplayAlert("Грешка", "Моля, попълнете всички задължителни полета", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                var businessTrip = new BusinessTrip
                {
                    Status = BusinessTripStatus.Pending,
                    IssueDate = DocumentDate,
                    ProjectName = Project,
                    UserFullName = EmployeeName,
                    Task = Task,
                    StartDate = TripStartDate,
                    EndDate = TripEndDate,
                    TotalDays = (byte)DurationDays,
                    CarOwnership = CarOwnerShip.Personal, // Default to personal car
                    Wage = DailyAllowanceRate,
                    AccommodationMoney = AccommodationAllowanceRate,
                    CarBrand = VehicleType,
                    CarRegistrationNumber = "", // Will be filled by user
                    CarTripDestination = DestinationCity,
                    DateOfArrival = TripStartDate,
                    CarModel = VehicleModel,
                    AdditionalExpences = AdditionalExpenses,
                    CarUsagePerHundredKm = (float)FuelConsumption,
                    PricePerLiter = 2.50, // Default price
                    DepartureDate = TripEndDate,
                    ExpensesResponsibility = "Служител",
                    Created = DateTime.Now,
                    UserId = App.User?.Id ?? string.Empty
                };

                var success = await _dbService.CreateBusinessTripAsync(businessTrip);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Успех", "Командировката е запазена успешно", "OK");

                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Грешка", "Неуспешно запазване на командировката", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Грешка", $"Възникна грешка: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        partial void OnTripStartDateChanged(DateTime value)
        {
            TripStartDate = value;
            TripEndDate = value.AddDays(DurationDays);
            CalculateTotalExpenses();
        }

        partial void OnDurationDaysChanged(int value)
        {
            TripEndDate = TripStartDate.AddDays(value);
            CalculateTotalExpenses();
        }
        partial void OnAdditionalExpensesChanged(decimal value)
        {
            CalculateTotalExpenses();
        }

        partial void OnDailyAllowanceRateChanged(decimal value)
        {
            CalculateTotalExpenses();
        }

        partial void OnAccommodationAllowanceRateChanged(decimal value)
        {
            CalculateTotalExpenses();
        }
    }
}