using Microsoft.EntityFrameworkCore;
using SoareAlexGameServer.Infrastructure.Entities.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoareAlexGameServer.Infrastructure.Data
{
    public class SqliteDbContext : DbContext
    {
        public DbSet<PlayerProfile> Players { get; set; }

        public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=database.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlayerProfile>().OwnsMany(p => p.Resources);
        }

    }
}
