USE NotificationDb;
GO

IF OBJECT_ID('dbo.ProcessedMessages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcessedMessages
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProcessedMessages PRIMARY KEY,
        MessageId UNIQUEIDENTIFIER NULL,
        MessageType NVARCHAR(200) NOT NULL,
        AppointmentId INT NOT NULL,
        UserId INT NOT NULL,
        AppointmentTime DATETIME2 NOT NULL,
        ProcessedAtUtc DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL,
        Error NVARCHAR(1000) NULL
    );
END;
GO
