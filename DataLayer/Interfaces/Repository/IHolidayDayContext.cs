using BusinessLayer.Entities;

namespace DataLayer.Interfaces.Repository;

public interface IHolidayDayContext
{
    Task<bool> Create(HolidayDay holidayDay);
    Task<List<HolidayDay>> GetAll();
    Task<HolidayDay> GetById(int id);
    Task<bool> Update(HolidayDay holidayDay);
    Task<bool> Delete(int holidayDayId);
}