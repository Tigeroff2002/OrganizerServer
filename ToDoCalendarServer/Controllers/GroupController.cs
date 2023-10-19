using Contracts.Request;
using Contracts.Request.RequestById;
using Logic.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using PostgreSQL.Abstractions;
using System.Diagnostics;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("group")]
public sealed class GroupController : ControllerBase
{
    public GroupController(IGroupsRepository groupsRepository, IUsersRepository usersRepository) 
    {
        _groupsRepository = groupsRepository
            ?? throw new ArgumentNullException(nameof(groupsRepository));
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateGroup(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupToCreate = JsonConvert.DeserializeObject<GroupInputDTO>(body);

        Debug.Assert(groupToCreate != null);

        var listUsers = new List<User>();

        var existedUsers = await _usersRepository.GetAllUsersAsync(token);

        foreach (var userId in groupToCreate.Participants)
        { 
            var currentUser = existedUsers.FirstOrDefault(x => x.Id == userId);

            if (currentUser != null)
            {
                listUsers.Add(currentUser);
            }
        }

        var group = new Group()
        {
            GroupName = groupToCreate.GroupName,
            Type = groupToCreate.Type,
            Participants = listUsers
        };

        await _groupsRepository.AddAsync(group, token);

        return Ok();
    }

    [HttpPut]
    [Route("add_participants")]
    public async Task<IActionResult> AddParticipants(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupAddParticipants = JsonConvert.DeserializeObject<GroupAddParticipants>(body);

        Debug.Assert(groupAddParticipants != null);

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

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(
            groupAddParticipants.GroupId, token);

        if (existedGroup!= null)
        {
            existedGroup.Participants.AddRange(listUsers);

            await _groupsRepository.UpdateAsync(existedGroup, token);

            return Ok();
        }

        return BadRequest("No group with such id");
    }

    [HttpDelete]
    [Route("delete_participant")]
    public async Task<IActionResult> DeleteParticipant(CancellationToken token)
    {
        var body = await ReadRequestBodyAsync();

        var groupDeleteParticipant = JsonConvert.DeserializeObject<GroupDeleteParticipant>(body);

        Debug.Assert(groupDeleteParticipant != null);

        var existedGroup = await _groupsRepository.GetGroupByIdAsync(
            groupDeleteParticipant.GroupId, token);

        var existedUser = await _usersRepository.GetUserByIdAsync(groupDeleteParticipant.Participant_Id, token);

        if (existedGroup != null && existedUser != null)
        {
            existedGroup.Participants = 
                existedGroup.Participants.Where(x => x.Id != groupDeleteParticipant.Participant_Id).ToList();

            await _groupsRepository.UpdateAsync(existedGroup, token);

            return Ok();
        }

        return BadRequest("No group or user with such id");
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
            var listOfUsers = new List<int>();

            foreach(var user in existedGroup.Participants)
            {
                listOfUsers.Add(user.Id);
            }

            var groupInfo = new GroupInputDTO()
            {
                GroupName = existedGroup.GroupName,
                Type = existedGroup.Type,
                Participants = listOfUsers
            };

            var json = JsonConvert.SerializeObject(groupInfo);

            return Ok(json);
        }

        return BadRequest("No group with such id");
    }

    private async Task<string> ReadRequestBodyAsync()
    {
        using var reader = new StreamReader(Request.Body);

        return await reader.ReadToEndAsync();
    }


    private readonly IGroupsRepository _groupsRepository;
    private readonly IUsersRepository _usersRepository;
}
