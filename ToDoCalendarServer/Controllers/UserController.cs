using Contracts.Request;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("users")]
public sealed class UserController : ControllerBase
{
    public UserController(
        ILogger<UserController> logger,
        IUsersDataHandler usersDataHandler,
        IDeserializer<UserRegistrationData> usersRegistrationDataDeserializer,
        IDeserializer<UserLoginData> usersLoginDataDeserializer,
        IDeserializer<UserInfoById> userInfoByIdDeserializer) 
    {
        _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        _usersDataHandler = usersDataHandler 
            ?? throw new ArgumentNullException(nameof(usersDataHandler));

        _usersRegistrationDataDeserializer = usersRegistrationDataDeserializer
            ?? throw new ArgumentNullException(nameof(usersRegistrationDataDeserializer));
        _usersLoginDataDeserializer = usersLoginDataDeserializer
            ?? throw new ArgumentNullException(nameof(usersLoginDataDeserializer));

        _userInfoByIdDeserializer = userInfoByIdDeserializer
            ?? throw new ArgumentNullException(nameof(userInfoByIdDeserializer));
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUserAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var userRegistrationData = 
            _usersRegistrationDataDeserializer.Deserialize(body);

        var result = 
            await _usersDataHandler.TryRegisterUser(userRegistrationData, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginUserAsyns(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var userLoginData = _usersLoginDataDeserializer.Deserialize(body);

        var result =
            await _usersDataHandler.TryLoginUser(userLoginData, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetInfoAsync(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var userInfoByIdRequest = _userInfoByIdDeserializer.Deserialize(body);
        
        var userInfoResponse = await _usersDataHandler.GetUserInfo(userInfoByIdRequest, token);

        Debug.Assert(userInfoResponse != null);

        var get_json = JsonConvert.SerializeObject(userInfoResponse);

        return Ok(get_json);
    }

    [Route("update_user_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateUserInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var updateUserInfo = JsonConvert.DeserializeObject<UserUpdateInfoDTO>(body);

        Debug.Assert(updateUserInfo != null);

        var response = await _usersDataHandler.UpdateUserInfo(updateUserInfo, token);

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_user_role")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateUserRole(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var updateUserRole = JsonConvert.DeserializeObject<UserUpdateRoleDTO>(body);

        Debug.Assert(updateUserRole != null);

        var response = await _usersDataHandler.UpdateUserRoleAsync(updateUserRole, token);

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    private readonly ILogger<UserController> _logger;
    private readonly IUsersDataHandler _usersDataHandler;
    private readonly IDeserializer<UserRegistrationData> _usersRegistrationDataDeserializer;
    private readonly IDeserializer<UserLoginData> _usersLoginDataDeserializer;
    private readonly IDeserializer<UserInfoById> _userInfoByIdDeserializer;
}
