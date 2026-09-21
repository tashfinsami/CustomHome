using Microsoft.EntityFrameworkCore;
using CustomHome.Models;

namespace CustomHome.Data
{
    public class ServiceStationContext : DbContext
    {
        public ServiceStationContext(DbContextOptions<ServiceStationContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceToken> ServiceTokens { get; set; }

        public DbSet<QueueSettings> QueueSettings { get; set; }

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueSettings>().HasData(
                new QueueSettings
                {
                    Id = 1,
                    MaxWaiting = 5,
                    MaxServing = 2
                }
            );
        }
    }
}