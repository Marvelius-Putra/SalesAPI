using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SalesAPI.Model;
using System.Data;
using System.Data.SQLite;

namespace SalesAPI.Repositories
{
    public class BaseRepository
    {
        private readonly IConfiguration _configuration;
        private readonly AppSettings _appSettings;

        public BaseRepository(IConfiguration configuration, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _configuration = configuration;
        }

        protected IDbConnection CreateConnection()
        {
            var connection = new SQLiteConnection(_appSettings.DefaultConnection);
            connection.Open();
            return connection;
        }
    }
}
