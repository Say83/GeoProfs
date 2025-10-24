using System;
using System.Threading.Tasks;

namespace GeoProfs.Application.Common.Interfaces
{
    // In een echte app zou dit een complexer object retourneren.
    public class LeaveBalance
    {
        public int RemainingDays { get; set; }
    }

    public interface ILeaveBalanceRepository
    {
        Task<LeaveBalance> GetByUserIdAsync(Guid userId);
    }
}
