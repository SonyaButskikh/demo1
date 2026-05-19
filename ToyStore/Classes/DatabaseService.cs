using System.Configuration;
using System.Data.OleDb;

namespace ToyStore.Classes
{
    internal class DatabaseService
    {
        private static string connectionString =
            ConfigurationManager.ConnectionStrings["ToyStoreConnection"].ConnectionString;

        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(connectionString);
        }
    }
}