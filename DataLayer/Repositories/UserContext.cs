using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer.Interfaces.Repository;
using MySqlConnector;
using DataLayer.Persistence;

namespace DataLayer.Repositories
{
    public class UserContext(CompanyAdministrationDbContext companyAdministrationDbContext)
        : IUserContext
    {
        private readonly CompanyAdministrationDbContext _companyAdministrationDbContext = companyAdministrationDbContext ?? throw new ArgumentNullException(nameof(companyAdministrationDbContext));

        public async Task<List<User>> GetAll()
        {
            var users = new List<User>();

            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    MySqlCommand command = new MySqlCommand("SELECT * FROM User", _companyAdministrationDbContext.Connection);
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Password = reader["Password"].ToString(),
                                Role = (Role)Convert.ToInt32(reader["Role"]),
                                ContractDays = Convert.ToInt32(reader["ContractDays"]),
                                AbsenceDays = Convert.ToInt32(reader["AbsenceDays"])
                            });
                        }
                    }
                    _companyAdministrationDbContext.Close();
                    return users;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading users from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<User> GetById(string userId)
                                {
                                    if (_companyAdministrationDbContext.IsConnect())
                                    {
                                        try
                                        {
                                            var command = new MySqlCommand(
                                                "SELECT * FROM User WHERE Id = @id",
                                                _companyAdministrationDbContext.Connection);
                        
                                            command.Parameters.AddWithValue("@id", userId);

                                            await using var reader = await command.ExecuteReaderAsync();
                                            if (await reader.ReadAsync())
                                            {
                                                return new User
                                                {
                                                    Id = reader["Id"].ToString(),
                                                    Name = reader["Name"].ToString(),
                                                    Email = reader["Email"].ToString(),
                                                    Role = Enum.Parse<Role>(reader["Role"].ToString()),
                                                    ContractDays = Convert.ToInt32(reader["ContractDays"]),
                                                    AbsenceDays = Convert.ToInt32(reader["AbsenceDays"])
                                                };
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception("Error retrieving user by ID.", ex);
                                        }
                                        finally
                                        {
                                            _companyAdministrationDbContext.Close();
                                        }
                                    }
                                    throw new Exception("Database connection is not established.");
                                }
        
        public async Task<User> GetByEmail(string email)
                                    {
                                        if (_companyAdministrationDbContext.IsConnect())
                                        {
                                            try
                                            {
                                                var query = @"
                                                    SELECT 
                                                        u.*,
                                                        a.id AS absence_id,a.daysTaken as absence_daytaken, a.type AS absence_type, a.daysCount AS absence_daysCount,
                                                        a.created AS absence_created, a.status AS absence_status, a.startDate AS absence_startDate,
                                                        bt.id AS trip_id, bt.status AS trip_status, bt.issueDate AS trip_issueDate, bt.issueId AS trip_issueId,
                                                        bt.projectName, bt.userFullName, bt.task, bt.startDate AS trip_startDate,
                                                        bt.endDate AS trip_endDate, bt.totalDays, bt.carOwnerShip, bt.wage,
                                                        bt.accomodationMoney, bt.carBrand, bt.carRegistrationNumber,
                                                        bt.carTripDestination, bt.dateOfArrival, bt.carModel, bt.additionalExpences,
                                                        bt.carUsagePerHundredKm, bt.pricePerLiter, bt.departureDate,
                                                        bt.expensesResponsibility, bt.created AS trip_created
                                                    FROM 
                                                        User u
                                                    LEFT JOIN 
                                                        Absence a ON u.id = a.userId
                                                    LEFT JOIN 
                                                        BusinessTrip bt ON u.id = bt.userId
                                                    WHERE 
                                                        u.email = @email";
                            
                                                var command = new MySqlCommand(query, _companyAdministrationDbContext.Connection);
                                                command.Parameters.AddWithValue("@email", email);
                            
                                                User user = null;
                                                var processedAbsenceIds = new HashSet<int>();
                                                var processedTripIds = new HashSet<int>();

                                                await using var reader = await command.ExecuteReaderAsync();
                                                while (await reader.ReadAsync())
                                                {
                                                    user ??= new User
                                                    {
                                                        Id = reader["id"].ToString(),
                                                        Name = reader["name"].ToString(),
                                                        Email = reader["email"].ToString(),
                                                        Role = Enum.Parse<Role>(reader["role"].ToString()),
                                                        ContractDays = Convert.ToInt32(reader["contractDays"]),
                                                        AbsenceDays = Convert.ToInt32(reader["absenceDays"]),
                                                        Password = reader["password"].ToString(),
                                                        Absences = new List<Absence>(),
                                                        BusinessTrips = new List<BusinessTrip>()
                                                    };
                            
                                                    if (!reader.IsDBNull(reader.GetOrdinal("absence_id")))
                                                    {
                                                        var absenceId = Convert.ToInt32(reader["absence_id"]);
                                                        if (!processedAbsenceIds.Contains(absenceId))
                                                        {
                                                            user.Absences.Add(new Absence
                                                            {
                                                                Id = absenceId,
                                                                Type = Enum.Parse<AbsenceType>(reader["absence_type"].ToString()),
                                                                DaysCount = Convert.ToByte(reader["absence_daysCount"]),
                                                                Created = Convert.ToDateTime(reader["absence_created"]),
                                                                DaysTaken = Convert.ToByte(reader["absence_daytaken"]),
                                                                Status = Enum.Parse<AbsenceStatus>(reader["absence_status"].ToString()),
                                                                StartDate = Convert.ToDateTime(reader["absence_startDate"]),
                                                                UserId = user.Id
                                                            });
                                                            processedAbsenceIds.Add(absenceId);
                                                        }
                                                    }
                            
                                                    if (!reader.IsDBNull(reader.GetOrdinal("trip_id")))
                                                    {
                                                        var tripId = Convert.ToInt32(reader["trip_id"]);
                                                        if (!processedTripIds.Contains(tripId))
                                                        {
                                                            user.BusinessTrips.Add(new BusinessTrip
                                                            {
                                                                Id = tripId,
                                                                Status = Enum.Parse<BusinessTripStatus>(reader["trip_status"].ToString()),
                                                                IssueDate = Convert.ToDateTime(reader["trip_issueDate"]),
                                                                ProjectName = reader["projectName"].ToString(),
                                                                UserFullName = reader["userFullName"].ToString(),
                                                                Task = reader["task"]?.ToString(),
                                                                StartDate = Convert.ToDateTime(reader["trip_startDate"]),
                                                                EndDate = Convert.ToDateTime(reader["trip_endDate"]),
                                                                TotalDays = Convert.ToByte(reader["totalDays"]),
                                                                IssueId = Convert.ToInt32(reader["trip_issueId"]),
                                                                CarOwnership = Enum.Parse<CarOwnerShip>(reader["carOwnerShip"].ToString()),
                                                                Wage = Convert.ToDecimal(reader["wage"]),
                                                                AccommodationMoney = Convert.ToDecimal(reader["accomodationMoney"]),
                                                                CarBrand = reader["carBrand"]?.ToString(),
                                                                CarRegistrationNumber = reader["carRegistrationNumber"]?.ToString(),
                                                                CarTripDestination = reader["carTripDestination"].ToString(),
                                                                DateOfArrival = Convert.ToDateTime(reader["dateOfArrival"]),
                                                                CarModel = reader["carModel"].ToString(),
                                                                AdditionalExpences = Convert.ToDecimal(reader["additionalExpences"]),
                                                                CarUsagePerHundredKm = Convert.ToSingle(reader["carUsagePerHundredKm"]),
                                                                PricePerLiter = Convert.ToDouble(reader["pricePerLiter"]),
                                                                DepartureDate = Convert.ToDateTime(reader["departureDate"]),
                                                                ExpensesResponsibility = reader["expensesResponsibility"]?.ToString(),
                                                                Created = Convert.ToDateTime(reader["trip_created"]),
                                                                UserId = user.Id
                                                            });
                                                            processedTripIds.Add(tripId);
                                                        }
                                                    }
                                                }

                                                return user;
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception("Error retrieving user by email.", ex);
                                            }
                                            finally
                                            {
                                                _companyAdministrationDbContext.Close();
                                            }
                                        }
                                        throw new Exception("Database connection is not established.");
                                    }

        public async Task<bool> Create(User user)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Id))
                    {
                        user.Id = Guid.NewGuid().ToString();
                    }

                    var command = new MySqlCommand(
                        "INSERT INTO User (Id, Name, Email, Password, Role, ContractDays, AbsenceDays) " +
                        "VALUES (@id, @name, @email, @password, @role, @contractDays, @absenceDays)",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@role", (int)user.Role);
                    command.Parameters.AddWithValue("@contractDays", user.ContractDays);
                    command.Parameters.AddWithValue("@absenceDays", user.AbsenceDays);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating user in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<bool> Update(User user)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE User SET Name = @name, Email = @email, Password = @password, Role = @role, " +
                        "ContractDays = @contractDays, AbsenceDays = @absenceDays WHERE Id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@name", user.Name);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@password", user.Password);
                    command.Parameters.AddWithValue("@role", (int)user.Role);
                    command.Parameters.AddWithValue("@contractDays", user.ContractDays);
                    command.Parameters.AddWithValue("@absenceDays", user.AbsenceDays);
                    command.Parameters.AddWithValue("@id", user.Id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating user in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task UpdateAbsenceBalancesAsync()
                         {
                             if (_companyAdministrationDbContext.IsConnect())
                             {
                                 await using var transaction = await _companyAdministrationDbContext.Connection.BeginTransactionAsync();
                                 try
                                 {
                                     var transactionCommand = new MySqlCommand(@"
                                                 UPDATE user
                                                 SET AbsenceDays = LEAST(AbsenceDays + ContractDays, 40);",
                                         _companyAdministrationDbContext.Connection, transaction);
                                     await transactionCommand.ExecuteNonQueryAsync();
                                     await transaction.CommitAsync();
                                 }
                                 catch (Exception ex)
                                 {
                                     await transaction.RollbackAsync();
                                     throw new Exception("Error updating leave balances in the database.", ex);
                                 }
                                 finally
                                 {
                                     _companyAdministrationDbContext.Close();
                                 }
                             }
                         }
        
        public async Task<bool> Delete(string userId)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "DELETE FROM User WHERE Id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", userId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error deleting user from the database.", ex);
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
