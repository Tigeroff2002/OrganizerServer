using Contracts.Response;
using Logic.Abstractions;
using Logic.Transport.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;

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
        IDeserializer<UserInfoById> userInfoByIdDeserializer,
        ISerializer<User> userInfoSerializer) 
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
        _userInfoSerializer = userInfoSerializer
            ?? throw new ArgumentNullException(nameof(userInfoSerializer));
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUserAsyns(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

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
        var body = await ReadRequestBodyAsync();

        var userLoginData = _usersLoginDataDeserializer.Deserialize(body);

        var result =
            await _usersDataHandler.TryLoginUser(userLoginData, token);

        var json = JsonConvert.SerializeObject(result);

        return result.Result ? Ok(json) : BadRequest(json);
    }

    [HttpGet]
    [Route("get_info")]
    [Authorize]
    public async Task<IActionResult> GetInfoAsync(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var userInfoByIdRequest = _userInfoByIdDeserializer.Deserialize(body);

        var user = await _usersDataHandler.GetUserInfo(userInfoByIdRequest, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"No user with id {userInfoByIdRequest.UserId} in system";

            var json = JsonConvert.SerializeObject(response1);

            return Ok(JsonConvert.SerializeObject(json));
        }

        var response = new GetResponse();
        response.Result = true;
        response.OutInfo = $"User with id {userInfoByIdRequest.UserId} was found";
        response.RequestedInfo = _userInfoSerializer.Serialize(user);

        var get_json = JsonConvert.SerializeObject(response);

        return Ok(get_json);
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly ILogger<UserController> _logger;
    private readonly IUsersDataHandler _usersDataHandler;
    private readonly IDeserializer<UserRegistrationData> _usersRegistrationDataDeserializer;
    private readonly IDeserializer<UserLoginData> _usersLoginDataDeserializer;
    private readonly IDeserializer<UserInfoById> _userInfoByIdDeserializer;
    private readonly ISerializer<User> _userInfoSerializer;
}
