using Contracts.Request;
using Contracts.Request.RequestById;
using Contracts.Response;
using Logic.Abstractions;
using Models.BusinessModels;
using Models.Enums;
using Models.StorageModels;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace Logic.ControllerHandlers;

public sealed class IssuesHandler : IIssuesHandler
{
    public IssuesHandler(ICommonUsersUnitOfWork commonUnitOfWork)
    {
        _commonUnitOfWork = commonUnitOfWork
            ?? throw new ArgumentNullException(nameof(commonUnitOfWork));
    }

    public async Task<Response> TryCreateIssue(
        string createData, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(createData);

        token.ThrowIfCancellationRequested();

        var issueToCreate = 
            JsonConvert.DeserializeObject<IssueInputDTO>(createData);
        Debug.Assert(issueToCreate != null);

        var userId = issueToCreate.UserId;
        var issueType = issueToCreate.IssueType;

        var user = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Issue has not been created cause" +
                $" current user with id {userId} was not found";

            return await Task.FromResult(response1);
        }

        var issueMoment = DateTimeOffset.UtcNow;

        var issue = new Issue
        {
            Title = issueToCreate.Title,
            Status = IssueStatus.Reported,
            IssueType = issueType,
            Description = issueToCreate.Description,
            IssueMoment = issueMoment,
            ImgLink = issueToCreate.ImgLink,
            UserId = user.Id
        };

        await _commonUnitOfWork.IssuesRepository.AddAsync(issue, token);

        _commonUnitOfWork.SaveChanges();

        var issueId = issue.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New snapshot with id = {issueId}" +
            $" by user '{user.UserName}'" +
            $" with type '{issueType}' has been created";

