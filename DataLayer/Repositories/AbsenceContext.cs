using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer.Persistence;

namespace DataLayer.Repositories
{
    public class AbsenceContext
    {
        private readonly EapDbContext _eapDbContext;

        public AbsenceContext(EapDbContext eapDbContext)
        {
            _eapDbContext = eapDbContext ?? throw new ArgumentNullException(nameof(eapDbContext));
        }

        public bool Create(Absence absence)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "INSERT INTO Absence (type, daysCount, daysTaken, created, status, startDate, userId) " +
                        "VALUES (@type, @daysCount, @daysTaken, @created, @status, @startDate, @userId)",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@type", (int)absence.Type);
                    command.Parameters.AddWithValue("@daysCount", absence.DaysCount);
                    command.Parameters.AddWithValue("@daysTaken", absence.DaysTaken);
                    command.Parameters.AddWithValue("@created", DateTime.Now.Date);
                    command.Parameters.AddWithValue("@status", (int)absence.Status);
                    command.Parameters.AddWithValue("@startDate", absence.StartDate);
                    command.Parameters.AddWithValue("@userId", absence.UserId);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating absence in the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Update(Absence absence)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "UPDATE Absence SET type = @type, daysCount = @daysCount, daysTaken = @daysTaken, " +
                        "status = @status, startDate = @startDate WHERE id = @id",
                        _eapDbContext.Connection);
                    command.Parameters.AddWithValue("@type", (int)absence.Type);
                    command.Parameters.AddWithValue("@daysCount", absence.DaysCount);
                    command.Parameters.AddWithValue("@daysTaken", absence.DaysTaken);
                    command.Parameters.AddWithValue("@status", (int)absence.Status);
                    command.Parameters.AddWithValue("@startDate", absence.StartDate);
                    command.Parameters.AddWithValue("@id", absence.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating absence in the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Delete(int absenceId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "DELETE FROM Absence WHERE id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", absenceId);

                    int rowsAffected = command.ExecuteNonQuery();
                    _eapDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error deleting absence from the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public Absence GetById(int absenceId)
        {
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM Absence WHERE id = @id",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@id", absenceId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established or absence not found.");
        }

        public List<Absence> GetByUserId(string userId)
        {
            var absences = new List<Absence>();
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT * FROM Absence WHERE userId = @userId ORDER BY created DESC",
                        _eapDbContext.Connection);

                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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
                    _eapDbContext.Close();
                    return absences;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving absences for user from the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
        public List<Absence> GetAll()
        {
            var absences = new List<Absence>();
            if (_eapDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlConnector.MySqlCommand(
                        "SELECT a.*, u.name FROM Absence a JOIN User u on u.id=a.userId ORDER BY a.created DESC",
                        _eapDbContext.Connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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
                    _eapDbContext.Close();
                    return absences;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving absences from the database.", ex);
                }
                finally
                {
                    _eapDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }
    }
}