using BusinessLayer.Entities;

namespace DataLayer.Interfaces.Repository;

public interface IBusinessTripContext
{
    Task<bool> Create(BusinessTrip businessTrip);
    Task<List<BusinessTrip>> GetAll();
    Task<BusinessTrip> GetById(int id);
    Task<List<BusinessTrip>> GetByUserId(string userId);
    Task<bool> Update(BusinessTrip businessTrip);
    Task<bool> Delete(int businessTripId);
}