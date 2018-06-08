using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCoreProjectionInvalidCastRepro
{
    public class ProjectionTests
    {
        private AppDbContext Context => new AppDbContext();
        private static bool _isInitialized = false;

        public ProjectionTests()
        {
            if (!_isInitialized)
            {
                using (var context = new AppDbContext())
                {
                    context.Database.EnsureCreated();
                }

                _isInitialized = true;
            }
        }

        // This throws System.NullReferenceException
        // More details on GitHub:
        // https://github.com/aspnet/EntityFrameworkCore/issues/12262
        [Fact]
        public async Task CanFilterProjectionWithCapturedVariable()
        {
            var users = await Context
                        .Users
                        .Select(u => new
                        {
                            IsLockedOut = u.LockoutEnd > System.DateTimeOffset.Now
                        })
                        .ToListAsync();
            Assert.Empty(users);
        }
    }

    public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var sqliteInMemoryConnectionString = InMemorySqliteHelper.GetSqliteInMemoryConnectionString();
            optionsBuilder.UseSqlite(sqliteInMemoryConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }

    public static class InMemorySqliteHelper
    {
        // This connection must be kept open for the lifetime of the process.
        // SQLite drops the database as soon as the last connection has been closed.
        private static SqliteConnection _connection;

        public static string GetSqliteInMemoryConnectionString()
        {
            var sqliteConnectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "in_memory_sqlite.db",
                Cache = SqliteCacheMode.Shared,
                Mode = SqliteOpenMode.Memory
            };
            var sqliteConnectionString = sqliteConnectionStringBuilder.ToString();
            if (_connection == null)
            {
                _connection = new SqliteConnection(sqliteConnectionString);
                _connection.Open();
            }

            return sqliteConnectionString;
        }
    }
}
