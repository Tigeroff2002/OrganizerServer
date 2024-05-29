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
    [Route("confirm")]
    public async Task<IActionResult> ConfirmUserAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var result =
            await _usersDataHandler.ConfirmUser(body, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [HttpPost]
    [Route("check_if_time_expired")]
    public async Task<IActionResult> CheckIfTimeExpiredAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var result =
            await _usersDataHandler.CheckIfTimeExpired(body, token);

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

    [Route("get_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetUserInfoAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var userInfoResponse = await _usersDataHandler.GetUserInfo(body, token);

        Debug.Assert(userInfoResponse != null);

        var get_json = JsonConvert.SerializeObject(userInfoResponse);

        return Ok(get_json);
    }

    [Route("get_all_users")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAllUsersAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var usersResponse = await _usersDataHandler.GetAllUsers(body, token);

        Debug.Assert(usersResponse != null);

        var get_json = JsonConvert.SerializeObject(usersResponse);

        return Ok(get_json);
    }

    [Route("get_admins")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAdminsAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var usersResponse = await _usersDataHandler.GetAdmins(body, token);

        Debug.Assert(usersResponse != null);

        var get_json = JsonConvert.SerializeObject(usersResponse);

        return Ok(get_json);
    }

    [Route("get_group_users")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetGroupUsersAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var usersResponse = await _usersDataHandler.GetUsersFromGroup(body, token);

        Debug.Assert(usersResponse != null);

        var get_json = JsonConvert.SerializeObject(usersResponse);

        return Ok(get_json);
    }

    [Route("get_not_group_users")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetNotGroupUsersAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var usersResponse = await _usersDataHandler.GetAllUsersNotInGroup(body, token);

        Debug.Assert(usersResponse != null);

        var get_json = JsonConvert.SerializeObject(usersResponse);

        return Ok(get_json);
    }

    [Route("get_group_users_not_in_event")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetAllGroupUsersNotInEventAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var usersResponse = await _usersDataHandler.GetGroupUsersNotInEvent(body, token);

        Debug.Assert(usersResponse != null);

        var get_json = JsonConvert.SerializeObject(usersResponse);

        return Ok(get_json);
    }

    private readonly IUsersDataHandler _usersDataHandler;
}
