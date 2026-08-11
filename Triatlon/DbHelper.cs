// Data/DbHelper.cs
using Npgsql;

namespace Triatlon.Data
{
    public class DbHelper
    {
        private readonly string _connString;

        public DbHelper(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Connection string ni nastavljen!");
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connString);
        }
    }
}
