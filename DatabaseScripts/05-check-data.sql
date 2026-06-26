SELECT *
FROM UserDb.dbo.Users
ORDER BY CreatedAtUtc DESC;

SELECT *
FROM AppointmentDb.dbo.Appointments
ORDER BY Time DESC;

SELECT *
FROM NotificationDb.dbo.ProcessedMessages
ORDER BY ProcessedAtUtc DESC;
