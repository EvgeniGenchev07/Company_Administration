using BusinessLayer.Entities;

namespace DataLayer.Interfaces.Repository;

public interface IProjectContext
{
    Task<bool> Create(Project project);
    Task<List<Project>> GetAll();
    Task<Project> GetById(int id);
    Task<bool> Update(Project project);
    Task<bool> Delete(int projectId);
}