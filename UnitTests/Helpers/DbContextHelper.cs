using System.IO;

using Domain.Database;

using Microsoft.EntityFrameworkCore;

namespace UnitTests.Helpers
{
    internal static class DbContextHelper
    {
        public static DogesDbContext CreateTestDb()
        {
            string tempFile = Path.GetTempFileName();
            return CreateTestDb($"Data Source={tempFile}");
        }
        public static DogesDbContext CreateTestDb(string connectionString)
        {
            DbContextOptions<DogesDbContext> options = new DbContextOptionsBuilder<DogesDbContext>()
                .UseSqlite(connectionString)
                .Options;

            DogesDbContext dbContext = new(options);
            dbContext.Database.Migrate();

            return dbContext;
        }
    }
}