using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using GeoProfs.Application.Common.Interfaces;
using GeoProfs.Domain.Entities;
using GeoProfs.Domain.Exceptions;

// De rest van het bestand blijft hetzelfde...
namespace GeoProfs.Application.LeaveRequests.Commands
{
    public class SubmitLeaveRequestCommandHandler : IRequestHandler<SubmitLeaveRequestCommand, Guid>
    {
        private readonly ILeaveBalanceRepository _leaveBalanceRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmailService _emailService;

        public SubmitLeaveRequestCommandHandler(
            ILeaveBalanceRepository leaveBalanceRepository, 
            ILeaveRequestRepository leaveRequestRepository,
            IEmailService emailService)
        {
            _leaveBalanceRepository = leaveBalanceRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.StartDate >= request.EndDate)
            {
                throw new InvalidDateRangeException("De startdatum moet voor de einddatum liggen.");
            }

            var leaveBalance = await _leaveBalanceRepository.GetByUserIdAsync(request.UserId);
            var requestedDays = (request.EndDate - request.StartDate).Days + 1;

            if (leaveBalance.RemainingDays < requestedDays)
            {
                throw new InsufficientLeaveBalanceException("Onvoldoende verlofsaldo voor deze aanvraag.");
            }
            
            var leaveRequest = new LeaveRequest(
                request.UserId,
                request.StartDate,
                request.EndDate,
                request.Type,
                request.Reason
            );

            await _leaveRequestRepository.AddAsync(leaveRequest);
            
            var managerEmail = "manager@geoprofs.nl";
            await _emailService.SendEmailAsync(
                managerEmail, 
                "Nieuwe Verlofaanvraag", 
                $"Er is een nieuwe verlofaanvraag ingediend door gebruiker {request.UserId}.");

            return leaveRequest.Id;
        }
    }
}
