using BusinessLayer;
using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer;
using DataLayer.Repositories;
using ServiceLayer.Services;
using System.Text;
using System.Text.Json;
namespace ServiceLayer.Services
{
    public class DatabaseService
    {
        private readonly AuthenticationService _authenticationService;
        private readonly BusinessTripContext _businessTripContext;
        private readonly HolidayDayContext _holidayDayContext;
        private readonly UserContext _userContext;
        private readonly AbsenceContext _absenceContext;
        internal static User User;

        public DatabaseService(AuthenticationService authenticationContext, BusinessTripContext businessTripContext,
            HolidayDayContext holidayDayContext, UserContext userContext, AbsenceContext absenceContext)
        {
            _authenticationService = authenticationContext ?? throw new ArgumentNullException(nameof(authenticationContext));
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
            User user = await  _authenticationService.Authenticate(email, password);
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
            var businessTrips = await _businessTripContext.GetAll();
            return businessTrips ?? new List<BusinessTrip>();
        }

        public async Task<bool> DeleteBusinessTripAsync(int id)
        {

            if (id <= 0)
            {
                return false;
            }

            if (await _businessTripContext.Delete(id))
            {
                if (User.BusinessTrips != null)
                {
                    var tripToRemove = User.BusinessTrips.FirstOrDefault(t => t.Id == id);
                    if (tripToRemove != null)
                    {
                        User.BusinessTrips.Remove(tripToRemove);
                    }
                }
                return true;
            }
            return false;

        }
        public async Task<List<HolidayDay>> GetAllHolidayDaysAsync()
        {
            var holidayDays = await _holidayDayContext.GetAll();
            return holidayDays ?? new List<HolidayDay>();
        }

        public async Task<List<BusinessTrip>> GetUserBusinessTripsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<BusinessTrip>();
            }
            var businessTrips = await _businessTripContext.GetByUserId(userId);
            return businessTrips ?? new List<BusinessTrip>();
        }

        public async Task<List<Absence>> GetUserAbsencesAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Absence>();
            }

            var absences = await _absenceContext.GetByUserId(userId);
            return absences ?? new List<Absence>();
        }
        public async Task<int> CalculateHolidays(DateTime start,  int days)
        {
            var holidays = await _holidayDayContext.GetAll();
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
            User user =await _userContext.GetById(absence.UserId);
            if (user == null)
            {
                return false;
            }
            if (absence.Type == AbsenceType.PersonalLeave)
            {
                user.AbsenceDays -= absence.DaysTaken;
                
                if (! await _userContext.Update(user))
                {
                    return false;
                }
            }
            if (await _absenceContext.Create(absence))
            {
                User.Absences?.Add(absence);
                User.AbsenceDays -= absence.DaysTaken;
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

            if (await _businessTripContext.Create(businessTrip))
            {
                User.BusinessTrips = await _businessTripContext.GetByUserId(businessTrip.UserId);
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
            return await _holidayDayContext.Create(holidayDay) ? holidayDay : null;
        }


        public async Task<bool> CancelAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            return await _absenceContext.Delete(absenceId);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _userContext.GetAll();
            return users ?? new List<User>();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return false;
            }
            user.Id = Guid.NewGuid().ToString();
            return await _userContext.Create(user);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
            {
                return false;
            }
            return await _userContext.Update(user);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return await _userContext.Delete(userId);
        }

        public async Task<List<Absence>> GetAllAbsencesAsync()
        {
            var absences = await _absenceContext.GetAll();
            return absences ?? new List<Absence>();
        }

        public async Task<bool> ApproveAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            Absence absence = await _absenceContext.GetById(absenceId);
            if (absence == null)
            {
                return false;
            }
            absence.Status = AbsenceStatus.Approved;
            return await _absenceContext.Update(absence);
        }

        public async Task<bool> RejectAbsenceAsync(int absenceId)
        {
            if (absenceId <= 0)
            {
                return false;
            }
            Absence absence = await _absenceContext.GetById(absenceId);
            if (absence == null)
            {
                return false;
            }
            absence.Status = AbsenceStatus.Rejected;
            User user = await _userContext.GetById(absence.UserId);
            if (user == null)
            {
                return false;
            }
            if (absence.Type == AbsenceType.PersonalLeave)
            {
                user.AbsenceDays += absence.DaysTaken;
                if (!await _userContext.Update(user))
                {
                    return false;
                }
            }
            return await _absenceContext.Update(absence);
        }

        public async Task<bool> ApproveBusinessTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                return false;
            }
            BusinessTrip businessTrip = await _businessTripContext.GetById(tripId);
            if (businessTrip == null)
            {
                return false;
            }
            businessTrip.Status = BusinessTripStatus.Approved;
            return await _businessTripContext.Update(businessTrip);
        }

        public async Task<bool> RejectBusinessTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                return false;
            }
            BusinessTrip businessTrip = await _businessTripContext.GetById(tripId);
            if (businessTrip == null)
            {
                return false;
            }
            businessTrip.Status = BusinessTripStatus.Rejected;
            return await _businessTripContext.Update(businessTrip);
        }

    }
}
