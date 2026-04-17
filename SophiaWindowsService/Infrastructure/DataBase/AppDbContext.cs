using System.Data.Entity;
using SophiaWindowsService.Application.Extensions;

namespace SophiaWindowsService.Infrastructure.DataBase
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(string connectionName = null)
            : base($"name={(string.IsNullOrWhiteSpace(connectionName) ? ConfigExtensions.DbConnectionName : connectionName)}")
        { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(ConfigExtensions.DbSchema);
            modelBuilder.Configurations.AddFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}