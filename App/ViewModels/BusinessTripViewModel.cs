using BusinessLayer;

namespace App.ViewModels
{
    public class BusinessTripViewModel
    {
        private readonly BusinessTrip _trip;

        public BusinessTripViewModel(BusinessTrip trip)
        {
            _trip = trip;
        }

        public int Id => _trip.Id;
        public BusinessTripStatus Status
        {
            get => _trip.Status;
            set => _trip.Status = value;
        }

        public DateTime StartDate => _trip.StartDate;
        public DateTime EndDate => _trip.EndDate;
        public string UserId => _trip.UserId;
        public string UserName => _trip.UserFullName ?? "Неизвестен потребител";
        public string UserFullName => _trip.UserFullName;

        public int IssueId => _trip.IssueId;

        public string ProjectName => _trip.ProjectName;
        public string CarTripDestination => _trip.CarTripDestination;
        public string Destination => _trip.CarTripDestination;
        public string Task => _trip.Task ?? "Няма посочена задача";

        public decimal Wage => _trip.Wage;
        public decimal AccommodationMoney => _trip.AccommodationMoney;
        public string CreatedDate => _trip.Created.ToString("MM/dd/yyyy");

        public int Days => (int)(_trip.EndDate - _trip.StartDate).TotalDays + 1;

        public string DateRange => $"{_trip.StartDate:MM/dd/yyyy} - {_trip.EndDate:MM/dd/yyyy} ({Days} {(Days == 1 ? "ден" : "дни")})";

        public string DurationText => $"{Days} {(Days == 1 ? "ден" : "дни")}";
        public string CreatedText => $"Заявено на {_trip.Created:MM/dd/yyyy}";

        public string CarModel => _trip.CarModel ?? "Не е посочен модел на автомобила";

        public decimal AdditionalExpences => _trip.AdditionalExpences;
        public string CarBrand => _trip.CarBrand ?? "Не е посочена марка на автомобила";

        public string CarRegistrationNumber => _trip.CarRegistrationNumber ?? "Не е посочен регистрационен номер";

        public string CarOwnership => _trip.CarOwnership switch
        {
            BusinessLayer.CarOwnerShip.Personal => "Личен автомобил",
            BusinessLayer.CarOwnerShip.Company => "Фирмен автомобил",
            BusinessLayer.CarOwnerShip.Rental => "Нает автомобил",
            _ => "Неизвестно"
        };

        public string CarUsagePerHundredKm => _trip.CarUsagePerHundredKm.ToString("F2") + " л/100км";

        public string PricePerLiter => _trip.PricePerLiter.ToString("F2") + " лв/л";

        public string ExpensesResponsibility => _trip.ExpensesResponsibility ?? "Не е посочено";
        public bool CanChangeStatus => Status == BusinessTripStatus.Pending;

        public string StatusText => _trip.Status switch
        {
            BusinessTripStatus.Pending => "В очакване",
            BusinessTripStatus.Approved => "Одобрен",
            BusinessTripStatus.Rejected => "Отхвърлен",
            _ => "Неизвестен"
        };

        public Color StatusColor => _trip.Status switch
        {
            BusinessTripStatus.Pending => Colors.Orange,
            BusinessTripStatus.Approved => Colors.Green,
            BusinessTripStatus.Rejected => Colors.Red,
            _ => Colors.Gray
        };
        public BusinessTrip BusinessTrip => _trip;
        public static BusinessTrip ToBusinessTrip(BusinessTripViewModel viewModel)
        {
            return new BusinessTrip
            {
                Id = viewModel.Id,
                Status = viewModel.Status,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                UserId = viewModel.UserId,
                UserFullName = viewModel.UserFullName,
                ProjectName = viewModel.ProjectName,
                CarTripDestination = viewModel.Destination,
                Task = viewModel.Task,
                Wage = viewModel.Wage,
                AccommodationMoney = viewModel.AccommodationMoney,
                Created = DateTime.Parse(viewModel.CreatedDate), // or keep original if available
                TotalDays = (byte)viewModel.Days,

                CarModel = viewModel.CarModel == "Не е посочен модел на автомобила" ? string.Empty : viewModel.CarModel,
                AdditionalExpences = viewModel.AdditionalExpences,
                CarBrand = viewModel.CarBrand == "Не е посочена марка на автомобила" ? null : viewModel.CarBrand,
                CarRegistrationNumber = viewModel.CarRegistrationNumber == "Не е посочен регистрационен номер" ? null : viewModel.CarRegistrationNumber,

                CarOwnership = ParseCarOwnership(viewModel.CarOwnership),
                CarUsagePerHundredKm = float.Parse(viewModel.CarUsagePerHundredKm.Replace(" л/100км", "")),
                PricePerLiter = double.Parse(viewModel.PricePerLiter.Replace(" лв/л", "")),
                ExpensesResponsibility = viewModel.ExpensesResponsibility == "Не е посочено" ? null : viewModel.ExpensesResponsibility,

                DepartureDate = viewModel.StartDate,
                DateOfArrival = viewModel.EndDate,
                IssueDate = viewModel.CreatedDate == "" ? DateTime.Now : DateTime.Parse(viewModel.CreatedDate) // fallback
            };
        }
        private static CarOwnerShip ParseCarOwnership(string text)
        {
            return text switch
            {
                "Личен автомобил" => CarOwnerShip.Personal,
                "Фирмен автомобил" => CarOwnerShip.Company,
                "Нает автомобил" => CarOwnerShip.Rental,
                _ => CarOwnerShip.Personal
            };
        }
    }
}

