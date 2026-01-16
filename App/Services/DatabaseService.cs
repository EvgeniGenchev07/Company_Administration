using BusinessLayer;
using DataLayer;
using System.Text;
using System.Text.Json;
namespace App.Services
{
    public class DatabaseService
    {
        private readonly AuthenticationContext _authenticationContext;
        private readonly BusinessTripContext _businessTripContext;
        private readonly HolidayDayContext _holidayDayContext;
        private readonly UserContext _userContext;
        private readonly AbsenceContext _absenceContext;

        public DatabaseService(AuthenticationContext authenticationContext, BusinessTripContext businessTripContext,
            HolidayDayContext holidayDayContext, UserContext userContext, AbsenceContext absenceContext)
        {
            _authenticationContext = authenticationContext ?? throw new ArgumentNullException(nameof(authenticationContext));
            _businessTripContext = businessTripContext ?? throw new ArgumentNullException(nameof(businessTripContext));
            _holidayDayContext = holidayDayContext ?? throw new ArgumentNullException(nameof(holidayDayContext));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _absenceContext = absenceContext ?? throw new ArgumentNullException(nameof(absenceContext));
        }
        public async Task<User> UserLogin(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }
            User user = _authenticationContext.Authenticate(email, password);
            return user;
        }

        public async Task<bool> UpdateUserAbsenceBalance()
        {
            try
            {
                await _userContext.UpdateAbsenceBalancesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<BusinessTrip>> GetAllBusinessTripsAsync()
        {
            var businessTrips = _businessTripContext.GetAll();
            return businessTrips ?? new List<BusinessTrip>();
        }

        public async Task<bool> DeleteBusinessTripAsync(int id)
        {

            if (id <= 0)
            {
                return false;
            }

            if (_businessTripContext.Delete(id))
            {
                User user = App.User;
                if (user.BusinessTrips != null)
                {
                    var tripToRemove = user.BusinessTrips.FirstOrDefault(t => t.Id == id);
                    if (tripToRemove != null)
                    {
                        user.BusinessTrips.Remove(tripToRemove);
                    }
                }
                return true;
            }
            return false;

        }
        public async Task<List<HolidayDay>> GetAllHolidayDaysAsync()
        {
            var holidayDays = _holidayDayContext.GetAll();
            return holidayDays ?? new List<HolidayDay>();
        }

        public async Task<List<BusinessTrip>> GetUserBusinessTripsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<BusinessTrip>();
            }
            var businessTrips = _businessTripContext.GetByUserId(userId);
            return businessTrips ?? new List<BusinessTrip>();
        }

        public async Task<List<Absence>> GetUserAbsencesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Absence>();
            }

            var absences = _absenceContext.GetByUserId(userId);
            return absences ?? new List<Absence>();
        }
        public int CalculateHolidays(DateTime start,  int days)
        {
            var holidays = _holidayDayContext.GetAll();
            int holidaysCount = holidays.Count(h => h.Date >= start && h.Date < start.AddDays(days));
            for (int i = 0; i < days; i++)
            {
                DateTime dateTime = start.AddDays(i);
                if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday) holidaysCount++;
            }
            return holidaysCount;
        }
        public async Task<bool> CreateAbsenceAsync(Absence absence)
        {
            if (absence == null)
            {
                return false;
            }
            User user = _userContext.GetById(absence.UserId);
            if (user == null)
            {
                return false;
            }
            if (absence.Type == AbsenceType.PersonalLeave)
            {
                user.AbsenceDays -= absence.DaysTaken;
                
                if (!_userContext.Update(user))
                {
                    return false;
                }
            }
            if (_absenceContext.Create(absence))
            {
                App.User.Absences?.Add(absence);
                App.User.AbsenceDays -= absence.DaysTaken;
                MessagingCenter.Send<DatabaseService>(this, "AbsenceCreated");
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> CreateBusinessTripAsync(BusinessTrip businessTrip)
        {
            if (businessTrip == null || string.IsNullOrEmpty(businessTrip.UserId) || string.IsNullOrEmpty(businessTrip.ProjectName))
            {
                return false;
            }

            if (_businessTripContext.Create(businessTrip))
            {
                App.User.BusinessTrips = _businessTripContext.GetByUserId(businessTrip.UserId);
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<HolidayDay> CreateHolidayDayAsync(HolidayDay holidayDay)
        {
            if (holidayDay == null)
            {
                return null;
            }
            return _holidayDayContext.Create(holidayDay) ? holidayDay : null;
        }


        public async Task<bool> CancelAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            return _absenceContext.Delete(absenceId);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = _userContext.ReadAll();
            return users ?? new List<User>();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return false;
            }
            user.Id = Guid.NewGuid().ToString();
            return _userContext.Create(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return false;
            }
            return _userContext.Update(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return _userContext.Delete(userId);
        }

        public async Task<List<Absence>> GetAllAbsencesAsync()
        {
            var absences = _absenceContext.GetAll();
            return absences ?? new List<Absence>();
        }

        public async Task<bool> ApproveAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            Absence absence = _absenceContext.GetById(absenceId);
            if (absence == null)
            {
                return false;
            }
            absence.Status = AbsenceStatus.Approved;
            return _absenceContext.Update(absence);
        }

        public async Task<bool> RejectAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            Absence absence = _absenceContext.GetById(absenceId);
            if (absence == null)
            {
                return false;
            }
            absence.Status = AbsenceStatus.Rejected;
            User user = _userContext.GetById(absence.UserId);
            if (user == null)
            {
                return false;
            }
            if (absence.Type == AbsenceType.PersonalLeave)
            {
                user.AbsenceDays += absence.DaysTaken;
                if (!_userContext.Update(user))
                {
                    return false;
                }
            }
            return _absenceContext.Update(absence);
        }

        public async Task<bool> ApproveBusinessTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                return false;
            }
            BusinessTrip businessTrip = _businessTripContext.GetById(tripId);
            if (businessTrip == null)
            {
                return false;
            }
            businessTrip.Status = BusinessTripStatus.Approved;
            return _businessTripContext.Update(businessTrip);
        }

        public async Task<bool> RejectBusinessTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                return false;
            }
            BusinessTrip businessTrip = _businessTripContext.GetById(tripId);
            if (businessTrip == null)
            {
                return false;
            }
            businessTrip.Status = BusinessTripStatus.Rejected;
            return _businessTripContext.Update(businessTrip);
        }

    }
}
