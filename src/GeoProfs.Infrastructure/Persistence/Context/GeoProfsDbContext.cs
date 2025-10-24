using GeoProfs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using GeoProfs.Application.Common.Interfaces; // TOEGEVOEGD

namespace GeoProfs.Infrastructure.Persistence.Context
{
    // We implementeren de interface zodat de Application laag deze ook kan gebruiken indien nodig
    public class GeoProfsDbContext : DbContext, IGeoProfsDbContext
    {
        public GeoProfsDbContext(DbContextOptions<GeoProfsDbContext> options) : base(options)
        {
        }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }
    }
}
