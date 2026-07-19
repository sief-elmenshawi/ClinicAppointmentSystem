using Clinic.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        // في مشروع حقيقي: هنا هيبقى فيه SendGrid / SMTP client
        _logger.LogInformation("📧 Email to {To} | Subject: {Subject} | Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}