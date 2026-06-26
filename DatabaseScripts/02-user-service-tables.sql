USE UserDb;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserId NVARCHAR(50) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        FullName NVARCHAR(220) NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(200) NOT NULL,
        PasswordSalt NVARCHAR(200) NOT NULL,
        CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF COL_LENGTH('dbo.Users', 'UserId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD UserId NVARCHAR(50) NULL;
    EXEC('UPDATE dbo.Users SET UserId = CONCAT(''legacy-'', Id) WHERE UserId IS NULL');
    ALTER TABLE dbo.Users ALTER COLUMN UserId NVARCHAR(50) NOT NULL;
END;
GO

IF COL_LENGTH('dbo.Users', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD FirstName NVARCHAR(100) NOT NULL CONSTRAINT DF_Users_FirstName DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Users', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LastName NVARCHAR(100) NOT NULL CONSTRAINT DF_Users_LastName DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Users', 'PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordHash NVARCHAR(200) NOT NULL CONSTRAINT DF_Users_PasswordHash DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Users', 'PasswordSalt') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PasswordSalt NVARCHAR(200) NOT NULL CONSTRAINT DF_Users_PasswordSalt DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Users', 'CreatedAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD CreatedAtUtc DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME();
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users')
      AND name = 'Email'
      AND max_length = -1
)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE LEN(Email) > 256)
    BEGIN
        THROW 50001, 'Cannot convert Users.Email to NVARCHAR(256) because at least one email is longer than 256 characters.', 1;
    END;

    ALTER TABLE dbo.Users ALTER COLUMN Email NVARCHAR(256) NOT NULL;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Users')
      AND name = 'FullName'
      AND max_length = -1
)
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE LEN(FullName) > 220)
    BEGIN
        THROW 50002, 'Cannot convert Users.FullName to NVARCHAR(220) because at least one full name is longer than 220 characters.', 1;
    END;

    ALTER TABLE dbo.Users ALTER COLUMN FullName NVARCHAR(220) NOT NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_UserId' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_UserId ON dbo.Users(UserId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
END;
GO

IF COL_LENGTH('dbo.Users', 'Role') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT 'Patient';
END;
GO

IF COL_LENGTH('dbo.Users', 'Specialization') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Specialization NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('dbo.Users', 'Gender') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Gender NVARCHAR(10) NOT NULL CONSTRAINT DF_Users_Gender DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Users', 'Age') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Age INT NOT NULL CONSTRAINT DF_Users_Age DEFAULT 0;
END;
GO
