using System;
using GeoProfs.Domain.Common;
using GeoProfs.Domain.Enums;

namespace GeoProfs.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        public Guid UserId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public LeaveType Type { get; private set; }
        public string Reason { get; private set; }
        public LeaveRequestStatus Status { get; private set; }
        public Guid? ApprovedByManagerId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Private constructor voor ORM (Entity Framework).
        private LeaveRequest() {}

        // Publieke constructor om een nieuwe, valide aanvraag te creëren.
        public LeaveRequest(Guid userId, DateTime startDate, DateTime endDate, LeaveType type, string reason)
        {
            if (userId == Guid.Empty) throw new ArgumentException("UserId mag niet leeg zijn.", nameof(userId));
            if (endDate < startDate) throw new ArgumentException("Einddatum kan niet voor startdatum liggen.", nameof(endDate));

            Id = Guid.NewGuid();
            UserId = userId;
            StartDate = startDate;
            EndDate = endDate;
            Type = type;
            Reason = SanitizeReason(reason);
            Status = LeaveRequestStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void Approve(Guid managerId)
        {
            if (Status != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Alleen een aanvraag met status 'Pending' kan worden goedgekeurd.");
            }
            Status = LeaveRequestStatus.Approved;
            ApprovedByManagerId = managerId;
        }

        public void Reject(Guid managerId)
        {
            if (Status != LeaveRequestStatus.Pending)
            {
                throw new InvalidOperationException("Alleen een aanvraag met status 'Pending' kan worden afgekeurd.");
            }
            Status = LeaveRequestStatus.Rejected;
            ApprovedByManagerId = managerId;
        }
        
        private string SanitizeReason(string rawReason)
        {
            if (string.IsNullOrWhiteSpace(rawReason)) return string.Empty;
            // Simpele sanitization om script-tags te voorkomen (preventie XSS).
            // Een robuustere oplossing gebruikt een library zoals HtmlSanitizer.
            return rawReason.Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
