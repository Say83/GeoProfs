using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using GeoProfs.Application.Common.Interfaces;
using GeoProfs.Domain.Entities;
using GeoProfs.Domain.Exceptions;

namespace GeoProfs.Application.LeaveRequests.Commands
{
    // Handler: Verwerkt het command.
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
            // 1. Business Rule: Controleer of de datums geldig zijn.
            if (request.StartDate >= request.EndDate)
            {
                throw new InvalidDateRangeException("De startdatum moet voor de einddatum liggen.");
            }

            // 2. Business Rule: Haal het verlofsaldo op en controleer of er genoeg dagen zijn.
            var leaveBalance = await _leaveBalanceRepository.GetByUserIdAsync(request.UserId);
            var requestedDays = (request.EndDate - request.StartDate).Days + 1;

            if (leaveBalance.RemainingDays < requestedDays)
            {
                throw new InsufficientLeaveBalanceException("Onvoldoende verlofsaldo voor deze aanvraag.");
            }
            
            // 3. Domeinlogica: Maak een nieuw LeaveRequest object aan.
            var leaveRequest = new LeaveRequest(
                request.UserId,
                request.StartDate,
                request.EndDate,
                request.Type,
                request.Reason
            );

            // 4. Data persistentie: Sla de nieuwe aanvraag op via de repository-interface.
            await _leaveRequestRepository.AddAsync(leaveRequest);
            
            // 5. Neveneffect: Stuur een notificatie naar de manager.
            var managerEmail = "manager@geoprofs.nl"; // In een echte app wordt dit dynamisch opgehaald.
            await _emailService.SendEmailAsync(
                managerEmail, 
                "Nieuwe Verlofaanvraag", 
                $"Er is een nieuwe verlofaanvraag ingediend door gebruiker {request.UserId}.");

            // 6. Retourneer de ID van de nieuwe aanvraag.
            return leaveRequest.Id;
        }
    }
}
