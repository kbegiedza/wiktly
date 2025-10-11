using Microsoft.AspNetCore.Identity.UI.Services;

namespace Wiktly.Web.Areas.Identity.Services;

public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Sending email to {Email} with subject {Subject}.", email, subject);

        return Task.CompletedTask;
    }
}