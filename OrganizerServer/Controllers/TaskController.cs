using Contracts;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using static Google.Apis.Requests.BatchRequest;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TaskController : ControllerBase
{
    public TaskController(
        ITasksHandler tasksHandler)
    {
        _tasksHandler = tasksHandler 
            ?? throw new ArgumentNullException(nameof(tasksHandler));
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateTask(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _tasksHandler.TryCreateTask(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("update_task_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateTaskParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _tasksHandler.TryUpdateTask(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("delete_task")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteTaskByReporter(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _tasksHandler.TryDeleteTask(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    [Route("get_task_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetTaskInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var response = await _tasksHandler.GetTaskInfo(body, token);

        var json = JsonConvert.SerializeObject(response);

        return response.Result ? Ok(json) : BadRequest(json);
    }

    private readonly ITasksHandler _tasksHandler;
}

