using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService.Tests.Helpers;

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
