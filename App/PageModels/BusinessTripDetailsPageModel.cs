using App.Pages;
using App.Services;
using App.ViewModels;
using BusinessLayer;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;

namespace App.PageModels
{
    public partial class BusinessTripDetailsPageModel : ObservableObject
    {
        private readonly DatabaseService _dbService;
        private readonly HttpClient _httpClient = new HttpClient();

        [ObservableProperty]
        private BusinessTrip _businessTrip;


        [ObservableProperty]
        private bool _canEdit;


        [ObservableProperty]
        private bool _isEditing;

        private BusinessTrip _originalBusinessTrip;
        [ObservableProperty]
        private decimal _additionalExpences;
        [ObservableProperty]
        private decimal _wage;
        [ObservableProperty]
        private decimal _accommodationMoney;
        private bool isReturning;
        public string EditButtonText => IsEditing ? "Запази" : "Редактирай";
        public decimal TotalExpenses => Wage * BusinessTrip.TotalDays + AccommodationMoney * BusinessTrip.TotalDays + AdditionalExpences;
        public BusinessTripDetailsPageModel(){}
        public BusinessTripDetailsPageModel(DatabaseService service)
        {
            BusinessTrip = BusinessTripDetailsPage.SelectedBusinessTrip.BusinessTrip;
            _dbService = service;
            _originalBusinessTrip = CloneBusinessTrip(BusinessTrip);
            
        }
        internal async void LoadData()
        {
            BusinessTrip = BusinessTripDetailsPage.SelectedBusinessTrip.BusinessTrip;
            _originalBusinessTrip = CloneBusinessTrip(BusinessTrip);
            AdditionalExpences = BusinessTrip.AdditionalExpences;
            Wage = BusinessTrip.Wage;
            AccommodationMoney = BusinessTrip.AccommodationMoney;
            IsEditing = false;
            isReturning = false;
            CalculateTotalExpenses();
            UpdateCanEdit();
            OnPropertyChanged(nameof(EditButtonText));
        }
        partial void OnBusinessTripChanged(BusinessTrip value)=>CalculateTotalExpenses();
        partial void OnAdditionalExpencesChanged(decimal value)=> CalculateTotalExpenses();
        partial void OnWageChanged(decimal value) => CalculateTotalExpenses();
        partial void OnAccommodationMoneyChanged(decimal value) => CalculateTotalExpenses();
        async partial void OnIsEditingChanged(bool value)
        {
            if(!value&&!isReturning) await Save();
        }
        
        private void CalculateTotalExpenses()
        {
            if (BusinessTrip != null)
            {
                OnPropertyChanged(nameof(TotalExpenses));
            }
        }

        private void UpdateCanEdit()
        {
            CanEdit = BusinessTrip?.Status == BusinessTripStatus.Pending;
        }

        private BusinessTrip CloneBusinessTrip(BusinessTrip original)
        {
            return new BusinessTrip
            {
                Id = original.Id,
                IssueId = original.IssueId,
                Status = original.Status,
                IssueDate = original.IssueDate,
                ProjectName = original.ProjectName,
                UserFullName = original.UserFullName,
                Task = original.Task,
                StartDate = original.StartDate,
                EndDate = original.EndDate,
                TotalDays = original.TotalDays,
                CarOwnership = original.CarOwnership,
                Wage = original.Wage,
                AccommodationMoney = original.AccommodationMoney,
                CarBrand = original.CarBrand,
                CarRegistrationNumber = original.CarRegistrationNumber,
                CarTripDestination = original.CarTripDestination,
                DateOfArrival = original.DateOfArrival,
                CarModel = original.CarModel,
                AdditionalExpences = original.AdditionalExpences,
                CarUsagePerHundredKm = original.CarUsagePerHundredKm,
                PricePerLiter = original.PricePerLiter,
                DepartureDate = original.DepartureDate,
                ExpensesResponsibility = original.ExpensesResponsibility,
                Created = original.Created
            };
        }
        
        [RelayCommand]
        private async Task Cancel()
        {
            if(IsEditing)  await CancelEdit();
            if (!IsEditing)
            {
                if (App.User.Role == Role.Admin)
                {
                    await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
                }
                else
                {
                    await Shell.Current.GoToAsync("//businesstrips");
                }
            }
        }

        [RelayCommand]
        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
            OnPropertyChanged(nameof(EditButtonText));
        }
        [RelayCommand]
        private async Task Delete()
        {
            if (BusinessTrip == null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Потвърждение",
                "Сигурни ли сте, че искате да изтриете тази командировка? Това действие не може да бъде отменено.",
                "Да, изтрий",
                "Отказ");

            if (confirm)
            {
                try
                {
                    var success = await _dbService.DeleteBusinessTripAsync(BusinessTrip.Id);

                    if (success)
                    {
                        await Shell.Current.DisplayAlert(
                            "Успех",
                            "Командировката беше изтрита успешно!",
                            "OK");

                        isReturning = true;
                        ToggleEdit();

                        if (App.User.Role == Role.Admin)
                        {
                            await Shell.Current.GoToAsync("//AdminAllBusinessTripsPage");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync("//businesstrips");
                        }
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert(
                            "Грешка",
                            "Неуспешно изтриване на командировката.",
                            "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert(
                        "Грешка",
                        $"Възникна грешка при изтриване: {ex.Message}",
                        "OK");
                }
            }
        }
        private async Task Save()
        {
                var result = await Shell.Current.DisplayAlert("Потвърждение",
                    "Искате ли да запазите промените?", "Да", "Не");

                if (result)
                {
                    CalculateTotalExpenses();
                    BusinessTrip.AccommodationMoney = AccommodationMoney;
                    BusinessTrip.AdditionalExpences = AdditionalExpences;
                    BusinessTrip.Wage = Wage;
                    await _dbService.CreateBusinessTripAsync(BusinessTrip);
                    if(IsEditing) ToggleEdit();
                    await Shell.Current.DisplayAlert("Успех",
                        "Промените са запазени успешно!", "OK");
                }
        }

        [RelayCommand]
        private async Task CancelEdit()
        {
            if (IsEditing)
            {
                var result = await Shell.Current.DisplayAlert("Потвърждение",
                    "Искате ли да отмените промените?", "Да", "Не");

                if (result)
                {
                    BusinessTrip = CloneBusinessTrip(_originalBusinessTrip);
                    isReturning = true;
                    IsEditing = false;
                }
            }
        }


        
    }
}