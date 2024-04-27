using Contracts.Request;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Models.UserActionModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("users")]
public sealed class UserController : ControllerBase
{
    public UserController(IUsersDataHandler usersDataHandler) 
    {
        _usersDataHandler = usersDataHandler 
            ?? throw new ArgumentNullException(nameof(usersDataHandler));
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUserAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var result = 
            await _usersDataHandler.TryRegisterUser(body, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginUserAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var result =
            await _usersDataHandler.TryLoginUser(body, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> LogoutUserAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var result = await _usersDataHandler.TryLogoutUser(body, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetInfoAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);
        
        var userInfoResponse = await _usersDataHandler.GetUserInfo(body, token);

        Debug.Assert(userInfoResponse != null);

        var get_json = JsonConvert.SerializeObject(userInfoResponse);

        return Ok(get_json);
    }

    [Route("update_user_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateUserInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _usersDataHandler.UpdateUserInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_user_role")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateUserRole(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _usersDataHandler.UpdateUserRoleAsync(body, token);

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    private readonly IUsersDataHandler _usersDataHandler;
}
