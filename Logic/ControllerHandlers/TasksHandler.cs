using Contracts;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Microsoft.Extensions.Logging;
using Models.BusinessModels;
using Models.Enums;
using Models.RedisEventModels.TaskEvents;
using Models.RedisEventModels.UserEvents;
using Models.StorageModels;
using Models.UserActionModels;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace Logic.ControllerHandlers;

public sealed class TasksHandler : DataHandlerBase, ITasksHandler
{
    public TasksHandler(
        ICommonUsersUnitOfWork commonUnitOfWork,
        IRedisRepository redisRepository, 
        ILogger<TasksHandler> logger)
        : base(commonUnitOfWork, redisRepository, logger)
    {
    }

    public async Task<Response> TryCreateTask(
        string createData, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(createData);

        token.ThrowIfCancellationRequested();

        var taskToCreate = 
            JsonConvert.DeserializeObject<TaskInputDTO>(createData);

        Debug.Assert(taskToCreate != null);

        var reporterId = taskToCreate.UserId;
        var implementerId = taskToCreate.ImplementerId;

        var reporter = await CommonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(reporterId, token);

        var implementer = await CommonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(implementerId, token);

        if (reporter == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Task has not been created cause" +
                $" current user with id {reporterId} was not found";

            return await Task.FromResult(response1);
        }

        if (implementer == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Task has not been created cause" +
                $" user with id {implementerId} was not found";

            return await Task.FromResult(response1);
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

        await CommonUnitOfWork.TasksRepository.AddAsync(task, token);

        CommonUnitOfWork.SaveChanges();

        var taskId = task.Id;

        await SendEventForCacheAsync(
            new TaskCreatedEvent(
                Id: Guid.NewGuid().ToString(),
                IsCommited: false,
                UserId: reporterId,
                TaskId: taskId,
                CreatedMoment: DateTimeOffset.UtcNow));

        await SendEventForCacheAsync(
            new TaskAssignedEvent(
                Id: Guid.NewGuid().ToString(),
                IsCommited: false,
                UserId: implementerId,
                TaskId: taskId));

        var response = new ResponseWithId()
        {
            Id = taskId,
        };

        response.Result = true;
        response.OutInfo =
            $"New task with id = {taskId}" +
            $" for user '{implementer.UserName}'" +
            $" implementation was created";

        return await Task.FromResult(response);
    }

    public async Task<Response> TryUpdateTask(
        string updateData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(updateData);

        token.ThrowIfCancellationRequested();

        var taskUpdateParams = 
            JsonConvert.DeserializeObject<TaskInputWithIdDTO>(updateData);

        Debug.Assert(taskUpdateParams != null);

        var taskId = taskUpdateParams.TaskId;

        var existedTask = await CommonUnitOfWork
            .TasksRepository
            .GetTaskByIdAsync(
            taskUpdateParams.TaskId, token);

        var reporterId = taskUpdateParams.UserId;
        var implementerId = taskUpdateParams.ImplementerId;

        if (existedTask != null)
        {
            var reporter = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(reporterId, token);

            var implementer = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(implementerId, token);

            if (reporter != null && implementer != null)
            {
                existedTask.Reporter = reporter;
                existedTask.Implementer = implementer;
            }

            if (existedTask.Reporter.Id != taskUpdateParams.UserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Task has not been modified cause" +
                    $" current user with id {taskUpdateParams.UserId}" +
                    $" not relate to thats reporter";

                return await Task.FromResult(response1);
            }

            var response = new Response();

            var numbers_of_new_params = 0;

            if (!string.IsNullOrWhiteSpace(taskUpdateParams.TaskCaption))
            {
                if (existedTask.Caption != taskUpdateParams.TaskCaption)
                {
                    existedTask.Caption = taskUpdateParams.TaskCaption;
                    numbers_of_new_params++;
                }
            }

            if (!string.IsNullOrWhiteSpace(taskUpdateParams.TaskDescription))
            {
                if (existedTask.Description != taskUpdateParams.TaskDescription)
                {
                    existedTask.Description = taskUpdateParams.TaskDescription;
                    numbers_of_new_params++;
                }
            }

            if (taskUpdateParams.TaskType != TaskType.None)
            {
                if (existedTask.TaskType != taskUpdateParams.TaskType)
                {
                    existedTask.TaskType = taskUpdateParams.TaskType;
                    numbers_of_new_params++;
                }
            }

            var isTaskStatusChanged = false;

            if (taskUpdateParams.TaskStatus != TaskCurrentStatus.None)
            {
                if (existedTask.TaskStatus != taskUpdateParams.TaskStatus)
                {
                    existedTask.TaskStatus = taskUpdateParams.TaskStatus;
                    numbers_of_new_params++;

                    isTaskStatusChanged = true;
                }
            }

            var currentImplementerId = taskUpdateParams.ImplementerId;

            var implementerChanged = (false, existedTask.ImplementerId, currentImplementerId);

            if (taskUpdateParams.ImplementerId != -1)
            {
                implementer = await CommonUnitOfWork
                    .UsersRepository
                    .GetUserByIdAsync(currentImplementerId, token);

                if (implementer == null)
                {
                    var response1 = new Response();
                    response1.Result = false;
                    response1.OutInfo =
                        $"Task has not been modified cause" +
                        $" new implementer with id {currentImplementerId} was not found";

                    return await Task.FromResult(response1);
                }

                if (existedTask.ImplementerId != currentImplementerId)
                {
                    implementerChanged.Item1 = true;

                    existedTask.Implementer = implementer;
                    numbers_of_new_params++;
                }
            }

            await CommonUnitOfWork
                .TasksRepository
                .UpdateAsync(existedTask, token);

            CommonUnitOfWork.SaveChanges();

            if (numbers_of_new_params > 0)
            {
                response.OutInfo = $"Task with id {taskId} has been modified";

                var dateTime = DateTimeOffset.UtcNow;

                var taskInfo = new TaskInfoResponse
                {
                    TaskId = taskId,
                    TaskCaption = existedTask.Caption,
                    TaskDescription = existedTask.Description,
                    TaskType = existedTask.TaskType,
                    TaskStatus = existedTask.TaskStatus,
                    Reporter = reporter is not null
                        ? new ShortUserInfo
                        {
                            UserId = reporterId,
                            UserName = reporter.UserName,
                            UserEmail = reporter.Email,
                            UserPhone = reporter.PhoneNumber,
                            Role = reporter.Role,
                        }
                        : null!,
                    Implementer = implementer is not null
                        ? new ShortUserInfo
                        {
                            UserId = implementerId,
                            UserName = implementer.UserName,
                            UserEmail = implementer.Email,
                            UserPhone = implementer.PhoneNumber,
                            Role = implementer.Role
                        }
                        : null!
                };

                var json = JsonConvert.SerializeObject(taskInfo);

                await SendEventForCacheAsync(
                    new TaskParamsChangedEvent(
                        Id: Guid.NewGuid().ToString(),
                        IsCommited: false,
                        UserId: reporterId,
                        TaskId: taskId,
                        UpdateMoment: dateTime,
                        Json: json));

                await SendEventForCacheAsync(
                    new TaskParamsChangedEvent(
                        Id: Guid.NewGuid().ToString(),
                        IsCommited: false,
                        UserId: implementerId,
                        TaskId: taskId,
                        UpdateMoment: dateTime,
                        Json: json));

                if (existedTask.TaskStatus == TaskCurrentStatus.Review 
                    || existedTask.TaskStatus == TaskCurrentStatus.Done)
                {
                    if (isTaskStatusChanged)
                    {
                        await SendEventForCacheAsync(
                            new TaskTerminalStatusReceivedEvent(
                                Id: Guid.NewGuid().ToString(),
                                IsCommited: false,
                                UserId: implementerId,
                                TaskId: taskId,
                                TerminalMoment: dateTime,
                                TerminalStatus: existedTask.TaskStatus));
                    }
                }

                if (implementerChanged.Item1)
                {
                    await SendEventForCacheAsync(
                        new TaskUnassignedEvent(
                            Id: Guid.NewGuid().ToString(),
                            IsCommited: false,
                            UserId: implementerChanged.ImplementerId,
                            TaskId: taskId));

                    await SendEventForCacheAsync(
                        new TaskAssignedEvent(
                            Id: Guid.NewGuid().ToString(),
                            IsCommited: false,
                            UserId: implementerChanged.currentImplementerId,
                            TaskId: taskId));
                }
            }
            else
            {
                response.OutInfo =
                    $"Task with id {taskId} has all same parameters" +
                    $" so it has not been modified";
            }

            response.Result = true;

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such task with id {taskId}";

        return await Task.FromResult(response2);
    }

    public async Task<Response> TryDeleteTask(
        string deleteData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(deleteData);

        token.ThrowIfCancellationRequested();

        var taskToDelete = 
            JsonConvert.DeserializeObject<TaskIdDTO>(deleteData);

        Debug.Assert(taskToDelete != null);

        var taskId = taskToDelete.TaskId;

        var existedTask = await CommonUnitOfWork
            .TasksRepository
            .GetTaskByIdAsync(taskId, token);

        var reporterId = taskToDelete.UserId;

        if (existedTask != null)
        {
            var reporter = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(reporterId, token);

            if (reporter != null)
            {
                existedTask.Reporter = reporter;
            }
            else
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Task has not been deleted cause" +
                    $" current user with id {reporterId} was not found";

                return await Task.FromResult(response1);
            }

            if (reporterId != existedTask.Reporter.Id)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Task has not been deleted cause" +
                    $" current user with id {reporterId} is not its reporter";

                return await Task.FromResult(response1);
            }

            await CommonUnitOfWork.TasksRepository.DeleteAsync(taskId, token);

            CommonUnitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Task with id {taskId} was deleted by reporter";

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such task with id {taskId}";

        return await Task.FromResult(response2);
    }

    public async Task<GetResponse> GetTaskInfo(
        string taskInfoById,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(taskInfoById);

        token.ThrowIfCancellationRequested();

        var taskWithIdRequest = 
            JsonConvert.DeserializeObject<TaskIdDTO>(taskInfoById);

        Debug.Assert(taskWithIdRequest != null);

        var taskId = taskWithIdRequest.TaskId;

        var existedTask = await CommonUnitOfWork
            .TasksRepository
            .GetTaskByIdAsync(taskId, token);

        if (existedTask != null)
        {
            var reporterId = existedTask.ReporterId;
            var implementerId = existedTask.ImplementerId;

            var reporter = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(reporterId, token);

            var implementer = await CommonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(implementerId, token);

            if (reporter != null && implementer != null)
            {
                existedTask.Reporter = reporter;
                existedTask.Implementer = implementer;
            }
            else if (reporter == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Task info has not been received cause" +
                    $" current user with id {reporterId} was not found";

                return await Task.FromResult(response1);
            }

            if (taskWithIdRequest.UserId != existedTask.Reporter.Id
                && taskWithIdRequest.UserId != existedTask.Implementer.Id)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about task {taskId} cause user" +
                    $" is not its reporter or implementer";

                return await Task.FromResult(response1);
            }

            if (reporter == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Task info has not been received" +
                    $" cause reporter of it was not found";

                return await Task.FromResult(response1);
            }

            if (implementer == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Task info has not been received" +
                    $" cause implementer of it was not found";

                return await Task.FromResult(response1);
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
            getReponse.RequestedInfo = JsonConvert.SerializeObject(taskInfo);

            return await Task.FromResult(getReponse);
        }

        var response2 = new GetResponse();
        response2.Result = false;
        response2.OutInfo = $"No such task with id {taskId}";

        return await Task.FromResult(response2);
    }
}
