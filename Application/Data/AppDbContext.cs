#region

using Application.Data.Entity.Authorization;
using Application.Data.Entity.Log;
using Application.Data.Entity.Todo;
using Application.Data.Entity.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

#endregion

namespace Application.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public static readonly LoggerFactory LoggerFactory =
            new LoggerFactory(new[] { new DebugLoggerProvider((_, __) => true) });


        public DbSet<Todo> Todo { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            optionsBuilder.EnableSensitiveDataLogging(true);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("SampleApplication");

            base.OnModelCreating(builder);
        }
    }
}