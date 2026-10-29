-- Safe upgrade for the existing project database; no existing rows are deleted.
IF DB_NAME() <> N'GuestRegistrationCourse' AND DB_NAME() NOT LIKE N'GuestRegistrationTests[_]%'
    THROW 51000, 'Select the project database before upgrading.', 1;
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL OR OBJECT_ID(N'dbo.BankCards', N'U') IS NULL OR OBJECT_ID(N'dbo.LoginAttempts', N'U') IS NULL
    THROW 51001, 'Create the three project tables before upgrading.', 1;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;
IF COL_LENGTH(N'dbo.Users', N'RowInsertTime') IS NULL
    ALTER TABLE dbo.Users ADD RowInsertTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_Users_RowInsertTime DEFAULT SYSUTCDATETIME() WITH VALUES;

IF COL_LENGTH(N'dbo.Users', N'UpdateTime') IS NULL
    ALTER TABLE dbo.Users ADD UpdateTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_Users_UpdateTime DEFAULT SYSUTCDATETIME() WITH VALUES;

EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Users_AuditTime ON dbo.Users
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL(OBJECT_ID(N''dbo.TR_Users_AuditTime''), ''AFTER'', ''DML'') > 1 RETURN;
    DECLARE @now DATETIME2(7) = SYSUTCDATETIME();
    UPDATE target
    SET RowInsertTime = previous.RowInsertTime,
        UpdateTime = CASE WHEN @now > previous.UpdateTime THEN @now ELSE DATEADD(NANOSECOND, 100, previous.UpdateTime) END
    FROM dbo.Users AS target
    INNER JOIN inserted AS changed ON changed.Id = target.Id
    INNER JOIN deleted AS previous ON previous.Id = target.Id;
END;');

IF COL_LENGTH(N'dbo.BankCards', N'RowInsertTime') IS NULL
    ALTER TABLE dbo.BankCards ADD RowInsertTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_BankCards_RowInsertTime DEFAULT SYSUTCDATETIME() WITH VALUES;

IF COL_LENGTH(N'dbo.BankCards', N'UpdateTime') IS NULL
    ALTER TABLE dbo.BankCards ADD UpdateTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_BankCards_UpdateTime DEFAULT SYSUTCDATETIME() WITH VALUES;

EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_BankCards_AuditTime ON dbo.BankCards
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL(OBJECT_ID(N''dbo.TR_BankCards_AuditTime''), ''AFTER'', ''DML'') > 1 RETURN;
    DECLARE @now DATETIME2(7) = SYSUTCDATETIME();
    UPDATE target
    SET RowInsertTime = previous.RowInsertTime,
        UpdateTime = CASE WHEN @now > previous.UpdateTime THEN @now ELSE DATEADD(NANOSECOND, 100, previous.UpdateTime) END
    FROM dbo.BankCards AS target
    INNER JOIN inserted AS changed ON changed.Id = target.Id
    INNER JOIN deleted AS previous ON previous.Id = target.Id;
END;');

IF COL_LENGTH(N'dbo.LoginAttempts', N'RowInsertTime') IS NULL
    ALTER TABLE dbo.LoginAttempts ADD RowInsertTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_LoginAttempts_RowInsertTime DEFAULT SYSUTCDATETIME() WITH VALUES;

IF COL_LENGTH(N'dbo.LoginAttempts', N'UpdateTime') IS NULL
    ALTER TABLE dbo.LoginAttempts ADD UpdateTime DATETIME2(7) NOT NULL
        CONSTRAINT DF_LoginAttempts_UpdateTime DEFAULT SYSUTCDATETIME() WITH VALUES;

EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_LoginAttempts_AuditTime ON dbo.LoginAttempts
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF TRIGGER_NESTLEVEL(OBJECT_ID(N''dbo.TR_LoginAttempts_AuditTime''), ''AFTER'', ''DML'') > 1 RETURN;
    DECLARE @now DATETIME2(7) = SYSUTCDATETIME();
    UPDATE target
    SET RowInsertTime = previous.RowInsertTime,
        UpdateTime = CASE WHEN @now > previous.UpdateTime THEN @now ELSE DATEADD(NANOSECOND, 100, previous.UpdateTime) END
    FROM dbo.LoginAttempts AS target
    INNER JOIN inserted AS changed ON changed.Id = target.Id
    INNER JOIN deleted AS previous ON previous.Id = target.Id;
END;');
COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
