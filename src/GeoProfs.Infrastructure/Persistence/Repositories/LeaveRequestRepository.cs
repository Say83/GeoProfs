using GeoProfs.Application.Common.Interfaces;
using GeoProfs.Domain.Entities;
using GeoProfs.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GeoProfs.Infrastructure.Persistence.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly GeoProfsDbContext _context;

        public LeaveRequestRepository(GeoProfsDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LeaveRequest leaveRequest)
        {
            await _context.LeaveRequests.AddAsync(leaveRequest);
            // De SaveChangesAsync wordt door een Unit of Work patroon afgehandeld
            // in een echte app, maar voor nu is dit voldoende.
            await _context.SaveChangesAsync();
        }

        public async Task<LeaveRequest> GetByIdAsync(Guid id)
        {
            // AsNoTracking() is een performance-optimalisatie voor read-only operaties.
            return await _context.LeaveRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(lr => lr.Id == id);
        }
    }
}
