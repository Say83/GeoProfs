using GeoProfs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GeoProfs.Infrastructure.Persistence.Context
{
    public class GeoProfsDbContext : DbContext
    {
        public GeoProfsDbContext(DbContextOptions<GeoProfsDbContext> options) : base(options)
        {
        }

        // Definieer de tabellen in de database
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        // Voeg hier andere DbSet<T> properties toe voor andere entities

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Past alle configuraties toe die in de assembly zijn gedefinieerd.
            // Dit is een schone manier om entity-configuraties te organiseren.
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}
