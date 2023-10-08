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

        return result ? Ok() : BadRequest();
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> LoginUserAsyns(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var userLoginData = _usersLoginDataDeserializer.Deserialize(body);

        var result =
            await _usersDataHandler.TryLoginUser(userLoginData, token);

        return result ? Ok() : BadRequest();
    }

    [HttpGet]
    [Route("get_info")]
    [Authorize]
    public async Task<string> GetInfoAsync(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var userInfoByIdRequest = _userInfoByIdDeserializer.Deserialize(body);

        var user = await _usersDataHandler.GetUserInfo(userInfoByIdRequest, token);

        if (user == null)
        {
            var notFoundResponse =
                new UserBadRequest
                {
                    UserId = userInfoByIdRequest.UserId,
                    Message = "This user was not found in DB"
                };

            return JsonConvert.SerializeObject(notFoundResponse);
        }

        return _userInfoSerializer.Serialize(user);
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
