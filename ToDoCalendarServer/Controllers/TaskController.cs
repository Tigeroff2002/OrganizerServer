using Contracts;
using Contracts.Request;
using Contracts.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Models.Enums;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TaskController : ControllerBase
{
    public TaskController(
        ITasksRepository tasksRepository,
        IUsersRepository usersRepository)
    {
        _tasksRepository = tasksRepository
            ?? throw new ArgumentNullException(nameof(tasksRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [HttpPost]
    [Route("create")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateTask(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var taskToCreate = JsonConvert.DeserializeObject<TaskInputDTO>(body);

        Debug.Assert(taskToCreate != null);

        var reporterId = taskToCreate.UserId;
        var implementerId = taskToCreate.ImplementerId;

        var reporter = await _usersRepository.GetUserByIdAsync(reporterId, token);
        var implementer = await _usersRepository.GetUserByIdAsync(implementerId, token);

        if (reporter == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Task has not been created cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        if (implementer == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = $"Task has not been created cause implementer was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var task = new UserTask
        {
            Caption = taskToCreate.TaskCaption,
            Description = taskToCreate.TaskDescription,
            TaskType = taskToCreate.TaskType,
            TaskStatus = taskToCreate.TaskStatus,
            ReporterId = reporter.Id,
            ImplementerId = implementer.Id
        };

        if (task.TaskType is TaskType.AbstractGoal or TaskType.MeetingPresense)
        {
            task.ImplementerId = task.ReporterId;
        }

        await _tasksRepository.AddAsync(task, token);

        _tasksRepository.SaveChanges();

        var taskId = task.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo = 
            $"New task with id = {taskId}" +
            $" for user '{implementer.UserName}' implementation was created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_task_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateTaskParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var taskUpdateParams = JsonConvert.DeserializeObject<TaskInputWithIdDTO>(body);

        Debug.Assert(taskUpdateParams != null);

        var taskId = taskUpdateParams.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(
            taskUpdateParams.TaskId, token);

        var reporterId = taskUpdateParams.UserId;
        var implementerId = taskUpdateParams.ImplementerId;

        if (existedTask != null)
        {
            var reporter = await _usersRepository.GetUserByIdAsync(reporterId, token);
            var implementer = await _usersRepository.GetUserByIdAsync(implementerId, token);

            if (reporter != null && implementer != null)
            {
                existedTask.Reporter = reporter;
                existedTask.Implementer = implementer;
            }

            if (existedTask.Reporter.Id != taskUpdateParams.UserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task has not been modified cause user not relate to thats reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (!string.IsNullOrWhiteSpace(taskUpdateParams.TaskCaption))
            {
                existedTask.Caption = taskUpdateParams.TaskCaption;
            }

            if (!string.IsNullOrWhiteSpace(taskUpdateParams.TaskDescription))
            {
                existedTask.Description = taskUpdateParams.TaskDescription;
            }

            if (taskUpdateParams.TaskType != TaskType.None)
            {
                existedTask.TaskType = taskUpdateParams.TaskType;
            }          

            if (taskUpdateParams.TaskStatus != TaskCurrentStatus.None)
            {
                existedTask.TaskStatus = taskUpdateParams.TaskStatus;
            }

            if (taskUpdateParams.ImplementerId != -1)
            {
                implementer = await _usersRepository.GetUserByIdAsync(taskUpdateParams.ImplementerId, token);

                if (implementer == null)
                {
                    var response1 = new Response();
                    response1.Result = false;
                    response1.OutInfo = $"Task has not been modified cause new implementer was not found";

                    return BadRequest(JsonConvert.SerializeObject(response1));
                }

                existedTask.Implementer = implementer;
            }

            await _tasksRepository.UpdateAsync(existedTask, token);

            _tasksRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"New info was added to task with id {taskId}";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such task with id {taskId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("delete_task")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteTaskByReporter(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var taskToDelete = JsonConvert.DeserializeObject<TaskIdDTO>(body);

        Debug.Assert(taskToDelete != null);

        var taskId = taskToDelete.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(taskId, token);

        var reporterId = taskToDelete.UserId;

        if (existedTask != null)
        {
            var reporter = await _usersRepository.GetUserByIdAsync(reporterId, token);

            if (reporter != null)
            {
                existedTask.Reporter = reporter;
            }

            if (taskToDelete.UserId != existedTask.Reporter.Id)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task has not been deleted cause user is not its reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _tasksRepository.DeleteAsync(taskId, token);

            _tasksRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Task with id {taskId} was deleted by reporter";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such task with id {taskId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_task_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetTaskInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var taskWithIdRequest = JsonConvert.DeserializeObject<TaskIdDTO>(body);

        Debug.Assert(taskWithIdRequest != null);

        var taskId = taskWithIdRequest.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(taskId, token);

        if (existedTask != null)
        {
            var reporterId = existedTask.ReporterId;
            var implementerId = existedTask.ImplementerId;

            var reporter = await _usersRepository.GetUserByIdAsync(reporterId, token);
            var implementer = await _usersRepository.GetUserByIdAsync(implementerId, token);

            if (reporter != null && implementer != null)
            {
                existedTask.Reporter = reporter;
                existedTask.Implementer = implementer;
            }

            if (taskWithIdRequest.UserId != existedTask.Reporter.Id 
                && taskWithIdRequest.UserId != existedTask.Implementer.Id)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Cant take info about task {taskId} cause user" +
                    $" is not its reporter or implementer";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (reporter == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task info has not been received" +
                    $" cause reporter of it was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (implementer == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task info has not been received" +
                    $" cause implementer of it was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var reporterInfo = new ShortUserInfo
            {
                UserEmail = reporter.Email,
                UserName = reporter.UserName,
                UserPhone = reporter.PhoneNumber
            };

            var implementerInfo = new ShortUserInfo
            {
                UserEmail = implementer.Email,
                UserName = implementer.UserName,
                UserPhone = reporter.PhoneNumber
            };

            var taskInfo = new TaskInfoResponse
            {
                TaskId = taskId,
                TaskCaption = existedTask.Caption,
                TaskDescription = existedTask.Description,
                TaskType = existedTask.TaskType,
                TaskStatus = existedTask.TaskStatus,
                Reporter = reporterInfo,
                Implementer = implementerInfo
            };

            var getReponse = new GetResponse();
            getReponse.Result = true;
            getReponse.OutInfo = $"Info about task with id {taskId} was received";
            getReponse.RequestedInfo = taskInfo;

            var json = JsonConvert.SerializeObject(getReponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such task with id {taskId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private readonly ITasksRepository _tasksRepository;
    private readonly IUsersRepository _usersRepository;
}

