-- Add the role needed for WPF authorization without changing existing credentials or profiles.
IF DB_NAME() <> N'GuestRegistrationCourse' AND DB_NAME() NOT LIKE N'GuestRegistrationTests[_]%'
    THROW 51000, 'Select the project database before upgrading.', 1;
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
    THROW 51001, 'Create the Users table before upgrading.', 1;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH(N'dbo.Users', N'RoleCode') IS NULL
    ALTER TABLE dbo.Users ADD RoleCode VARCHAR(30) NOT NULL
        CONSTRAINT DF_Users_RoleCode DEFAULT 'Guest' WITH VALUES;
COMMIT TRANSACTION;
