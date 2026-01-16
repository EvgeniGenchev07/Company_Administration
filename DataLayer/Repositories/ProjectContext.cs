using BusinessLayer.Entities;
using DataLayer.Persistence;
using MySqlConnector;
using System;
using System.Collections.Generic;
using DataLayer.Interfaces.Repository;

namespace DataLayer.Repositories
{
    public class ProjectContext(CompanyAdministrationDbContext companyAdministrationDbContext)
        : IProjectContext
    {
        private readonly CompanyAdministrationDbContext _companyAdministrationDbContext = companyAdministrationDbContext ?? throw new ArgumentNullException(nameof(companyAdministrationDbContext));

        public async Task<bool> Create(Project project)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "INSERT INTO Project (name, description, startDate, endDate) " +
                        "VALUES (@name, @description, @startDate, @endDate)",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@name", project.Name);
                    command.Parameters.AddWithValue("@description", project.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@startDate", project.StartDate);
                    command.Parameters.AddWithValue("@endDate", project.EndDate);

                    int rows = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error creating project in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<Project> GetById(int id)
                                            {
                                                if (_companyAdministrationDbContext.IsConnect())
                                                {
                                                    try
                                                    {
                                                        var command = new MySqlCommand("SELECT * FROM Project WHERE id = @id", _companyAdministrationDbContext.Connection);
                                                        command.Parameters.AddWithValue("@id", id);
                                    
                                                        using (var reader = await command.ExecuteReaderAsync())
                                                        {
                                                            if (await reader.ReadAsync())
                                                            {
                                                                var project = new Project
                                                                {
                                                                    Id = Convert.ToInt32(reader["id"]),
                                                                    Name = reader["name"].ToString(),
                                                                    Description = reader["description"].ToString(),
                                                                    StartDate = Convert.ToDateTime(reader["startDate"]),
                                                                    EndDate = Convert.ToDateTime(reader["endDate"]),
                                                                    Users = new List<User>()
                                                                };
                                                                return project;
                                                            }
                                                        }
                                                        return null;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        throw new Exception("Error retrieving project by ID from the database.", ex);
                                                    }
                                                    finally
                                                    {
                                                        _companyAdministrationDbContext.Close();
                                                    }
                                                }
                                                throw new Exception("Database connection is not established.");
                                            }
        
        public async Task<List<Project>> GetAll()
        {
            var projects = new List<Project>();
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM Project ORDER BY startDate DESC", _companyAdministrationDbContext.Connection);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            projects.Add(new Project
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                StartDate = Convert.ToDateTime(reader["startDate"]),
                                EndDate = Convert.ToDateTime(reader["endDate"]),
                                Users = new List<User>()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving projects from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
                return projects;
            }
            throw new Exception("Database connection is not established.");
        }
        
        public async Task<bool> Update(Project project)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE Project SET name = @name, description = @description, " +
                        "startDate = @startDate, endDate = @endDate WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", project.Id);
                    command.Parameters.AddWithValue("@name", project.Name);
                    command.Parameters.AddWithValue("@description", project.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@startDate", project.StartDate);
                    command.Parameters.AddWithValue("@endDate", project.EndDate);

                    int rows = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error updating project in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<bool> Delete(int id)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("DELETE FROM Project WHERE id = @id", _companyAdministrationDbContext.Connection);
                    command.Parameters.AddWithValue("@id", id);

                    int rows = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error deleting project from the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }
    }
}
