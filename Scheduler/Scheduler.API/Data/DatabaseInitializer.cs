using Microsoft.Data.SqlClient;

namespace Scheduler.API.Data;

public static class DatabaseInitializer
{
    public static void EnsureDatabaseCreated(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var originalDb = builder.InitialCatalog;
        
        // Connect to master to create the database
        builder.InitialCatalog = "master";
        
        using var connection = new SqlConnection(builder.ConnectionString);
        
        int retries = 5;
        while (retries > 0)
        {
            try 
            {
                connection.Open();
                
                using var command = connection.CreateCommand();
                command.CommandText = $@"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{originalDb}')
                    BEGIN
                        CREATE DATABASE [{originalDb}];
                    END";
                
                command.ExecuteNonQuery();
                Console.WriteLine($"Database {originalDb} checked/created successfully.");
                return;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error connecting to DB (Attempt {6 - retries}/5): {ex.Message}");
                retries--;
                if (retries == 0) throw;
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
