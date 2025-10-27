-- Add PreferredLanguage column to Users table if not exists
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
