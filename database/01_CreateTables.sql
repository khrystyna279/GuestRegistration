-- Select GuestRegistrationCourse in the SSMS database dropdown before running.
-- Tests run this same schema in a separate database with a generated name.
IF DB_NAME() <> N'GuestRegistrationCourse'
   AND DB_NAME() NOT LIKE N'GuestRegistrationTests[_]%'
    THROW 51000, 'Select GuestRegistrationCourse before creating the tables.', 1;

IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.BankCards', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.LoginAttempts', N'U') IS NOT NULL
    THROW 51001, 'Tables already exist. This script never drops existing data.', 1;

SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Login NVARCHAR(30) COLLATE Latin1_General_100_BIN2 NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(200) NOT NULL,
    RecoveryWordHash VARCHAR(200) NOT NULL,
    Gender VARCHAR(10) NOT NULL,
    Address NVARCHAR(200) NOT NULL,
    Email NVARCHAR(254) COLLATE Latin1_General_100_BIN2 NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    CreatedAtUtc DATETIME2(0) NOT NULL,
    CONSTRAINT CK_Users_Login CHECK
        (LEN(Login) BETWEEN 3 AND 30 AND Login = LOWER(LTRIM(RTRIM(Login)))
         AND Login NOT LIKE N'%[^a-z0-9_]%' COLLATE Latin1_General_100_BIN2),
    CONSTRAINT CK_Users_Email CHECK
        (LEN(Email) BETWEEN 3 AND 254 AND Email = LOWER(LTRIM(RTRIM(Email)))),
    CONSTRAINT CK_Users_Names CHECK
        (LEN(LTRIM(RTRIM(FirstName))) BETWEEN 1 AND 50
         AND LEN(LTRIM(RTRIM(LastName))) BETWEEN 1 AND 50),
    CONSTRAINT CK_Users_Gender CHECK (Gender IN ('Female', 'Male', 'Other')),
    CONSTRAINT CK_Users_Profile CHECK
        (LEN(LTRIM(RTRIM(Address))) BETWEEN 1 AND 200 AND LEN(Phone) BETWEEN 7 AND 20),
    CONSTRAINT CK_Users_Hashes CHECK (LEN(PasswordHash) > 0 AND LEN(RecoveryWordHash) > 0)
);

CREATE UNIQUE INDEX IX_Users_Login ON dbo.Users(Login);
CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);

CREATE TABLE dbo.BankCards
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BankCards PRIMARY KEY,
    UserId INT NOT NULL,
    CardHolder NVARCHAR(100) NOT NULL,
    Brand VARCHAR(20) NOT NULL,
    LastFourDigits CHAR(4) NOT NULL,
    ExpirationMonth INT NOT NULL,
    ExpirationYear INT NOT NULL,
    CONSTRAINT CK_BankCards_Digits CHECK
        (LEN(LastFourDigits) = 4
         AND LastFourDigits NOT LIKE '%[^0-9]%' COLLATE Latin1_General_100_BIN2),
    CONSTRAINT CK_BankCards_Holder CHECK (LEN(LTRIM(RTRIM(CardHolder))) BETWEEN 1 AND 100),
    CONSTRAINT CK_BankCards_Brand CHECK (Brand IN ('Visa', 'Mastercard', 'Other')),
    CONSTRAINT CK_BankCards_Expiration CHECK
        (ExpirationMonth BETWEEN 1 AND 12 AND ExpirationYear BETWEEN 2020 AND 2100),
    CONSTRAINT FK_BankCards_Users_UserId FOREIGN KEY(UserId)
        REFERENCES dbo.Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_BankCards_UserId ON dbo.BankCards(UserId);

CREATE TABLE dbo.LoginAttempts
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LoginAttempts PRIMARY KEY,
    UserId INT NULL,
    LoginEntered NVARCHAR(30) NOT NULL,
    AttemptedAtUtc DATETIME2(0) NOT NULL,
    IsSuccessful BIT NOT NULL,
    CONSTRAINT CK_LoginAttempts_Login CHECK (LEN(LTRIM(RTRIM(LoginEntered))) BETWEEN 1 AND 30),
    CONSTRAINT FK_LoginAttempts_Users_UserId FOREIGN KEY(UserId)
        REFERENCES dbo.Users(Id) ON DELETE SET NULL
);

CREATE INDEX IX_LoginAttempts_UserId ON dbo.LoginAttempts(UserId);
COMMIT TRANSACTION;
