-- Update the original 20 demo users without changing IDs or relationships.
USE [GuestRegistrationCourse];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @People TABLE (Id INT PRIMARY KEY, FirstName NVARCHAR(50), LastName NVARCHAR(50));
INSERT INTO @People (Id, FirstName, LastName) VALUES
(1, N'Христина', N'Лега'),
(2, N'Марта', N'Козак'),
(3, N'Юлія', N'Гілевич'),
(4, N'Кароліна', N'Балух'),
(5, N'Катерина', N'Смірнова'),
(6, N'Максим', N'Іжик'),
(7, N'Катерина', N'Присяжна'),
(8, N'Поліна', N'Циклаурі'),
(9, N'Катерина', N'Гладич'),
(10, N'Сергій', N'Шемчук'),
(11, N'Єлизавета', N'Поливко'),
(12, N'Христина', N'Бадюкевич'),
(13, N'Євгеній', N'Кандич'),
(14, N'Олег', N'Монцібович'),
(15, N'Вікторія', N'Федис'),
(16, N'Ірина', N'Павлух'),
(17, N'Каріна', N'Чендей'),
(18, N'Валерія', N'Козявіна'),
(19, N'Роксоляна', N'Марголич'),
(20, N'Катерина', N'Жушман');

-- Stop if the original demo IDs and logins are no longer present.
IF (SELECT COUNT(*) FROM dbo.Users AS u INNER JOIN @People AS p ON p.Id = u.Id
    WHERE u.Login = N'guest' + RIGHT(N'0' + CONVERT(NVARCHAR(2), p.Id), 2)) <> 20
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51002, 'Expected demo users guest01 through guest20. No changes made.', 1;
END;

UPDATE u SET FirstName = p.FirstName, LastName = p.LastName
FROM dbo.Users AS u INNER JOIN @People AS p ON p.Id = u.Id;

UPDATE b SET CardHolder = p.FirstName + N' ' + p.LastName
FROM dbo.BankCards AS b INNER JOIN @People AS p ON p.Id = b.UserId;

COMMIT TRANSACTION;

SELECT Id, Login, FirstName, LastName FROM dbo.Users ORDER BY Id;
SELECT Id, UserId, CardHolder FROM dbo.BankCards ORDER BY Id;
SELECT N'Users' AS TableName, COUNT(*) AS [RowCount] FROM dbo.Users
UNION ALL SELECT N'BankCards', COUNT(*) FROM dbo.BankCards
UNION ALL SELECT N'LoginAttempts', COUNT(*) FROM dbo.LoginAttempts;
