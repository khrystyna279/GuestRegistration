USE [GuestRegistrationCourse];
GO
SELECT N'Users' AS TableName, COUNT(*) AS [RowCount] FROM dbo.Users
UNION ALL SELECT N'BankCards', COUNT(*) FROM dbo.BankCards
UNION ALL SELECT N'LoginAttempts', COUNT(*) FROM dbo.LoginAttempts;
SELECT Id, Login, FirstName, LastName, Gender, Address, Email, Phone, CreatedAtUtc, RoleCode, RowInsertTime, UpdateTime
FROM dbo.Users ORDER BY Id;
SELECT * FROM dbo.BankCards ORDER BY Id;
SELECT * FROM dbo.LoginAttempts ORDER BY Id;
DBCC CHECKCONSTRAINTS WITH ALL_CONSTRAINTS;
