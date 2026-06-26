-- ============================================================
-- 06-seed-data.sql
-- NOTE: Password hashes cannot be inserted via SQL because they
-- use PBKDF2-SHA256 (computed by the application).
-- Sample users are seeded automatically by UserService on first
-- startup when the Users table is empty (DataSeeder.cs).
--
-- Doctors  → password: Doc@123
-- Patients → password: abc@321
-- ============================================================

USE UserDb;
GO

-- ── Verify seed ran ──────────────────────────────────────────
SELECT
    Id,
    UserId,
    FullName,
    Role,
    Specialization,
    Gender,
    Age,
    Email
FROM dbo.Users
ORDER BY Role, FullName;
GO

-- ── Sample appointments (run AFTER services have started and
--    users are seeded, replace UserId / DoctorId with real IDs)
-- ── Uncomment and adjust once you know the actual IDs
-- USE AppointmentDb;
-- GO
-- INSERT INTO dbo.Appointments (UserId, DoctorId, PatientName, ProblemType, Time, Status)
-- VALUES
--   (13, 1,  'John Smith',    'Heart Problems',  DATEADD(day,  2, GETUTCDATE()), 'Pending'),
--   (13, 1,  'John Smith',    'Heart Problems',  DATEADD(day,  7, GETUTCDATE()), 'Confirmed'),
--   (14, 2,  'Mary Johnson',  'Neurology',        DATEADD(day,  3, GETUTCDATE()), 'Pending'),
--   (15, 3,  'William Jones', 'Bone & Joint Problems', DATEADD(day, 10, GETUTCDATE()), 'Pending'),
--   (16, 6,  'Susan Miller',  'Women''s Health',  DATEADD(day,  5, GETUTCDATE()), 'Confirmed'),
--   (17, 12, 'Thomas Wilson', 'General Checkup',  DATEADD(day, 14, GETUTCDATE()), 'Pending');
-- GO
