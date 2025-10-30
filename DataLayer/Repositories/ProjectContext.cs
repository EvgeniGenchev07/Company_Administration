using BusinessLayer.Entities;
using DataLayer.Persistence;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace DataLayer.Repositories
{
    public class ProjectContext
    {
        private readonly EapDbContext _eapDbContext;

        public ProjectContext(EapDbContext eapDbContext)
        {
            _eapDbContext = eapDbContext ?? throw new ArgumentNullException(nameof(eapDbContext));
        }

        public bool Create(Project project)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "INSERT INTO Project (name, description, startDate, endDate) " +
                        "VALUES (@name, @description, @startDate, @endDate)",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@name", project.Name);
                    command.Parameters.AddWithValue("@description", project.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@startDate", project.StartDate);
                    command.Parameters.AddWithValue("@endDate", project.EndDate);

                    int rows = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _eapDbContext.Close();
                    throw new Exception("Error creating project in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public List<Project> GetAll()
        {
            var projects = new List<Project>();
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM Project ORDER BY startDate DESC", _eapDbContext.Connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projects.Add(new Project
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                StartDate = Convert.ToDateTime(reader["startDate"]),
                                EndDate = Convert.ToDateTime(reader["endDate"]),
                                Users = new List<User>() // can be populated later if needed
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
                    _eapDbContext.Close();
                }
                return projects;
            }
            throw new Exception("Database connection is not established.");
        }

        public Project? GetById(int id)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM Project WHERE id = @id", _eapDbContext.Connection);
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Update(Project project)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE Project SET name = @name, description = @description, " +
                        "startDate = @startDate, endDate = @endDate WHERE id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", project.Id);
                    command.Parameters.AddWithValue("@name", project.Name);
                    command.Parameters.AddWithValue("@description", project.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@startDate", project.StartDate);
                    command.Parameters.AddWithValue("@endDate", project.EndDate);

                    int rows = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _eapDbContext.Close();
                    throw new Exception("Error updating project in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Delete(int id)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("DELETE FROM Project WHERE id = @id", _eapDbContext.Connection);
                    command.Parameters.AddWithValue("@id", id);

                    int rows = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    _eapDbContext.Close();
                    throw new Exception("Error deleting project from the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }
    }
}
