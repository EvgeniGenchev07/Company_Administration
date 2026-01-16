using BusinessLayer.Entities;

namespace DataLayer.Interfaces.Repository;

public interface IAbsenceContext
{
    Task<bool> Create(Absence absence);
    Task<List<Absence>> GetAll();
    Task<Absence> GetById(int id);
    Task<List<Absence>> GetByUserId(string userId);
    Task<bool> Update(Absence absence);
    Task<bool> Delete(int absenceId);

}