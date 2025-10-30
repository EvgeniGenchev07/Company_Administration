using MySqlConnector;

namespace DataLayer.Persistence
{
    public class CompanyAdministrationDbContext
    {
        private readonly MySqlConnection _connection;
        public MySqlConnection Connection => _connection;
        public CompanyAdministrationDbContext(MySqlConnection connection)
        {
            _connection = connection;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                return false;
            }
            _connection.Open();
            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
