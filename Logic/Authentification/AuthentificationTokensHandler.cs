﻿using Contracts.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models.BusinessModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Logic.Authentification;

public sealed class AuthentificationTokensHandler
    : AuthenticationHandler<TokenAuthentificationOptions>
{
    public AuthentificationTokensHandler(
        IOptionsMonitor<TokenAuthentificationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock systemClock,
        ICommonUsersUnitOfWork commonUnitOfWork)
        : base(options, logger, encoder, systemClock)
    {
        _commonUnitOfWork = commonUnitOfWork
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        HttpRequestRewindExtensions.EnableBuffering(Request);

        using var streamReader = new StreamReader(
            Request.Body, 
            detectEncodingFromByteOrderMarks: false, 
            leaveOpen: true);

        var body = await streamReader.ReadToEndAsync();

        Request.Body.Position = 0;

        var model = JsonConvert.DeserializeObject<RequestWithToken>(body);

        if (model == null)
        {
            Logger.LogInformation("Start application - so no result for authentification");

            return await Task.FromResult(AuthenticateResult.NoResult());
        }

        Logger.LogInformation(
            "Current token for user with id {UserId}: {Token}",
            model.UserId,
            model.Token);

        var token = model.Token;

        var userId = model.UserId;

        if (string.IsNullOrEmpty(token))
        {
            return await Task.FromResult(AuthenticateResult.Fail("Token is null or empty"));
        }

        var authResponse = await ValidateUserToken(userId, token);

        if (!authResponse.Result)
        {
            return await Task.FromResult(AuthenticateResult.Fail(authResponse.OutInfo));
        }

        var claims = new[] 
        {
            new Claim("token", token) 
        };

        var identity = new ClaimsIdentity(
            claims, 
            nameof(AuthentificationTokensHandler));

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            this.Scheme.Name);

        return await Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private async Task<Response> ValidateUserToken(int userId, string token)
    {
        var existedUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(
                userId,
                CancellationToken.None);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Cant authentificate user with id {userId}" +
                $" cause it is not existed in db";

            return await Task.FromResult(response1);
        }

        var userTokens = _commonUnitOfWork
            .UserDevicesRepository
            .GetAllDevicesMapsAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult()
            .Where(x => x.UserId == userId && x.IsActive == true)
            .Select(x => x.FirebaseToken)
            .ToList();

        foreach (var userToken in userTokens)
        {
            if (userToken == token)
            {
                var response = new Response();
                response.Result = true;
                response.OutInfo = $"Authentification for user with id {userId} is succeeded";

                return await Task.FromResult(response);
            }
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo =
            $"Cant authentificate user" +
            $" with id {userId} cause token {token}" +
            $" is not present for this user";

        return await Task.FromResult(response2);
    }

    private readonly ICommonUsersUnitOfWork _commonUnitOfWork;
}
