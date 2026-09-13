using Clinic.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Clinic.Infrastructure.Services;

public class SmsSender : ISmsSender
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string phoneNumber, string message)
    {
        // في مشروع حقيقي: هنا هيبقى فيه Twilio / SMS provider client
        _logger.LogInformation("📱 SMS to {Phone} | {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}