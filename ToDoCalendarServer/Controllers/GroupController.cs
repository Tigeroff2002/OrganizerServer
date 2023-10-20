using Contracts.Request;
using Contracts.Request.RequestById;
using Logic.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.BusinessModels;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("groups")]
public sealed class GroupController : ControllerBase
{
    public GroupController(
        IGroupsRepository groupsRepository,
        IUsersRepository usersRepository) 
    {
        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));

        _usersRepository = usersRepository
            ?? throw new ArgumentNullException(nameof(usersRepository));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateGroup(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupToCreate = JsonConvert.DeserializeObject<GroupInputDTO>(body);

        Debug.Assert(groupToCreate != null);

        var listUserGroupMaps = new List<UserGroupMap>();

        if (groupToCreate.Participants != null)
        {
            var existedUsers = await _usersRepository.GetAllUsersAsync(token);

            foreach (var userId in groupToCreate.Participants)
            {
                var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                if (currentUser != null)
                {
                    var userGroupMap = new UserGroupMap();
                    userGroupMap.UserId = userId;
                    userGroupMap.User = currentUser;
                    userGrou
                    listUserGroupMaps.Add(currentUser);
                }
            }
        }

        var selfUser = await _usersRepository.GetUserByIdAsync(groupToCreate.UserId, token);

        if (selfUser == null)
        {
            var response1 = new Response();
            response1.Result = true;
            response1.OutInfo = $"Group has not been created cause current user was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        listUsers.Add(selfUser);

        var group = new Group()
        {
            GroupName = groupToCreate.GroupName,
            Type = groupToCreate.Type,
            Participants = listUsers
        };

        await _groupsRepository.AddAsync(group, token);

        var response = new Response();
        response.Result = true;
        response.OutInfo = $"New group with name {group.GroupName} was created";

        var json = JsonConvert.SerializeObject(response);

        return Ok(response);
    }

    [HttpPut]
    [Route("add_participants")]
    public async Task<IActionResult> AddParticipants(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupAddParticipants = JsonConvert.DeserializeObject<GroupAddParticipants>(body);

        Debug.Assert(groupAddParticipants != null);

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(
            groupAddParticipants.GroupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.Participants
                    .FirstOrDefault(x => x.Id == groupAddParticipants.UserId) == null)
            {
                var response1 = new Response();
                response1.Result = true;
                response1.OutInfo = $"Group has not been modified cause user not relate to that";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var listUsers = new List<User>();

            var existedUsers = await _usersRepository.GetAllUsersAsync(token);

            foreach (var userId in groupAddParticipants.Participants)
            {
                var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

                if (currentUser != null)
                {
                    listUsers.Add(currentUser);
                }
            }

            existedGroup.Participants.AddRange(listUsers);

            await _groupsRepository.UpdateAsync(existedGroup, token);

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"New participants to group with id {groupAddParticipants.GroupId} were added";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such group with id {groupAddParticipants.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpDelete]
    [Route("delete_participant")]
    public async Task<IActionResult> DeleteParticipant(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupDeleteParticipant = JsonConvert.DeserializeObject<GroupDeleteParticipant>(body);

        Debug.Assert(groupDeleteParticipant != null);

        if (groupDeleteParticipant.UserId != groupDeleteParticipant.Participant_Id)
        {
            var response1 = new Response();
            response1.Result = true;
            response1.OutInfo = $"Group has not been modified cause user not deleting yourself";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(
            groupDeleteParticipant.GroupId, token);

        var existedUser = await _usersRepository.GetUserByIdAsync(groupDeleteParticipant.Participant_Id, token);

        if (existedGroup != null && existedUser != null)
        {
            existedGroup.Participants = 
                existedGroup.Participants.Where(x => x.Id != groupDeleteParticipant.Participant_Id).ToList();

            await _groupsRepository.UpdateAsync(existedGroup, token);

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Participant with id {groupDeleteParticipant.Participant_Id}" +
                $" has been deleted from group with id {groupDeleteParticipant.GroupId}";

            var json = JsonConvert.SerializeObject(response);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such group with id {groupDeleteParticipant.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [HttpGet]
    [Route("get_group_info")]
    public async Task<IActionResult> GetGroupInfo(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupByIdRequest = JsonConvert.DeserializeObject<GroupDetailsRequest>(body);

        Debug.Assert(groupByIdRequest != null);

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(groupByIdRequest.GroupId, token);

        if (existedGroup != null)
        {
            if (existedGroup.Participants
                    .FirstOrDefault(x => x.Id == groupByIdRequest.UserId) == null)
            {
                var response1 = new Response();
                response1.Result = true;
                response1.OutInfo = $"Group has not been received cause user not relate to that";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var listOfUsers = new List<int>();

            foreach(var user in existedGroup.Participants)
            {
                listOfUsers.Add(user.Id);
            }

            var groupInfo = new GroupInputDTO
            {
                GroupName = existedGroup.GroupName,
                Type = existedGroup.Type,
                Participants = listOfUsers
            };

            var json = JsonConvert.SerializeObject(groupInfo);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such group with id {groupByIdRequest.GroupId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }

    private readonly IGroupsRepository _groupsRepository;
    private readonly IUsersRepository _usersRepository;
}
