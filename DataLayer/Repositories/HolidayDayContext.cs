using BusinessLayer.Entities;
using BusinessLayer.Enums;
using DataLayer.Persistence;
using MySqlConnector;

namespace DataLayer.Repositories
{
    public class HolidayDayContext
    {
        private readonly CompanyAdministrationDbContext _companyAdministrationDbContext;

        public HolidayDayContext(CompanyAdministrationDbContext companyAdministrationDbContext)
        {
            _companyAdministrationDbContext = companyAdministrationDbContext ?? throw new ArgumentNullException(nameof(companyAdministrationDbContext));
        }

        public bool Create(HolidayDay holiday)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "INSERT INTO HolidayDay (name, date, isCustom) " +
                        "VALUES (@name, @date, @isCustom)",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@name", holiday.Name);
                    command.Parameters.AddWithValue("@date", holiday.Date);
                    command.Parameters.AddWithValue("@isCustom", holiday.IsCustom);

                    int rowsAffected = command.ExecuteNonQuery();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error creating holiday in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }
        
        public HolidayDay? GetById(int id)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("SELECT * FROM HolidayDay WHERE id = @id", _companyAdministrationDbContext.Connection);
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var holiday = new HolidayDay
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                IsCustom = Convert.ToBoolean(reader["isCustom"]),
                                Date = Convert.ToDateTime(reader["date"]),
                            };
                            return holiday;
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving holiday by ID from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Update(HolidayDay holiday)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "UPDATE HolidayDay SET name = @name, date = @date, isCustom = @isCustom WHERE id = @id",
                        _companyAdministrationDbContext.Connection);

                    command.Parameters.AddWithValue("@id", holiday.Id);
                    command.Parameters.AddWithValue("@name", holiday.Name);
                    command.Parameters.AddWithValue("@date", holiday.Date);
                    command.Parameters.AddWithValue("@isCustom", holiday.IsCustom);

                    int rowsAffected = command.ExecuteNonQuery();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error updating holiday in the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public bool Delete(int id)
        {
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand("DELETE FROM HolidayDay WHERE id = @id", _companyAdministrationDbContext.Connection);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    _companyAdministrationDbContext.Close();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    _companyAdministrationDbContext.Close();
                    throw new Exception("Error deleting holiday from the database.", ex);
                }
            }
            throw new Exception("Database connection is not established.");
        }

        public List<HolidayDay> GetAll()
        {
            var holidays = new List<HolidayDay>();
            if (_companyAdministrationDbContext.IsConnect())
            {
                try
                {
                    var command = new MySqlCommand(
                        "SELECT * FROM HolidayDay ORDER BY date DESC",
                        _companyAdministrationDbContext.Connection);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            holidays.Add(new HolidayDay
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                IsCustom = Convert.ToBoolean(reader["isCustom"]),
                                Date = Convert.ToDateTime(reader["date"]),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error retrieving holidays from the database.", ex);
                }
                finally
                {
                    _companyAdministrationDbContext.Close();
                }
                int year = DateTime.Now.Year;

                holidays.AddRange(InitializeOfficialHolidays(year - 1).Result);
                holidays.AddRange(InitializeOfficialHolidays(year).Result);
                holidays.AddRange(InitializeOfficialHolidays(year + 1).Result);
                return holidays;
            }
            throw new Exception("Database connection is not established.");
        }
        private DateTime CalculateOrthodoxEaster(int year)
        {
            int a = year % 4;
            int b = year % 7;
            int c = year % 19;
            int d = (19 * c + 15) % 30;
            int e = (2 * a + 4 * b - d + 34) % 7;
            int month = (int)Math.Floor((d + e + 114) / 31M);
            int day = ((d + e + 114) % 31) + 1;

            DateTime easter = new DateTime(year, month, day);
            return easter.AddDays(13);
        }
        private async Task<List<HolidayDay>> InitializeOfficialHolidays(int year)
        {
            var holidays = new List<HolidayDay>();
            var fixedHolidays = new List<HolidayDay>
        {
            new HolidayDay(){Date=new DateTime(year, 1, 1),Name="Нова година" },
             new HolidayDay(){Date=new DateTime(year, 3, 3),Name = "Ден на Освобождението"},
             new HolidayDay(){Date=new DateTime(year, 5, 1), Name = "Ден на труда" },
             new HolidayDay(){Date=new DateTime(year, 5, 6), Name = "Гергьовден" },
            new HolidayDay(){Date =new DateTime(year, 5, 24), Name = "Ден на българската просвета и култура" },
             new HolidayDay(){Date=new DateTime(year, 9, 6), Name = "Ден на Съединението" },
            new HolidayDay(){Date =new DateTime(year, 9, 22), Name = "Ден на Независимостта" },
             new HolidayDay(){Date=new DateTime(year, 12, 24), Name = "Бъдни вечер" },
           new HolidayDay(){Date  =new DateTime(year, 12, 25), Name = "Коледа" },
            new HolidayDay(){Date= new DateTime(year, 12, 26), Name = "Коледа" }
        };

            var easter = CalculateOrthodoxEaster(year);
            var easterHolidays = new List<HolidayDay>
        {
           new HolidayDay(){Date = easter.AddDays(-2),Name="Разпети петък" },
             new HolidayDay(){Date =easter.AddDays(-1),Name="Страстна събота" },
             new HolidayDay(){Date =easter,Name="Великден" },
             new HolidayDay(){Date =easter.AddDays(1),Name="Великден" }
        };

            holidays.AddRange(fixedHolidays);
            holidays.AddRange(easterHolidays);
            for (int i = 0; i < holidays.Count; i++)
            {
                var holiday = holidays[i];
                if (holiday.Date.DayOfWeek == DayOfWeek.Saturday || holiday.Date.DayOfWeek == DayOfWeek.Sunday)
                {
                    DateTime monday = holiday.Date;
                    while (monday.DayOfWeek != DayOfWeek.Monday)
                    {
                        monday = monday.AddDays(1);
                    }

                    if (!holidays.Select(h => h.Date).Contains(monday))
                    {
                        holidays.Add(new HolidayDay() { Date = monday, Name = $"Почивен поради {holiday.Name}" });
                    }
                }
            }
            return holidays;
        }
    }
}
