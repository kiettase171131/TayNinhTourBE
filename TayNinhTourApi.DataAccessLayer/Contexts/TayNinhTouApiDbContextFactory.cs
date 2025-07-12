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
                // Fallback connection string n?u không tìm th?y trong config
                connectionString = "Server=103.216.119.189;Port=3306;Database=TayNinhTourDb;Uid=TayNinhTour;Pwd=App@123456;SslMode=none;AllowPublicKeyRetrieval=true;Connection Timeout=30;Command Timeout=60;";
            }

            // Configure DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<TayNinhTouApiDbContext>();
            optionsBuilder.UseMySql(connectionString, 
                new MySqlServerVersion(new Version(8, 0, 21)),
                mySqlOptions => mySqlOptions.CommandTimeout(120));

            return new TayNinhTouApiDbContext(optionsBuilder.Options);
        }
    }
}