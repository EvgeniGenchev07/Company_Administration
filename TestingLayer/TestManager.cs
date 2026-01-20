using DataLayer.Persistence;
using MySqlConnector;

namespace TestingLayer;

[SetUpFixture]
public class TestManager
{
    internal static CompanyAdministrationDbContext DbContext { get; private set; }

    [OneTimeSetUp]
    public void Setup()
    {
        const string connectionString = "connection_string";
        var connection = new MySqlConnection(connectionString);
        DbContext = new CompanyAdministrationDbContext(connection);

    }

    [OneTimeTearDown]
    public void TearDown()
    {
        DbContext.Close();
    }
}
