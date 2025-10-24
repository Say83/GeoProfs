using GeoProfs.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace GeoProfs.Application.Common.Interfaces
{
    public interface IGeoProfsDbContext
    {
        DbSet<LeaveRequest> LeaveRequests { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
