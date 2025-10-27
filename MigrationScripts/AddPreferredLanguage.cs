using Microsoft.Data.SqlClient;
using System;

namespace backend.MigrationScripts
{
    public class AddPreferredLanguage
    {
        public static void Execute(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

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
            Console.WriteLine("Migration executed successfully");
        }
    }
}
