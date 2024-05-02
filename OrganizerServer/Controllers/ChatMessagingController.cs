using Contracts.Request;
using Contracts.Response;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Models.BusinessModels;
using Models.Enums;
using Models;
using Newtonsoft.Json;
using PostgreSQL;
using PostgreSQL.Abstractions;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Contracts.Request.RequestById;
using Google.Apis.Util;
using Models.StorageModels;

namespace ToDoCalendarServer.Controllers;

[ApiController]
[Route("chatting")]
public sealed class ChatMessagingController : ControllerBase
{
    public ChatMessagingController(IUsersMessagingUnitOfWork unitOfWork) 
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    [HttpPost]
    [Route("add_first_chat_message")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> AddFirstMessage(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var messageToCreate = JsonConvert.DeserializeObject<ChatFirstMessageDTO>(body);

        Debug.Assert(messageToCreate != null);

        var userId = messageToCreate.UserId;
        var receiverId = messageToCreate.ReceiverId;

        Debug.Assert(userId != receiverId);

        var messageText = messageToCreate.Text;

        var user = await _unitOfWork.UsersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"New message has not been posted cause" +
                $" current user with id {userId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var receiver = await _unitOfWork.UsersRepository.GetUserByIdAsync(receiverId, token);

        if (receiver == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"New message has not been posted cause" +
                $" related receiver with id {receiverId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var writeMoment = DateTimeOffset.UtcNow;

        var existedChat = await _unitOfWork
            .ChatRepository
            .GetChatByBothUserIdsAsync(userId, receiverId, token);

        var message = new DirectMessage
        {
            SendTime = writeMoment,
            Text = messageText,
            isEdited = false,
            UserId = user.Id
        };

        int chatId;

        if (existedChat == null)
        {
            var newChat = new DirectChat
            {
                CreateTime = writeMoment,
                User1Id = userId,
                User2Id = receiverId,
                Caption = $"{user.UserName}/{receiver.UserName} chat"
            };

            await _unitOfWork.ChatRepository.AddAsync(newChat, token);

            _unitOfWork.SaveChanges();

            chatId = newChat.Id;
        }
        else
        {
            chatId = existedChat.Id;
        }

        message.ChatId = chatId;

        await _unitOfWork.MessageRepository.AddAsync(message, token);

        _unitOfWork.SaveChanges();

        var newMessageId = message.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New messahe with id = {newMessageId}" +
            $" by user '{user.UserName}'" +
            $" for user '{receiver.UserName}' has been posted";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [HttpPost]
    [Route("add_new_chat_message")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> AddNewChatMessage(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var messageToCreate = JsonConvert.DeserializeObject<NewMessageInputDTO>(body);

        Debug.Assert(messageToCreate != null);

        var userId = messageToCreate.UserId;
        var chatId = messageToCreate.ChatId;
        var messageText = messageToCreate.Text;

        var user = await _unitOfWork.UsersRepository.GetUserByIdAsync(userId, token);

        if (user == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"New message has not been posted cause" +
                $" current user with id {userId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var existedChat = await _unitOfWork.ChatRepository.GetChatByIdAsync(chatId, token);

        if (existedChat == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"New message has not been posted cause" +
                $" related chat with id {chatId} was not found";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        if (existedChat.User1Id != userId 
            && existedChat.User2Id != userId)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"New message has not been posted cause" +
                $" user with id {chatId} not relates" +
                $" to users from chat {chatId}";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var writeMoment = DateTimeOffset.UtcNow;

        var message = new DirectMessage
        {
            SendTime = writeMoment,
            Text = messageText,
            isEdited = false,
            UserId = user.Id,
            ChatId = chatId
        };

        await _unitOfWork.MessageRepository.AddAsync(message, token);

        _unitOfWork.SaveChanges();

        var newMessageId = message.Id;

        var response = new Response();
        response.Result = true;
        response.OutInfo =
            $"New messahe with id = {newMessageId}" +
            $" by user '{user.UserName}'" +
            $" for chat '{existedChat.Caption}' has been posted";

        var json = JsonConvert.SerializeObject(response);

        return Ok(json);
    }

    [Route("update_message")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> UpdateMessageParams(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var messageUpdateParams = JsonConvert.DeserializeObject<MessageInputWithIdDTO>(body);

        Debug.Assert(messageUpdateParams != null);

        var userId = messageUpdateParams.UserId;
        var messageId = messageUpdateParams.MessageId;

        var existedMessage = await _unitOfWork
            .MessageRepository
            .GetMessageByIdAsync(messageId, token);

        if (existedMessage != null)
        {
            var currentUser = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (currentUser == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Message has not been modified cause user" +
                    $" with id {userId} is not found in db";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (existedMessage.Id != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Message has not been modified cause user" +
                    $" with id {userId} not relate to thats creator";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var response = new Response();

            existedMessage.Text = messageUpdateParams.Text;
            existedMessage.isEdited = true;

            await _unitOfWork.MessageRepository.UpdateAsync(existedMessage, token);

            _unitOfWork.SaveChanges();

            response.OutInfo = $"Message with id {messageId} has been modified";

            response.Result = true;

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such message with id {messageId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("delete_message")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> DeleteIssue(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var messageToDelete = JsonConvert.DeserializeObject<MessageIdDTO>(body);

        Debug.Assert(messageToDelete != null);

        var messageId = messageToDelete.MessageId;

        var existedMessage = await _unitOfWork
            .MessageRepository
            .GetMessageByIdAsync(messageId, token);

        if (existedMessage != null)
        {
            var userId = existedMessage!.UserId;

            var currentUser = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(userId, token);

            if (currentUser == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Message has not been deleted cause user" +
                    $" with id {userId} is not found in db";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (messageToDelete.UserId != userId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Message has not been deleted cause" +
                    $" current user with id {messageToDelete.UserId} is not its creator";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            await _unitOfWork.MessageRepository.DeleteAsync(messageId, token);

            _unitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = $"Message with id {messageId} was deleted by creator";

            return Ok(JsonConvert.SerializeObject(response));
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such message with id {messageId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_message_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetMessageInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var messageWithIdRequest = JsonConvert.DeserializeObject<MessageIdDTO>(body);

        Debug.Assert(messageWithIdRequest != null);

        var messageId = messageWithIdRequest.MessageId;
        var currentUserId = messageWithIdRequest.UserId;

        var existedMessage = await _unitOfWork
            .MessageRepository
            .GetMessageByIdAsync(messageId, token);

        if (existedMessage != null)
        {
            var chatId = existedMessage.ChatId;

            var existedChat = await _unitOfWork
                .ChatRepository
                .GetChatByIdAsync(chatId, token);

            if (existedChat == null)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about message cause " +
                    $"related direct chat with id {chatId} was not found in db";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            if (existedChat.User1Id != currentUserId 
                && existedChat.User2Id != currentUserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about message with id" +
                    $" {messageId} cause current user with id {currentUserId}" +
                    $" not relates to users from direct chat with id {chatId}";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var writerId = existedMessage.UserId;

            var existedWriter = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(writerId, token);

            Debug.Assert(existedWriter != null);

            var receiverId = existedChat.User1Id == writerId
                ? existedChat.User2Id
                : existedChat.User1Id;

            var existedReceiver = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(receiverId, token);

            Debug.Assert(existedReceiver != null);

            var messageInfo = new MessageInfoWithChatResponse
            {
                MessageId = messageId,
                ChatId = chatId,
                ChatCaption = existedChat.Caption,
                WriterId = writerId,
                WriterName = existedWriter.UserName,
                ReceiverId = receiverId,
                ReceiverName = existedReceiver.UserName,
                IsEdited = existedMessage.isEdited,
                SendTime = existedMessage.SendTime,
                Text = existedMessage.Text,
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo =
                $"Info about message with id {messageId}" +
                $" by user with id {currentUserId} was received";
            getResponse.RequestedInfo = messageInfo;

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such message with id {messageId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_chat_info")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetChatInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var chatWithIdRequest = JsonConvert.DeserializeObject<ChatIdDTO>(body);

        Debug.Assert(chatWithIdRequest != null);

        var chatId = chatWithIdRequest.ChatId;
        var currentUserId = chatWithIdRequest.UserId;

        var existedChat = await _unitOfWork
            .ChatRepository
            .GetChatByIdAsync(chatId, token);

        if (existedChat != null)
        {
            if (existedChat.User1Id != currentUserId
                && existedChat.User2Id != currentUserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about chat with id" +
                    $" {chatId} cause current user with id {currentUserId}" +
                    $" not relates to users from direct chat with id {chatId}";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var homeUserId = existedChat.User1Id;
            var awayUserId = existedChat.User2Id;

            var existedHomeUser = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(homeUserId, token);

            Debug.Assert(existedHomeUser != null);

            var existedAwayUser = await _unitOfWork
                .UsersRepository
                .GetUserByIdAsync(awayUserId, token);

            Debug.Assert(existedAwayUser != null);

            var homeUserInfo = new ShortUserInfo
            {
                UserEmail = existedHomeUser.Email,
                UserName = existedHomeUser.UserName,
                UserPhone = existedHomeUser.PhoneNumber,
                Role = existedHomeUser.Role,
                UserId = homeUserId,
            };

            var awayUserInfo = new ShortUserInfo
            {
                UserEmail = existedAwayUser.Email,
                UserName = existedAwayUser.UserName,
                UserPhone = existedAwayUser.PhoneNumber,
                Role = existedAwayUser.Role,
                UserId = awayUserId,
            };

            var messagesFromChat = await _unitOfWork
                .MessageRepository
                .GetAllMessagesByChatIdAsync(chatId, token);

            var allMessagesList = messagesFromChat.Select(message =>
            {
                var messageId = message.Id;

                var existedMessage = _unitOfWork
                    .MessageRepository
                    .GetMessageByIdAsync(messageId, token);

                if (existedMessage != null)
                {
                    return new MessageInfoResponse
                    {
                        MessageId = messageId,
                        SendTime = message.SendTime,
                        Text = message.Text,
                        IsEdited = message.isEdited,
                        WriterId = message.UserId,
                        WriterName = message.UserId == homeUserId
                            ? homeUserInfo.UserName
                            : awayUserInfo.UserName
                    };
                }

                return null;
            });

            var messagesList = allMessagesList.Where(x => x != null).ToList();

            Debug.Assert(messagesList != null);

            var chatInfo = new ChatMessagesResponse
            {
                ChatId = chatId,
                Caption = existedChat.Caption,
                CreateTime = existedChat.CreateTime,
                UserHome = homeUserInfo,
                UserAway = awayUserInfo,
                Messages = messagesList!
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo =
                $"Info about chat with id {chatId}" +
                $" by user with id {currentUserId} was received";
            getResponse.RequestedInfo = chatInfo;

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = $"No such chat with id {chatId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_possible_direct_chat")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetPossibleDirectChatInfo(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var possibleChatWithIdRequest = 
            JsonConvert.DeserializeObject<UserDirectChatRequestDTO>(body);

        Debug.Assert(possibleChatWithIdRequest != null);

        var currentUserId = possibleChatWithIdRequest.UserId;
        var receiverId = possibleChatWithIdRequest.ReceiverId;

        Debug.Assert(currentUserId != receiverId);

        var existedCurrentUser = await _unitOfWork
            .UsersRepository
            .GetUserByIdAsync(currentUserId, token);

        if (existedCurrentUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Possible chat info was not received cause user" +
                $" with id {currentUserId} is not found in db";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var existedReceiver = await _unitOfWork
            .UsersRepository
            .GetUserByIdAsync(receiverId, token);

        if (existedReceiver == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Possible chat info was not received cause receiver" +
                $" with id {receiverId} is not found in db";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var existedChat = await _unitOfWork
            .ChatRepository
            .GetChatByBothUserIdsAsync(currentUserId, receiverId, token);

        if (existedChat != null)
        {
            var (existedHomeUser, existedAwayUser) =
                existedChat.User1Id == currentUserId
                    ? (existedCurrentUser, existedReceiver)
                    : (existedReceiver, existedCurrentUser);

            var homeUserId = existedHomeUser.Id;
            var awayUserId = existedAwayUser.Id;

            var homeUserInfo = new ShortUserInfo
            {
                UserEmail = existedHomeUser.Email,
                UserName = existedHomeUser.UserName,
                UserPhone = existedHomeUser.PhoneNumber,
                Role = existedHomeUser.Role,
                UserId = homeUserId,
            };

            var awayUserInfo = new ShortUserInfo
            {
                UserEmail = existedAwayUser.Email,
                UserName = existedAwayUser.UserName,
                UserPhone = existedAwayUser.PhoneNumber,
                Role = existedAwayUser.Role,
                UserId = awayUserId,
            };

            var messagesFromChat = await _unitOfWork
                .MessageRepository
                .GetAllMessagesByChatIdAsync(existedChat.Id, token);

            var allMessagesList = messagesFromChat.Select(message =>
            {
                var messageId = message.Id;

                return new MessageInfoResponse
                {
                    MessageId = messageId,
                    SendTime = message.SendTime,
                    Text = message.Text,
                    IsEdited = message.isEdited,
                    WriterId = message.UserId,
                    WriterName = message.UserId == homeUserId
                        ? homeUserInfo.UserName
                        : awayUserInfo.UserName
                };
            });

            var messagesList = allMessagesList.Where(x => x != null).ToList();

            Debug.Assert(messagesList != null);

            var chatInfo = new ChatMessagesResponse
            {
                ChatId = existedChat.Id,
                Caption = existedChat.Caption,
                CreateTime = existedChat.CreateTime,
                UserHome = homeUserInfo,
                UserAway = awayUserInfo,
                Messages = messagesList!
            };

            var getResponse = new GetResponse();
            getResponse.Result = true;
            getResponse.OutInfo =
                $"Info about existed chat with id {existedChat.Id}" +
                $" by user with id {currentUserId} was received";
            getResponse.RequestedInfo = chatInfo;

            var json = JsonConvert.SerializeObject(getResponse);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = false;
        response2.OutInfo = 
            $"No such chat with for user {currentUserId}" +
            $" and related receiver {receiverId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("rename_chat_caption")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> RenameChatCaption(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var chatWithIdRequest = JsonConvert.DeserializeObject<ChatInputWithIdDTO>(body);

        Debug.Assert(chatWithIdRequest != null);

        var chatId = chatWithIdRequest.ChatId;
        var currentUserId = chatWithIdRequest.UserId;

        var existedChat = await _unitOfWork
            .ChatRepository
            .GetChatByIdAsync(chatId, token);

        if (existedChat != null)
        {
            if (existedChat.User1Id != currentUserId
                && existedChat.User2Id != currentUserId)
            {
                var response1 = new Response();
                response1.Result = false;
                response1.OutInfo =
                    $"Cant take info about chat with id" +
                    $" {chatId} cause current user with id {currentUserId}" +
                    $" not relates to users from direct chat with id {chatId}";

                return BadRequest(JsonConvert.SerializeObject(response1));
            }

            var newName = chatWithIdRequest.Caption;

            await _unitOfWork.ChatRepository.UpdateNameAsync(chatId, newName, token);

            _unitOfWork.SaveChanges();

            var response = new Response();
            response.Result = true;
            response.OutInfo = 
                $"Chat with id {chatId} has been renamed" +
                $" by user with id {currentUserId}";

            var json = JsonConvert.SerializeObject(response);

            return Ok(json);
        }

        var response2 = new Response();
        response2.Result = true;
        response2.OutInfo = $"No such chat with id {chatId}";

        return BadRequest(JsonConvert.SerializeObject(response2));
    }

    [Route("get_user_chats")]
    [Authorize(AuthenticationSchemes = AuthentificationSchemesNamesConst.TokenAuthenticationDefaultScheme)]
    public async Task<IActionResult> GetUserChats(CancellationToken token)
    {
        var body = await RequestExtensions.ReadRequestBodyAsync(Request.Body);

        var userWithIdRequest =
            JsonConvert.DeserializeObject<UserInfoRequest>(body);

        Debug.Assert(userWithIdRequest != null);

        var userId = userWithIdRequest.UserId;

        var existedUser = await _unitOfWork
            .UsersRepository
            .GetUserByIdAsync(userId, token);

        if (existedUser == null)
        {
            var response1 = new Response();
            response1.Result = false;
            response1.OutInfo =
                $"Info about user chats was not received cause user" +
                $" with id {userId} was not found in db";

            return BadRequest(JsonConvert.SerializeObject(response1));
        }

        var userChats = await _unitOfWork
            .ChatRepository
            .GetAllChatsByUserIdAsync(userId, token);

        var allUsers = await _unitOfWork.UsersRepository.GetAllUsersAsync(token);

        var userChatsIdsModels = userChats.Select(chat =>
        {
            var chatId = chat.Id;

            var receiverId = chat.User1Id == userId
                ? chat.User2Id
                : chat.User1Id;

            var existedReceiver = 
                allUsers.FirstOrDefault(x => x.Id == receiverId);

            if (existedReceiver == null)
            {
                return null;
            }

            return new ShortChatInfoResponse
            {
                ChatId = chatId,
                Caption = chat.Caption,
                ReceiverId = receiverId,
                ReceiverName = existedReceiver.UserName
            };
        });

        var notNullChats = userChatsIdsModels.Where(x => x != null).ToList();

        var userChatsInfo = 
            new UsersChatsResponse 
            {
                ChatsWithIds = notNullChats! 
            };

        var getResponse = new GetResponse();
        getResponse.Result = true;
        getResponse.OutInfo =
            $"Info about existed user {userId} chats was received";
        getResponse.RequestedInfo = userChatsInfo;

        var json = JsonConvert.SerializeObject(getResponse);

        return Ok(json);
    }

    private readonly IUsersMessagingUnitOfWork _unitOfWork;
}
