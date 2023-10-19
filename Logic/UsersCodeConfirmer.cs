using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Logic;

public sealed class UsersCodeConfirmer
    : IUsersCodeConfirmer
{
    public UsersCodeConfirmer(
        IOptions<SmtpConfiguration> options,
        ILogger<UsersCodeConfirmer> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _configuration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ConfirmAsync(string userEmail, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            throw new ArgumentException();
        }

        token.ThrowIfCancellationRequested();

        var smtpClient = new SmtpClient(_configuration.Host)
        {
            Port = int.Parse(_configuration.Port),
            UseDefaultCredentials = false,
            Credentials =
                new NetworkCredential(
                    _configuration.Username,
                    _configuration.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = false
        };

        _logger.LogInformation(
            "Smtp client has been created with credentials from config");

        var code = 
            RandomNumberGenerator.GetInt32(10000000).ToString().PadLeft(6, '0');

        var subject = "Registration your new calendar app account";

        var body = new StringBuilder();

        body.Append("Hello!\n");
        body.Append("Bellow is your registration code. You need to write to your mobile app...\n");
        body.Append($"Your 6-digit confirmation code: {code}");

        _logger.LogInformation("Server email adress is {Host}", _configuration.Host);


        var message = new MailMessage();
        message.From = new MailAddress(_configuration.Host);
        message.To.Add(userEmail);
        message.Subject = subject;
        message.Body = body.ToString();

        await smtpClient.SendMailAsync(message, token);

        return true;
    }

    private readonly SmtpConfiguration _configuration;
    private readonly ILogger _logger;
}
