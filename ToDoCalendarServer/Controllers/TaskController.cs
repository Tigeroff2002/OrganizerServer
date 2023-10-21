using Contracts;
using Contracts.Request;
using Contracts.Response;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
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
    public async Task<IActionResult> CreateTask(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

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
            Reporter = reporter,
            Implementer = implementer
        };

        var response = new Response();
        response.Result = true;
        response.OutInfo = 
            $"New task with id = {task.Id}" +
            $" for user '{implementer.UserName}' implementation was created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(response);
    }

    [HttpPut]
    [Route("update_task_params")]
    public async Task<IActionResult> UpdateTaskParams(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var taskUpdateParams = JsonConvert.DeserializeObject<TaskInputWithIdDTO>(body);

        Debug.Assert(taskUpdateParams != null);

        var taskId = taskUpdateParams.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(
            taskUpdateParams.TaskId, token);

        if (existedTask != null)
        {
            if (existedTask.Reporter.Id != taskUpdateParams.UserId)
            {
                var response1 = new Response();
                response1.Result = true;
                response1.OutInfo = $"Task has not been modified cause user not relate to thats reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            existedTask.Caption = taskUpdateParams.TaskCaption;
            existedTask.Description = taskUpdateParams.TaskDescription;
            existedTask.TaskType = taskUpdateParams.TaskType;
            existedTask.TaskStatus = taskUpdateParams.TaskStatus;

            var implementer = await _usersRepository.GetUserByIdAsync(taskUpdateParams.ImplementerId, token);

            if (implementer == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task has not been modified cause new implementer was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            existedTask.Implementer = implementer;

            await _tasksRepository.UpdateAsync(existedTask, token);

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

    [HttpDelete]
    [Route("delete_task")]
    public async Task<IActionResult> DeleteParticipant(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var taskToDelete = JsonConvert.DeserializeObject<TaskIdDTO>(body);

        Debug.Assert(taskToDelete != null);

        var taskId = taskToDelete.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(taskId, token);

        if (existedTask != null)
        {
            if (taskToDelete.UserId != existedTask.Reporter.Id)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task has not been modified cause user is not its reporter";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Task with id {taskId} was deleted by reporter";
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such task with id {taskId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpGet]
    [Route("get_task_info")]
    public async Task<IActionResult> GetGroupInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var taskWithIdRequest = JsonConvert.DeserializeObject<TaskIdDTO>(body);

        Debug.Assert(taskWithIdRequest != null);

        var taskId = taskWithIdRequest.TaskId;

        var existedTask = await _tasksRepository.GetTaskByIdAsync(taskId, token);

        if (existedTask != null)
        {
            if (taskWithIdRequest.UserId != existedTask.Reporter.Id 
                && taskWithIdRequest.UserId != existedTask.Implementer.Id)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Cant take info about task cause user is not its reporter or implementer";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var reporter = await _usersRepository.GetUserByIdAsync(existedTask.Reporter.Id, token);
            var implementer = await _usersRepository.GetUserByIdAsync(existedTask.Implementer.Id, token);

            if (reporter == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task info has not been received cause reporter of it was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (implementer == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = $"Task info has not been received cause implementer of it was not found";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var reporterInfo = new ShortUserInfo
            {
                UserEmail = reporter.Email,
                UserName = reporter.UserName
            };

            var implementerInfo = new ShortUserInfo
            {
                UserEmail = implementer.Email,
                UserName = implementer.UserName
            };

            var taskInfo = new TaskInfoResponse
            {
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

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly ITasksRepository _tasksRepository;
    private readonly IUsersRepository _usersRepository;
}

