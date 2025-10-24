using GeoProfs.Application.Common.Interfaces;
using System;
using System.Threading.Tasks;

namespace GeoProfs.Infrastructure.Persistence.Repositories
{
    // Dit is een gesimuleerde repository. In een echte app zou deze
    // data uit een HR-systeem of een andere database halen.
    public class LeaveBalanceRepository : ILeaveBalanceRepository
    {
        public Task<LeaveBalance> GetByUserIdAsync(Guid userId)
        {
            // We simuleren dat elke gebruiker 25 dagen verlof heeft.
            var simulatedBalance = new LeaveBalance { RemainingDays = 25 };
            return Task.FromResult(simulatedBalance);
        }
    }
}
