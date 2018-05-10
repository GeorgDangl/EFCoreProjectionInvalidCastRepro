using System;
using System.Linq;
using System.Threading.Tasks;
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
                    context.EmailTemplates.Add(new EmailTemplate {Id = Guid.NewGuid(), TemplateType = EmailTemplateType.PasswordResetRequest});
                    context.SaveChanges();
                }

                _isInitialized = true;
            }
        }

        // Throws System.InvalidCastException :
        // Invalid cast from 'EFCoreProjectionInvalidCastRepro.EmailTemplateTypeDto'
        // to 'EFCoreProjectionInvalidCastRepro.EmailTemplateType'.
        [Fact]
        public async Task CanFilterProjectionWithCapturedVariable()
        {
            var templateType = EmailTemplateTypeDto.PasswordResetRequest;
            var template = await Context
                .EmailTemplates
                .Select(t => new EmailTemplateDto {Id = t.Id, TemplateType = (EmailTemplateTypeDto) t.TemplateType})
                .Where(t => t.TemplateType == templateType)
                .FirstOrDefaultAsync();
            Assert.NotNull(template);
        }

        // This one succeeds, the filter condition is inlined
        [Fact]
        public async Task CanFilterProjectionWithInlineVariable()
        {
            var template = await Context
                .EmailTemplates
                .Select(t => new EmailTemplateDto {Id = t.Id, TemplateType = (EmailTemplateTypeDto) t.TemplateType})
                .Where(t => t.TemplateType == EmailTemplateTypeDto.PasswordResetRequest)
                .FirstOrDefaultAsync();
            Assert.NotNull(template);
        }

        // The same error happends for non-async materialization
        [Fact]
        public void CanFilterProjectionWithCapturedVariableNonAsync()
        {
            var templateType = EmailTemplateTypeDto.PasswordResetRequest;
            var template = Context
                .EmailTemplates
                .Select(t => new EmailTemplateDto { Id = t.Id, TemplateType = (EmailTemplateTypeDto)t.TemplateType })
                .Where(t => t.TemplateType == templateType)
                .FirstOrDefault();
            Assert.NotNull(template);
        }
    }

    public class EmailTemplate
    {
        public Guid Id { get; set; }
        public EmailTemplateType TemplateType { get; set; }
    }

    public enum EmailTemplateType
    {
        PasswordResetRequest = 0,
        EmailConfirmation = 1
    }

    public class EmailTemplateDto
    {
        public Guid Id { get; set; }
        public EmailTemplateTypeDto TemplateType { get; set; }
    }

    public enum EmailTemplateTypeDto
    {
        PasswordResetRequest = 0,
        EmailConfirmation = 1
    }

    public class AppDbContext : DbContext
    {
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

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
