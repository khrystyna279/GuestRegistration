-- Update gender, synthetic email addresses, and fictional Lviv addresses only.
USE [GuestRegistrationCourse];
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Profiles TABLE
(
    Id INT PRIMARY KEY,
    LastName NVARCHAR(50),
    Gender VARCHAR(10),
    Email NVARCHAR(254) UNIQUE,
    Address NVARCHAR(200)
);

INSERT INTO @Profiles (Id, LastName, Gender, Email, Address) VALUES
(1, N'Лега', 'Female', N'leha@example.com', N'Львів, вул. Зелена, 12'),
(2, N'Козак', 'Female', N'kozak@example.com', N'Львів, вул. Шевченка, 24'),
(3, N'Гілевич', 'Female', N'hilevych@example.com', N'Львів, вул. Личаківська, 36'),
(4, N'Балух', 'Female', N'balukh@example.com', N'Львів, вул. Городоцька, 48'),
(5, N'Смірнова', 'Female', N'smirnova@example.com', N'Львів, вул. Наукова, 15'),
(6, N'Іжик', 'Male', N'izhyk@example.com', N'Львів, вул. Стрийська, 27'),
(7, N'Присяжна', 'Female', N'prysiazhna@example.com', N'Львів, вул. Сахарова, 19'),
(8, N'Циклаурі', 'Female', N'tsyklauri@example.com', N'Львів, вул. Франка, 31'),
(9, N'Гладич', 'Female', N'hladych@example.com', N'Львів, вул. Кульпарківська, 43'),
(10, N'Шемчук', 'Male', N'shemchuk@example.com', N'Львів, вул. Пасічна, 22'),
(11, N'Поливко', 'Female', N'polyvko@example.com', N'Львів, вул. Варшавська, 34'),
(12, N'Бадюкевич', 'Female', N'badiukevych@example.com', N'Львів, вул. Замарстинівська, 46'),
(13, N'Кандич', 'Male', N'kandych@example.com', N'Львів, вул. Княгині Ольги, 18'),
(14, N'Монцібович', 'Male', N'montsibovych@example.com', N'Львів, вул. Виговського, 29'),
(15, N'Федис', 'Female', N'fedys@example.com', N'Львів, вул. Сяйво, 11'),
(16, N'Павлух', 'Female', N'pavlukh@example.com', N'Львів, вул. Любінська, 23'),
(17, N'Чендей', 'Female', N'chendei@example.com', N'Львів, вул. Коперника, 17'),
(18, N'Козявіна', 'Female', N'koziavina@example.com', N'Львів, вул. Дорошенка, 25'),
(19, N'Марголич', 'Female', N'marholych@example.com', N'Львів, вул. Антоновича, 39'),
(20, N'Жушман', 'Female', N'zhushman@example.com', N'Львів, вул. Пекарська, 21');

-- Stop if the preceding name update has not been applied to all 20 demo users.
IF (SELECT COUNT(*) FROM dbo.Users AS u
    INNER JOIN @Profiles AS p ON p.Id = u.Id AND p.LastName = u.LastName
    WHERE u.Login = N'guest' + RIGHT(N'0' + CONVERT(NVARCHAR(2), p.Id), 2)) <> 20
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51003, 'Expected 20 named demo users. Run 04_UpdateNames.sql first. No changes made.', 1;
END;

UPDATE u
SET Gender = p.Gender, Email = p.Email, Address = p.Address
FROM dbo.Users AS u INNER JOIN @Profiles AS p ON p.Id = u.Id;

COMMIT TRANSACTION;

SELECT Id, Login, FirstName, LastName, Gender, Email, Address, Phone
FROM dbo.Users ORDER BY Id;
