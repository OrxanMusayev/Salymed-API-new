using Microsoft.Data.SqlClient;
using System;

namespace backend
{
    public class AddPreferredLanguageColumn
    {
        public static void Main(string[] args)
        {
            var connectionString = "Server=localhost,1433;Database=SalymedDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("✅ Connected to database");

                var command = connection.CreateCommand();
                command.CommandText = @"
                    IF NOT EXISTS (
                        SELECT * FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'[dbo].[Users]')
                        AND name = 'PreferredLanguage'
                    )
                    BEGIN
                        ALTER TABLE [dbo].[Users]
                        ADD PreferredLanguage NVARCHAR(5) NOT NULL DEFAULT 'az';
                        PRINT 'PreferredLanguage column added successfully';
                    END
                    ELSE
                    BEGIN
                        PRINT 'PreferredLanguage column already exists';
                    END
                ";

                command.ExecuteNonQuery();
                Console.WriteLine("✅ Migration executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
