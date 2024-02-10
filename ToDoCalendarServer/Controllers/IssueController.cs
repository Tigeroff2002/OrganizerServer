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
[Route("issues")]
public sealed class IssueController : ControllerBase
{
    public IssueController(
        IIssuesRepository issuesRepository,
        IUsersRepository usersRepository)
    {
        _issuesRepository = issuesRepository
           ?? throw new ArgumentNullException(nameof(issuesRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [HttpPost]
    [Route("create_new")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> CreateIssue(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var issueToCreate = JsonConvert.DeserializeObject<IssueInputDTO>(body);

        Debug.Assert(issueToCreate != null);

        var userId = issueToCreate.UserId;
        var issueType = issueToCreate.IssueType;

        var user = await _usersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo = 
                $"Issue has not been created cause" +
                $" current user with id {userId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var issueMoment = DateTimeOffset.UtcNow;

        var issue = new Issue
        {
            Title = issueToCreate.Title,
            IssueType = issueType,
            Description = issueToCreate.Description,
            IssueMoment = issueMoment,
            ImgLink = issueToCreate.ImgLink,
            UserId = user.Id
        };

        await _issuesRepository.AddAsync(issue, token);

        _issuesRepository.SaveChanges();

        var issueId = issue.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New snapshot with id = {issueId}" +
            $" by user '{user.UserName}'" +
            $" with type '{issueType}' has been created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_issue_params")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateIssueParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var issueUpdateParams = JsonConvert.DeserializeObject<IssueInputWithIdDTO>(body);

        Debug.Assert(issueUpdateParams != null);

        var issueId = issueUpdateParams.IssueId;

        var existedIssue = await _issuesRepository.GetIssueByIdAsync(issueId, token);

        var currentUserId = issueUpdateParams.UserId;

        if (existedIssue != null)
        {
            var currentUser = await _usersRepository.GetUserByIdAsync(currentUserId, token);

            if (currentUser == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Issue has not been modified cause user" +
                    $" with id {currentUserId} is not found in db";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (existedIssue.UserId != currentUserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Issue has not been modified cause user" +
                    $" with id {currentUserId} not relate to thats creator";

                return BadRequest(JsonConvert.SerializeObject(response1));
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

            if (!string.IsNullOrWhiteSpace(issueUpdateParams.ImgLink))
            {
                existedIssue.ImgLink = issueUpdateParams.ImgLink;
                numbers_of_new_params++;
            }

            if (numbers_of_new_params > 0)
            {
                await _issuesRepository.UpdateAsync(existedIssue, token);

                _issuesRepository.SaveChanges();

                response.OutInfo = $"Issue with id {issueId} has been modified";
            }
            else
            {
                response.OutInfo =
                    $"Issue with id {issueId} has all same parameters" +
                    $" so it has not been modified";
            }

            response.Result = true;

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("delete_issue")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteIssue(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var issueToDelete = JsonConvert.DeserializeObject<IssueIdDTO>(body);

        Debug.Assert(issueToDelete != null);

        var issueId = issueToDelete.IssueId;

        var existedIssue = await _issuesRepository.GetIssueByIdAsync(issueId, token);

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

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _issuesRepository.DeleteAsync(issueId, token);

            _issuesRepository.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Issue with id {issueId} was deleted by creator";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_issue_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetIssueInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var issueWithIdRequest = JsonConvert.DeserializeObject<IssueIdDTO>(body);

        Debug.Assert(issueWithIdRequest != null);

        var issueId = issueWithIdRequest.IssueId;

        var existedIssue = await _issuesRepository.GetIssueByIdAsync(issueId, token);

        if (existedIssue != null)
        {
            var userId = existedIssue!.UserId;

            var manager = await _usersRepository.GetUserByIdAsync(userId, token);

            if (manager == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo = 
                    $"Cant take info about issue cause " +
                    $"user with id {userId} is not found in db";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (issueWithIdRequest.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about issue cause user with id" +
                    $" {issueWithIdRequest.UserId} is not its manager";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var issueInfo = new IssueInfoResponse
            {
                Title = existedIssue.Title,
                IssueType = existedIssue.IssueType,
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

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such issue with id {issueId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private readonly IIssuesRepository _issuesRepository;
    private readonly IUsersRepository _usersRepository;
}
