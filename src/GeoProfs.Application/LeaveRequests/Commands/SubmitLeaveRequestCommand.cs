using MediatR;
using System;
using GeoProfs.Domain.Enums;

namespace GeoProfs.Application.LeaveRequests.Commands
{
    // Command: een data transfer object (DTO) dat de intentie van de gebruiker representeert.
    public class SubmitLeaveRequestCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; } // Wordt door de controller ingevuld, niet door de client.
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType Type { get; set; }
        public string Reason { get; set; }
    }
}
