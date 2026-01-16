using BusinessLayer.Entities;

namespace DataLayer.Interfaces.Repository;

public interface IUserContext
{
    Task<bool> Create(User user);
    Task<List<User>> GetAll();
    Task<User> GetById(string id);
    Task<User> GetByEmail(string email);
    Task<bool> Update(User absence);
    Task UpdateAbsenceBalancesAsync();
    Task<bool> Delete(string userId);
}