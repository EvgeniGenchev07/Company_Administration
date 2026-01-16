using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer.Interfaces.Repository;
using DataLayer.Persistence;
using MySqlConnector;

namespace DataLayer.Repositories
{
    public class AbsenceContext(CompanyAdministrationDbContext companyAdministrationDbContext)
        : IAbsenceContext
    {
        private readonly CompanyAdministrationDbContext _companyAdministrationDbContext = companyAdministrationDbContext ?? throw new ArgumentNullException(nameof(companyAdministrationDbContext));

        public async Task<bool> Create(Absence absence)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "INSERT INTO Absence (type, daysCount, daysTaken, created, status, startDate, userId) " +
                        "VALUES (@type, @daysCount, @daysTaken, @created, @status, @startDate, @userId)",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@type", (int)absence.Type);
                    command.Parameters.AddWithValue("@daysCount", absence.DaysCount);
                    command.Parameters.AddWithValue("@daysTaken", absence.DaysTaken);
                    command.Parameters.AddWithValue("@created", DateTime.Now.Date);
                    command.Parameters.AddWithValue("@status", (int)absence.Status);
                    command.Parameters.AddWithValue("@startDate", absence.StartDate);
                    command.Parameters.AddWithValue("@userId", absence.UserId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating absence in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
        
        public async Task<Absence> GetById(int absenceId)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "SELECT * FROM Absence WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", absenceId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Absence
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Type = (AbsenceType)Convert.ToInt32(reader["type"]),
                                DaysCount = Convert.ToByte(reader["daysCount"]),
                                Created = Convert.ToDateTime(reader["created"]),
                                DaysTaken = Convert.ToByte(reader["daysTaken"]),
                                Status = (AbsenceStatus)Convert.ToInt32(reader["status"]),
                                StartDate = Convert.ToDateTime(reader["startDate"]),
                                UserId = reader["userId"].ToString()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving absence from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established or absence not found.");
        }

        public async Task<List<Absence>> GetByUserId(string userId)
        {
            var absences = new List<Absence>();
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "SELECT * FROM Absence WHERE userId = @userId ORDER BY created DESC",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            absences.Add(new Absence
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Type = (AbsenceType)Convert.ToInt32(reader["type"]),
                                DaysCount = Convert.ToByte(reader["daysCount"]),
                                DaysTaken = Convert.ToByte(reader["daysTaken"]),
                                Created = Convert.ToDateTime(reader["created"]),
                                Status = (AbsenceStatus)Convert.ToInt32(reader["status"]),
                                StartDate = Convert.ToDateTime(reader["startDate"]),
                                UserId = reader["userId"].ToString()
                            });
                        }
                    }
                    _companyAdministrationDbContext.Close();
                    return absences;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving absences for user from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
        
        public async Task<List<Absence>> GetAll()
        {
            var absences = new List<Absence>();
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "SELECT a.*, u.name FROM Absence a JOIN User u on u.id=a.userId ORDER BY a.created DESC",
                        _companyAdministrationDbContext.Connection);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            absences.Add(new Absence
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Type = (AbsenceType)Convert.ToInt32(reader["type"]),
                                DaysCount = Convert.ToByte(reader["daysCount"]),
                                DaysTaken = Convert.ToByte(reader["daysTaken"]),
                                Created = Convert.ToDateTime(reader["created"]),
                                Status = (AbsenceStatus)Convert.ToInt32(reader["status"]),
                                StartDate = Convert.ToDateTime(reader["startDate"]),
                                UserId = reader["userId"].ToString(),
                                UserName = reader["name"].ToString()
                            });
                        }
                    }
                    _companyAdministrationDbContext.Close();
                    return absences;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving absences from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
        
        public async Task<bool> Update(Absence absence)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE Absence SET type = @type, daysCount = @daysCount, daysTaken = @daysTaken, " +
                        "status = @status, startDate = @startDate WHERE id = @id",
                        _companyAdministrationDbContext.Connection);
                    command.Parameters.AddWithValue("@type", (int)absence.Type);
                    command.Parameters.AddWithValue("@daysCount", absence.DaysCount);
                    command.Parameters.AddWithValue("@daysTaken", absence.DaysTaken);
                    command.Parameters.AddWithValue("@status", (int)absence.Status);
                    command.Parameters.AddWithValue("@startDate", absence.StartDate);
                    command.Parameters.AddWithValue("@id", absence.Id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating absence in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<bool> Delete(int absenceId)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "DELETE FROM Absence WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", absenceId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error deleting absence from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
    }
}