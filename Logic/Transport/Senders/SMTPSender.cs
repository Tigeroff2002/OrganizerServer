using Contracts.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Models.BusinessModels;
using Models.UserActionModels.NotificationModels;
using System.Net.Mail;
using System.Text;

namespace Logic.Transport.Senders;

public sealed class SMTPSender
    : ISMTPSender
{
    public SMTPSender(
        IOptions<SmtpConfiguration> options,
        ILogger<SMTPSender> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _smtpConfiguration = options.Value;

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Response> ConfirmAsync(ShortUserInfo shortUserInfo, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(shortUserInfo);

        token.ThrowIfCancellationRequested();

        var email = shortUserInfo.UserEmail;

        if (string.IsNullOrWhiteSpace(email))
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = "Provided user email is null, or empty, or consist from whitespaces";

            return await Task.FromResult(response1);
        }

        if (!IsValidEmail(email))
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = "Provided user email does not match email string requirment";

            return await Task.FromResult(response1);
        }

        var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        await smtpClient.ConnectAsync(
            _smtpConfiguration.Host,
            _smtpConfiguration.Port,
            useSsl: true,
            token);

        await smtpClient.AuthenticateAsync(
            _smtpConfiguration.FromAdress,
            _smtpConfiguration.FromPassword);

        _logger.LogInformation("Connected to smtp server");

        var randomLink = RandomURLGenerator.GenerateURL();

        var subject = "Registration new calendar app account";

        var body = new StringBuilder();

        body.Append($"Hello, {shortUserInfo.UserName}!\n");
        body.Append("Bellow is your account confirmation link. You need to open it in your browser page...\n");
        body.Append($"Or maybe we can call your to your phone number: {shortUserInfo.UserPhone}\n");
        body.Append($"Your confirmation link is here: {randomLink}.");

        var mailMessage = new MailMessage(
            new MailAddress(_smtpConfiguration.FromAdress),
            new MailAddress(email))
        {
            Subject = subject,
            Body = body.ToString()
        };

        var response = await smtpClient.SendAsync(
            MimeMessage.CreateFromMailMessage(mailMessage), token);

        _logger.LogInformation("Server response: {Response}", response);

        await smtpClient.DisconnectAsync(quit: true, token);

        var confirmationResponse = new Response();
        confirmationResponse.Result = true;
        confirmationResponse.OutInfo =
            $"Link confirmation was performed for user" +
            $" with email {email} with link: {randomLink}";

        Task.Delay(REGISTRATION_EMAIL_DELAY_SECONDS).GetAwaiter().GetResult();

        return confirmationResponse;
    }

    public async Task SendSMTPNotificationAsync(
        UserEmailReminderInfo model,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(model);

        token.ThrowIfCancellationRequested();

        var body = new StringBuilder();
        var numberMinutesOfOffset = model.TotalMinutes;

        var email = model.Email;
        var eventName = model.Description;
        var userName = model.UserName;
        var subject = model.Subject;

        body.Append($"Hello, {userName}!\n");
        body.Append(
            $"We are sending you a reminder that your event" +
            $" will start in less than {numberMinutesOfOffset} minutes.\n");
        body.Append(eventName);

        var mailMessage = new MailMessage(
            new MailAddress(_smtpConfiguration.FromAdress),
            new MailAddress(email))
        {
            Subject = subject,
            Body = body.ToString()
        };

        var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(
                _smtpConfiguration.Host,
                _smtpConfiguration.Port,
                useSsl: true,
                token);

            _logger.LogInformation("Connected to smtp server");

            await smtpClient.AuthenticateAsync(
                _smtpConfiguration.FromAdress,
                _smtpConfiguration.FromPassword);

            await smtpClient.SendAsync(
                MimeMessage.CreateFromMailMessage(mailMessage), token);

            _logger.LogInformation("New message to email {Email} was sended successfully", email);
        }

        catch (Exception ex)
        {
            _logger.LogWarning("Exception occured: {Message}", ex.Message);
        }
    }

    public async Task SendNotificationAsync(
        UserEmailNotificationInfo notification,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(notification);

        token.ThrowIfCancellationRequested();

        var body = new StringBuilder();

        var email = notification.Email;
        var description = notification.Description;
        var userName = notification.UserName;
        var subject = notification.Subject;

        body.Append($"Hello, {userName}!\n");
        body.Append($"{description} \n");

        var mailMessage = new MailMessage(
            new MailAddress(_smtpConfiguration.FromAdress),
            new MailAddress(email))
        {
            Subject = subject,
            Body = body.ToString()
        };

        var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(
                _smtpConfiguration.Host,
                _smtpConfiguration.Port,
                useSsl: true,
                token);

            _logger.LogInformation("Connected to smtp server");

            await smtpClient.AuthenticateAsync(
                _smtpConfiguration.FromAdress,
                _smtpConfiguration.FromPassword);

            await smtpClient.SendAsync(
                MimeMessage.CreateFromMailMessage(mailMessage), token);

            _logger.LogInformation("New message to email {Email} was sended successfully", email);
        }

        catch (Exception ex)
        {
            _logger.LogWarning("Exception occured: {Message}", ex.Message);
        }
    }

    private bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false; // suggested by @TK-421
        }
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }

    private const int REGISTRATION_EMAIL_DELAY_SECONDS = 0;

    private readonly SmtpConfiguration _smtpConfiguration;
    private readonly ILogger _logger;
}