        return await Task.FromResult(response);
    }

    public async Task<Response> TryUpdateIssue(
        string updateData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(updateData);

        token.ThrowIfCancellationRequested();

        var issueUpdateParams = 
            JsonConvert.DeserializeObject<IssueInputWithIdDTO>(updateData);

        Debug.Assert(issueUpdateParams != null);

        var issueId = issueUpdateParams.IssueId;

        var existedIssue = await _commonUnitOfWork
            .IssuesRepository
            .GetIssueByIdAsync(issueId, token);

        var currentUserId = issueUpdateParams.UserId;

        if (existedIssue != null)
        {
            var currentUser = await _commonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(currentUserId, token);

            if (currentUser == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Issue has not been modified cause user" +
                    $" with id {currentUserId} is not found in db";

                return await Task.FromResult(response1);
            }

            if (existedIssue.UserId != currentUserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Issue has not been modified cause user" +
                    $" with id {currentUserId} not relate to thats creator";

                return await Task.FromResult(response1);
            }

            var response = new Response();

            var numbers_of_new_params = 0;

            if (!string.IsNullOrWhiteSpace(issueUpdateParams.Title))
            {
                existedIssue.Title = issueUpdateParams.Title;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(issueUpdateParams.Description))
            {
                existedIssue.Description = issueUpdateParams.Description;
                numbers_of_new_params++;
            }

            if (issueUpdateParams.IssueType != IssueType.None)
            {
                existedIssue.IssueType = issueUpdateParams.IssueType;
                numbers_of_new_params++;
            }

            if (issueUpdateParams.IssueStatus != IssueStatus.None)
            {
                existedIssue.Status = issueUpdateParams.IssueStatus;
                numbers_of_new_params++;
            }

            if (!string.IsNullOrWhiteSpace(issueUpdateParams.ImgLink))
            {
                existedIssue.ImgLink = issueUpdateParams.ImgLink;
                numbers_of_new_params++;
            }

            if (numbers_of_new_params > 0)
            {
                await _commonUnitOfWork
                    .IssuesRepository
                    .UpdateAsync(existedIssue, token);

                response.OutInfo = $"Issue with id {issueId} has been modified";
            }
            else
            {
                response.OutInfo =
                    $"Issue with id {issueId} has all same parameters" +
                    $" so it has not been modified";
            }

            _commonUnitOfWork.SaveChanges();

            response.Result = true;

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return await Task.FromResult(response2);
    }

    public async Task<Response> TryDeleteIssue(
        string deleteData,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(deleteData);

        token.ThrowIfCancellationRequested();

        var issueToDelete = 
            JsonConvert.DeserializeObject<IssueIdDTO>(deleteData);

        Debug.Assert(issueToDelete != null);

        var issueId = issueToDelete.IssueId;

        var existedIssue = await _commonUnitOfWork
            .IssuesRepository
            .GetIssueByIdAsync(issueId, token);

        if (existedIssue != null)
        {
            var userId = existedIssue!.UserId;

            if (issueToDelete.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Issue has not been deleted cause" +
                    $" current user with id {issueToDelete.UserId} is not its creator";

                return await Task.FromResult(response1);
            }

            await _commonUnitOfWork
                .IssuesRepository
                .DeleteAsync(issueId, token);

            _commonUnitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Issue with id {issueId} was deleted by creator";

            return await Task.FromResult(response);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return await Task.FromResult(response2);
    }

    public async Task<GetResponse> GetIssueInfo(
        string issueInfoById, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(issueInfoById);

        token.ThrowIfCancellationRequested();

        var issueWithIdRequest = 
            JsonConvert.DeserializeObject<IssueIdDTO>(issueInfoById);

        Debug.Assert(issueWithIdRequest != null);

        var issueId = issueWithIdRequest.IssueId;

        var existedIssue = await _commonUnitOfWork
            .IssuesRepository
            .GetIssueByIdAsync(issueId, token);

        if (existedIssue != null)
        {
            var userId = existedIssue!.UserId;

            var manager = await _commonUnitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (manager == null)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about issue cause " +
                    $"user with id {userId} is not found in db";

                return await Task.FromResult(response1);
            }

            if (issueWithIdRequest.UserId != userId)
            {
                var response1 = new GetResponse();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about issue cause user with id" +
                    $" {issueWithIdRequest.UserId} is not its manager";

                return await Task.FromResult(response1);
            }

            var issueInfo = new IssueInfoResponse
            {
                IssueId = existedIssue.Id,
                Title = existedIssue.Title,
                IssueType = existedIssue.IssueType,
                IssueStatus = existedIssue.Status,
                Description = existedIssue.Description,
                ImgLink = existedIssue.ImgLink,
                CreateMoment = existedIssue.IssueMoment
            };

            var getResponse = new GetResponse();

            getResponse.Result = true;
            getResponse.OutInfo =
                $"Info about issue with id {issueId}" +
                $" by user with id {userId} was received";
            getResponse.RequestedInfo = issueInfo;

            return await Task.FromResult(getResponse);
        }

        var response2 = new GetResponse();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return await Task.FromResult(response2);
    }

    public async Task<GetResponse> GetAllIssuesInfo(
        string allIssuesDTO, 
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrEmpty(allIssuesDTO);

        token.ThrowIfCancellationRequested();

        var requestDTO = 
            JsonConvert.DeserializeObject<RequestWithToken>(allIssuesDTO);

        Debug.Assert(requestDTO != null);

        var userId = requestDTO.UserId;

        var existedUser = await _commonUnitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about issues cause " +
                $"user with id {userId} is not found in db";

            return await Task.FromResult(response1);
        }

        if (existedUser.Role != UserRole.Admin)
        {
            var response1 = new GetResponse();
            response1.Result = false;
            response1.OutInfo =
                $"Cant take info about issues cause " +
                $"user with id {userId} is not system administrator";

            return await Task.FromResult(response1);
        }

        var allUsers =
            await _commonUnitOfWork
                .UsersRepository
                .GetAllUsersAsync(token);

        var allIssues =
            _commonUnitOfWork
                .IssuesRepository
                .GetAllIssuesAsync(token)
                .GetAwaiter().GetResult()
                .Where(x => x.Status != IssueStatus.Closed)
                .Select(x => new FullIssueInfoResponse
                {
                    IssueId = x.Id,
                    Title = x.Title,
                    IssueType = x.IssueType,
                    IssueStatus = x.Status,
                    Description = x.Description,
                    ImgLink = x.ImgLink,
                    CreateMoment = x.IssueMoment,
                    UserName =
                        allUsers
                            .FirstOrDefault(u => u.Id == x.UserId)!
                            .UserName
                })
                .ToList();

        var issuesResponseModel = new SystemIssuesResponse
        {
            Issues = allIssues
        };

        var getResponse = new GetResponse();

        getResponse.Result = true;
        getResponse.OutInfo =
            $"Info about all users not closed issues" +
            $" for admin with id {userId} was received";
        getResponse.RequestedInfo = issuesResponseModel;

        return await Task.FromResult(getResponse);
    }

    private readonly ICommonUsersUnitOfWork _commonUnitOfWork;
}
