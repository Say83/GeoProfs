using GeoProfs.Application.Common.Interfaces;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GeoProfs.Infrastructure.Services
{
    // Dit is een gesimuleerde e-mail service die de output naar de debug console schrijft.
    // In een productieomgeving zou je hier MailKit of een andere e-mail API (zoals SendGrid) gebruiken.
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            Debug.WriteLine("===== Sending Email =====");
            Debug.WriteLine($"To: {to}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Body: {body}");
            Debug.WriteLine("=========================");
            
            return Task.CompletedTask;
        }
    }
}
