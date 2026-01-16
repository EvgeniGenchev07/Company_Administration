using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer.Interfaces.Repository;
using DataLayer.Persistence;
using MySqlConnector;

namespace DataLayer.Repositories
{
    public class BusinessTripContext(CompanyAdministrationDbContext companyAdministrationDbContext)
        : IBusinessTripContext
    {
        private readonly CompanyAdministrationDbContext _companyAdministrationDbContext = companyAdministrationDbContext ?? throw new ArgumentNullException(nameof(companyAdministrationDbContext));

        public async Task<bool> Create(BusinessTrip businessTrip)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {

                    var command = new MySqlCommand(
                        "INSERT INTO BusinessTrip (issueId, status, issueDate, projectName, userFullName, task, " +
                        "startDate, endDate, totalDays, carOwnerShip, wage, accomodationMoney, carBrand, " +
                        "carRegistrationNumber, carTripDestination, dateOfArrival, carModel, additionalExpences, carUsagePerHundredKm, " +
                        "pricePerLiter, departureDate, expensesResponsibility, created, userId) " +
                        "VALUES (@issueId, @status, @issueDate, @projectName, @userFullName, @task, @startDate, @endDate, " +
                        "@totalDays, @carOwnerShip, @wage, @accomodationMoney, @carBrand, @carRegistrationNumber, " +
                        "@carTripDestination, @dateOfArrival, @carModel, @additionalExpences, @carUsagePerHundredKm, @pricePerLiter, " +
                        "@departureDate, @expensesResponsibility, @created, @userId)",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@issueId", 0);
                    command.Parameters.AddWithValue("@status", (int)businessTrip.Status);
                    command.Parameters.AddWithValue("@issueDate", businessTrip.IssueDate);
                    command.Parameters.AddWithValue("@projectName", businessTrip.ProjectName);
                    command.Parameters.AddWithValue("@userFullName", businessTrip.UserFullName);
                    command.Parameters.AddWithValue("@task", businessTrip.Task ?? "");
                    command.Parameters.AddWithValue("@startDate", businessTrip.StartDate);
                    command.Parameters.AddWithValue("@endDate", businessTrip.EndDate);
                    command.Parameters.AddWithValue("@totalDays", businessTrip.TotalDays);
                    command.Parameters.AddWithValue("@carOwnerShip", (int)businessTrip.CarOwnership);
                    command.Parameters.AddWithValue("@wage", businessTrip.Wage);
                    command.Parameters.AddWithValue("@accomodationMoney", businessTrip.AccommodationMoney);
                    command.Parameters.AddWithValue("@carBrand", businessTrip.CarBrand ?? "");
                    command.Parameters.AddWithValue("@carRegistrationNumber", businessTrip.CarRegistrationNumber ?? "");
                    command.Parameters.AddWithValue("@carTripDestination", businessTrip.CarTripDestination);
                    command.Parameters.AddWithValue("@dateOfArrival", businessTrip.DateOfArrival);
                    command.Parameters.AddWithValue("@carModel", businessTrip.CarModel);
                    command.Parameters.AddWithValue("@additionalExpences", businessTrip.AdditionalExpences);
                    command.Parameters.AddWithValue("@carUsagePerHundredKm", businessTrip.CarUsagePerHundredKm);
                    command.Parameters.AddWithValue("@pricePerLiter", businessTrip.PricePerLiter);
                    command.Parameters.AddWithValue("@departureDate", businessTrip.DepartureDate);
                    command.Parameters.AddWithValue("@expensesResponsibility", businessTrip.ExpensesResponsibility ?? "");
                    command.Parameters.AddWithValue("@created", DateTime.Now.Date);
                    command.Parameters.AddWithValue("@userId", businessTrip.UserId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        using (var transaction = await _companyAdministrationDbContext.Connection.BeginTransactionAsync())
                        {
                            try
                            {
                                var transactionCommand = new MySqlCommand(@"
                                SET @currentYearMonth := NULL;
                                SET @rank := 0;

                                UPDATE BusinessTrip bt
                                JOIN (
                                    SELECT 
                                        id,
                                        @rank := IF(@currentYearMonth = DATE_FORMAT(issueDate, '%Y-%m'),
                                                    @rank + 1,
                                                    1) AS newIssueId,
                                        @currentYearMonth := DATE_FORMAT(issueDate, '%Y-%m') AS ym
                                    FROM BusinessTrip
                                    ORDER BY issueDate
                                ) AS ranked ON bt.id = ranked.id
                                SET bt.issueId = ranked.newIssueId
                                WHERE bt.id = ranked.id;",
                                _companyAdministrationDbContext.Connection, transaction);
                                await transactionCommand.ExecuteNonQueryAsync();
                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                throw new Exception("Error updating issueId: " + ex.Message);
                            }
                        }
                    }
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating business trip in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<BusinessTrip> GetById(int businessTripId)
                       {
                           if (_companyAdministrationDbContext.IsConnect())
                           {
                               try
                               {
                                   var command = new MySqlCommand(
                                       "SELECT * FROM BusinessTrip WHERE id = @id",
                                       _companyAdministrationDbContext.Connection);
               
                                   command.Parameters.AddWithValue("@id", businessTripId);
               
                                   using (var reader = await command.ExecuteReaderAsync())
                                   {
                                       if (await reader.ReadAsync())
                                       {
                                           return new BusinessTrip
                                           {
                                               Id = Convert.ToInt32(reader["id"]),
                                               Status = (BusinessTripStatus)Convert.ToInt32(reader["status"]),
                                               IssueDate = Convert.ToDateTime(reader["issueDate"]),
                                               ProjectName = reader["projectName"].ToString(),
                                               UserFullName = reader["userFullName"].ToString(),
                                               Task = reader["task"].ToString(),
                                               StartDate = Convert.ToDateTime(reader["startDate"]),
                                               EndDate = Convert.ToDateTime(reader["endDate"]),
                                               TotalDays = Convert.ToByte(reader["totalDays"]),
                                               CarOwnership = (CarOwnerShip)Convert.ToInt32(reader["carOwnerShip"]),
                                               Wage = Convert.ToDecimal(reader["wage"]),
                                               AccommodationMoney = Convert.ToDecimal(reader["accomodationMoney"]),
                                               CarBrand = reader["carBrand"].ToString(),
                                               IssueId = Convert.ToInt32(reader["issueId"]),
                                               CarRegistrationNumber = reader["carRegistrationNumber"].ToString(),
                                               CarTripDestination = reader["carTripDestination"].ToString(),
                                               DateOfArrival = Convert.ToDateTime(reader["dateOfArrival"]),
                                               CarModel = reader["carModel"].ToString(),
                                               AdditionalExpences = Convert.ToDecimal(reader["additionalExpences"]),
                                               CarUsagePerHundredKm = Convert.ToSingle(reader["carUsagePerHundredKm"]),
                                               PricePerLiter = Convert.ToDouble(reader["pricePerLiter"]),
                                               DepartureDate = Convert.ToDateTime(reader["departureDate"]),
                                               ExpensesResponsibility = reader["expensesResponsibility"].ToString(),
                                               Created = Convert.ToDateTime(reader["created"]),
                                               UserId = reader["userId"].ToString()
                                           };
                                       }
                                   }
                               }
                               catch (Exception ex)
                               {
                                   throw new Exception("Error retrieving business trip from the database.", ex);
                               }
                               finally
                               {
                                   _companyAdministrationDbContext.Close();
                               }
                           }
                           throw new Exception("Database connection is not established.");
                       }
        
        public async Task<List<BusinessTrip>> GetByUserId(string userId)
                         {
                             var businessTrips = new List<BusinessTrip>();
                             if (_companyAdministrationDbContext.IsConnect())
                             {
                                 try
                                 {
                                     var command = new MySqlCommand(
                                         "SELECT * FROM BusinessTrip WHERE userId = @userId ORDER BY created DESC",
                                         _companyAdministrationDbContext.Connection);
                 
                                     command.Parameters.AddWithValue("@userId", userId);
                 
                                     using (var reader = await command.ExecuteReaderAsync())
                                     {
                                         while (await reader.ReadAsync())
                                         {
                                             businessTrips.Add(new BusinessTrip
                                             {
                                                 Id = Convert.ToInt32(reader["id"]),
                                                 Status = (BusinessTripStatus)Convert.ToInt32(reader["status"]),
                                                 IssueDate = Convert.ToDateTime(reader["issueDate"]),
                                                 ProjectName = reader["projectName"].ToString(),
                                                 UserFullName = reader["userFullName"].ToString(),
                                                 Task = reader["task"].ToString(),
                                                 StartDate = Convert.ToDateTime(reader["startDate"]),
                                                 EndDate = Convert.ToDateTime(reader["endDate"]),
                                                 TotalDays = Convert.ToByte(reader["totalDays"]),
                                                 CarOwnership = (CarOwnerShip)Convert.ToInt32(reader["carOwnerShip"]),
                                                 Wage = Convert.ToDecimal(reader["wage"]),
                                                 IssueId = Convert.ToInt32(reader["issueId"]),
                                                 AccommodationMoney = Convert.ToDecimal(reader["accomodationMoney"]),
                                                 CarBrand = reader["carBrand"].ToString(),
                                                 CarRegistrationNumber = reader["carRegistrationNumber"].ToString(),
                                                 CarTripDestination = reader["carTripDestination"].ToString(),
                                                 DateOfArrival = Convert.ToDateTime(reader["dateOfArrival"]),
                                                 CarModel = reader["carModel"].ToString(),
                                                 AdditionalExpences = Convert.ToDecimal(reader["additionalExpences"]),
                                                 CarUsagePerHundredKm = Convert.ToSingle(reader["carUsagePerHundredKm"]),
                                                 PricePerLiter = Convert.ToDouble(reader["pricePerLiter"]),
                                                 DepartureDate = Convert.ToDateTime(reader["departureDate"]),
                                                 ExpensesResponsibility = reader["expensesResponsibility"].ToString(),
                                                 Created = Convert.ToDateTime(reader["created"]),
                                                 UserId = reader["userId"].ToString()
                                             });
                                         }
                                     }
                                     _companyAdministrationDbContext.Close();
                                     return businessTrips;
                                 }
                                 catch (Exception ex)
                                 {
                                     throw new Exception("Error retrieving business trips by user ID from the database.", ex);
                                 }
                                 finally
                                 {
                                     _companyAdministrationDbContext.Close();
                                 }
                             }
                             throw new Exception("Database connection is not established.");
                         }
        
        public async Task<List<BusinessTrip>> GetAll()
                                                                  {
                                                                      var businessTrips = new List<BusinessTrip>();
                                                                      if (_companyAdministrationDbContext.IsConnect())
                                                                      {
                                                                          try
                                                                          {
                                                                              var command = new MySqlCommand(
                                                                                  "SELECT * FROM BusinessTrip ORDER BY created DESC",
                                                                                  _companyAdministrationDbContext.Connection);
                                                          
                                                                              using (var reader = await command.ExecuteReaderAsync())
                                                                              {
                                                                                  while (await reader.ReadAsync())
                                                                                  {
                                                                                      businessTrips.Add(new BusinessTrip
                                                                                      {
                                                                                          Id = Convert.ToInt32(reader["id"]),
                                                                                          Status = (BusinessTripStatus)Convert.ToInt32(reader["status"]),
                                                                                          IssueDate = Convert.ToDateTime(reader["issueDate"]),
                                                                                          ProjectName = reader["projectName"].ToString(),
                                                                                          UserFullName = reader["userFullName"].ToString(),
                                                                                          Task = reader["task"].ToString(),
                                                                                          StartDate = Convert.ToDateTime(reader["startDate"]),
                                                                                          EndDate = Convert.ToDateTime(reader["endDate"]),
                                                                                          TotalDays = Convert.ToByte(reader["totalDays"]),
                                                                                          CarOwnership = (CarOwnerShip)Convert.ToInt32(reader["carOwnerShip"]),
                                                                                          Wage = Convert.ToDecimal(reader["wage"]),
                                                                                          AccommodationMoney = Convert.ToDecimal(reader["accomodationMoney"]),
                                                                                          IssueId = Convert.ToInt32(reader["issueId"]),
                                                                                          CarBrand = reader["carBrand"].ToString(),
                                                                                          CarRegistrationNumber = reader["carRegistrationNumber"].ToString(),
                                                                                          CarTripDestination = reader["carTripDestination"].ToString(),
                                                                                          DateOfArrival = Convert.ToDateTime(reader["dateOfArrival"]),
                                                                                          CarModel = reader["carModel"].ToString(),
                                                                                          AdditionalExpences = Convert.ToDecimal(reader["additionalExpences"]),
                                                                                          CarUsagePerHundredKm = Convert.ToSingle(reader["carUsagePerHundredKm"]),
                                                                                          PricePerLiter = Convert.ToDouble(reader["pricePerLiter"]),
                                                                                          DepartureDate = Convert.ToDateTime(reader["departureDate"]),
                                                                                          ExpensesResponsibility = reader["expensesResponsibility"].ToString(),
                                                                                          Created = Convert.ToDateTime(reader["created"]),
                                                                                          UserId = reader["userId"].ToString()
                                                                                      });
                                                                                  }
                                                                              }
                                                                              _companyAdministrationDbContext.Close();
                                                                              return businessTrips;
                                                                          }
                                                                          catch (Exception ex)
                                                                          {
                                                                              throw new Exception("Error retrieving all business trips from the database.", ex);
                                                                          }
                                                                          finally
                                                                          {
                                                                              _companyAdministrationDbContext.Close();
                                                                          }
                                                                      }
                                                                      throw new Exception("Database connection is not established.");
                                                                  }
        
        public async Task<bool> Update(BusinessTrip businessTrip)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE BusinessTrip SET status = @status, projectName = @projectName, " +
                        "userFullName = @userFullName, task = @task, startDate = @startDate, endDate = @endDate, " +
                        "totalDays = @totalDays, carOwnerShip = @carOwnerShip, wage = @wage, accomodationMoney = @accomodationMoney, " +
                        "carBrand = @carBrand, carRegistrationNumber = @carRegistrationNumber, carTripDestination = @carTripDestination, " +
                        "dateOfArrival = @dateOfArrival, carModel = @carModel, additionalExpences = @additionalExpences, carUsagePerHundredKm = @carUsagePerHundredKm, " +
                        "pricePerLiter = @pricePerLiter, departureDate = @departureDate, expensesResponsibility = @expensesResponsibility " +
                        "WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@status", (int)businessTrip.Status);
                    command.Parameters.AddWithValue("@projectName", businessTrip.ProjectName);
                    command.Parameters.AddWithValue("@userFullName", businessTrip.UserFullName);
                    command.Parameters.AddWithValue("@task", businessTrip.Task ?? "");
                    command.Parameters.AddWithValue("@startDate", businessTrip.StartDate);
                    command.Parameters.AddWithValue("@endDate", businessTrip.EndDate);
                    command.Parameters.AddWithValue("@totalDays", businessTrip.TotalDays);
                    command.Parameters.AddWithValue("@carOwnerShip", (int)businessTrip.CarOwnership);
                    command.Parameters.AddWithValue("@wage", businessTrip.Wage);
                    command.Parameters.AddWithValue("@accomodationMoney", businessTrip.AccommodationMoney);
                    command.Parameters.AddWithValue("@carBrand", businessTrip.CarBrand ?? "");
                    command.Parameters.AddWithValue("@carRegistrationNumber", businessTrip.CarRegistrationNumber ?? "");
                    command.Parameters.AddWithValue("@carTripDestination", businessTrip.CarTripDestination);
                    command.Parameters.AddWithValue("@dateOfArrival", businessTrip.DateOfArrival);
                    command.Parameters.AddWithValue("@carModel", businessTrip.CarModel);
                    command.Parameters.AddWithValue("@additionalExpences", businessTrip.AdditionalExpences);
                    command.Parameters.AddWithValue("@carUsagePerHundredKm", businessTrip.CarUsagePerHundredKm);
                    command.Parameters.AddWithValue("@pricePerLiter", businessTrip.PricePerLiter);
                    command.Parameters.AddWithValue("@departureDate", businessTrip.DepartureDate);
                    command.Parameters.AddWithValue("@expensesResponsibility", businessTrip.ExpensesResponsibility ?? "");
                    command.Parameters.AddWithValue("@id", businessTrip.Id);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating business trip in the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public async Task<bool> Delete(int businessTripId)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "DELETE FROM BusinessTrip WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", businessTripId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error deleting business trip from the database.", ex);
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