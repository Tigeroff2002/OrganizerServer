using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;
using Contracts.Response;
using Models.BusinessModels;

namespace Logic;

public sealed class UserEmailConfirmer
    : IUserEmailConfirmer
{
    public UserEmailConfirmer(
        IOptions<SmtpConfiguration> options,
        ILogger<UserEmailConfirmer> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _configuration = options.Value;

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
            _configuration.Host,
            _configuration.Port,
            useSsl: true,
            token);

        await smtpClient.AuthenticateAsync(
            _configuration.FromAdress,
            _configuration.FromPassword);

        _logger.LogInformation("Connected to smtp server");

        var randomLink = RandomURLGenerator.GenerateURL();

        var subject = "Registration new calendar app account";

        var body = new StringBuilder();

        body.Append($"Hello, {shortUserInfo.UserName}!\n");
        body.Append("Bellow is your account confirmation link. You need to open it in your browser page...\n");
        body.Append($"Or maybe we can call your to your phone number: {shortUserInfo.UserPhone}\n");
        body.Append($"Your confirmation link is here: {randomLink}.");

        var mailMessage = new MailMessage(
            new MailAddress(_configuration.FromAdress),
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

        Task.Delay(30_000).GetAwaiter().GetResult();

        return confirmationResponse;
    }

    bool IsValidEmail(string email)
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

    private readonly SmtpConfiguration _configuration;
    private readonly ILogger _logger;
}
