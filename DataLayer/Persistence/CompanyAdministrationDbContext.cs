using MySqlConnector;

namespace DataLayer.Persistence
{
    public class CompanyAdministrationDbContext(MySqlConnection connection)
    {
        public MySqlConnection Connection => connection;

        public bool IsConnect()
        {
            if (Connection == null)
            {
                return false;
            }
            connection.Open();
            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
