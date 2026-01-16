using BusinessLayer.Entities;
using BusinessLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IDatabaseService
    {
        Task<User> UserLogin(string email, string password);
        Task<bool> UpdateUserAbsenceBalance();
        Task<List<BusinessTrip>> GetAllBusinessTripsAsync();

        Task<bool> DeleteBusinessTripAsync(int id);
        Task<List<HolidayDay>> GetAllHolidayDaysAsync();

        Task<List<BusinessTrip>> GetUserBusinessTripsAsync(string userId);
        Task<List<Absence>> GetUserAbsencesAsync(string userId);
        Task<int> CalculateHolidays(DateTime start, int days);
        Task<bool> CreateAbsenceAsync(Absence absence);

        Task<bool> CreateBusinessTripAsync(BusinessTrip businessTrip);
        Task<HolidayDay> CreateHolidayDayAsync(HolidayDay holidayDay);
        Task<bool> CancelAbsenceAsync(int absenceId);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user);

        Task<bool> UpdateUserAsync(User user);

        Task<bool> DeleteUserAsync(string userId);

        Task<List<Absence>> GetAllAbsencesAsync();

        Task<bool> ApproveAbsenceAsync(int absenceId);

        Task<bool> RejectAbsenceAsync(int absenceId);

        Task<bool> ApproveBusinessTripAsync(int tripId);

        Task<bool> RejectBusinessTripAsync(int tripId);
    }
}
