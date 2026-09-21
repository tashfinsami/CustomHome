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
    }
}