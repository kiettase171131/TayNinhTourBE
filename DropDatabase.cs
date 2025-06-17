using MySqlConnector;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = "Server=103.216.119.189;Uid=TayNinhTour;Pwd=App@123456";
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("Connected to MySQL server successfully!");
            
            // Drop TayNinhTourDb if exists
            var dropCommand = new MySqlCommand("DROP DATABASE IF EXISTS TayNinhTourDb;", connection);
            await dropCommand.ExecuteNonQueryAsync();
            Console.WriteLine("Dropped database TayNinhTourDb");
            
            // Drop myapp_db if exists  
            var dropCommand2 = new MySqlCommand("DROP DATABASE IF EXISTS myapp_db;", connection);
            await dropCommand2.ExecuteNonQueryAsync();
            Console.WriteLine("Dropped database myapp_db");
            
            Console.WriteLine("All databases dropped successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
