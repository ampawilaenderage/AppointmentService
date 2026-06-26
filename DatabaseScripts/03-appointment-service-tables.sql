USE AppointmentDb;
GO

IF OBJECT_ID('dbo.Appointments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Appointments
    (
        Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Appointments PRIMARY KEY,
        UserId      INT NOT NULL,
        DoctorId    INT NULL,
        PatientName NVARCHAR(220) NOT NULL CONSTRAINT DF_Appointments_PatientName DEFAULT '',
        ProblemType NVARCHAR(100) NOT NULL CONSTRAINT DF_Appointments_ProblemType DEFAULT '',
        Time        DATETIME2 NOT NULL,
        Status      NVARCHAR(MAX) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT 'Pending'
    );
END;
GO

IF COL_LENGTH('dbo.Appointments', 'DoctorId') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD DoctorId INT NULL;
END;
GO

IF COL_LENGTH('dbo.Appointments', 'ProblemType') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD ProblemType NVARCHAR(100) NOT NULL CONSTRAINT DF_Appointments_ProblemType DEFAULT '';
END;
GO

IF COL_LENGTH('dbo.Appointments', 'PatientName') IS NULL
BEGIN
    ALTER TABLE dbo.Appointments ADD PatientName NVARCHAR(220) NOT NULL CONSTRAINT DF_Appointments_PatientName DEFAULT '';
END;
GO
