using GeoProfs.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace GeoProfs.Application.Common.Interfaces
{
    public interface ILeaveRequestRepository
    {
        Task<LeaveRequest> GetByIdAsync(Guid id);
        Task AddAsync(LeaveRequest leaveRequest);
    }
}
