-- Run this script in SSMS, connected to your SQL Server instance.
USE [master];
GO
IF DB_ID(N'GuestRegistrationCourse') IS NULL
BEGIN
    CREATE DATABASE [GuestRegistrationCourse];
END;
GO
USE [GuestRegistrationCourse];
GO
SELECT DB_NAME() AS CurrentDatabase;
