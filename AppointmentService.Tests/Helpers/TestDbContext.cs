using AppointmentService.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Tests.Helpers;

public static class TestDbContext
{
    public static AppDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }
}
