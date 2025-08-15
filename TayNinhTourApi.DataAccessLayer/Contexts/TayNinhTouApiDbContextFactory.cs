using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TayNinhTourApi.DataAccessLayer.Contexts
{
    /// <summary>
    /// Design-time factory for creating DbContext instances for EF Core migrations
    /// </summary>
    public class TayNinhTouApiDbContextFactory : IDesignTimeDbContextFactory<TayNinhTouApiDbContext>
    {
        public TayNinhTouApiDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "TayNinhTourApi.Controller");
            
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true);

            var configuration = configurationBuilder.Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback connection string với timeout được tăng
                connectionString = "Server=localhost;Port=3306;Database=tayninhtourdb_local;Uid=root;Pwd=;SslMode=none;AllowPublicKeyRetrieval=true;Connection Timeout=120;Command Timeout=300;";
            }

            // Configure DbContext options with retry policy and extended timeouts
            var optionsBuilder = new DbContextOptionsBuilder<TayNinhTouApiDbContext>();
            optionsBuilder.UseMySql(connectionString, 
                new MySqlServerVersion(new Version(8, 0, 21)),
                mySqlOptions => {
                    mySqlOptions.CommandTimeout(300); // Tăng command timeout lên 5 phút
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });

            return new TayNinhTouApiDbContext(optionsBuilder.Options);
        }
    }
}