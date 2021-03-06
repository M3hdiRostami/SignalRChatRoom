using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.models;
using WebApplication.Services;

namespace WebApplication.Features.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatStoreService _chatStoreService;

        public ChatHub(IChatStoreService chatStoreService)
        {
            this._chatStoreService = chatStoreService;
        }
        public async Task SendUserAction(string user, string message)
        {

            await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveUserAction", user, message);
        }


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            await Clients.All.SendAsync("OnConnected " + Context.ConnectionId);

        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            await _chatStoreService.RemoveUserFromJoinedList(Context.ConnectionId);
            await LoadGroupsList();
            await Clients.All.SendAsync("OnDisconnected " + Context.ConnectionId);
        }


        public async Task SendToConnectionId(string connectionId, Message message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(message));
        }

        public async Task SendToAll(Message message)
        {
            await Clients.All.SendAsync("ReceiveMessage", JsonConvert.SerializeObject(message));
        }

        public async Task SendToGroup(Message message)
        {
            if (Context.ConnectionId.Equals(message.ConnectionId))
            {

                await _chatStoreService.AddUserMessage(message);
                await Clients.Group(message.GroupName).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(message));
            }
        }
        public async Task SendMyJoinedGroupsList(string? connectionId)
        {

            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Context.ConnectionId;
            }
            var grouplist = _chatStoreService.GetUserJoinedList(connectionId);
            await Clients.Caller.SendAsync("ReceiveMyJoinedGroupsList", JsonConvert.SerializeObject(grouplist));

        }
        public async Task JoinGroup(string groupName, string? connectionId, string userName)
        {

            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Context.ConnectionId;
            }

            await _chatStoreService.JoinToGroup(connectionId, groupName);
            await Groups.AddToGroupAsync(connectionId, groupName);
            Message message = new Message
            {
                Sender = "notifier",
                Type = "join",
                ConnectionId = connectionId,
                GroupName = groupName,
                Content = userName + " joined group: " + groupName,
            };
            await LoadGroupsList();
            await SendMyJoinedGroupsList(connectionId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(message));

        }

        public async Task LeaveGroup(string groupName, string? connectionId, string userName)
        {
            await _chatStoreService.RemoveUserFromJoinedList(Context.ConnectionId);

            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Context.ConnectionId;
            }
            await Groups.RemoveFromGroupAsync(connectionId, groupName);

            Message message = new Message
            {
                Sender = "notifier",
                Type = "leave",
                ConnectionId = connectionId,
                GroupName = groupName,
                Content = userName + " left group: " + groupName,
            };
            await LoadGroupsList();
            await SendMyJoinedGroupsList(connectionId);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", JsonConvert.SerializeObject(message));

        }
        public async Task LoadGroupsList()
        {
            //todo:If more mapping is required use mapping tools such as AutoMapper,Mapster
            var groupsList = _chatStoreService.GetGroupList().Select(g => new { g.GroupAvatar,g.GroupName,g.MembersCount});
            await Clients.All.SendAsync("loadGroupsList", JsonConvert.SerializeObject(groupsList));

        }


    }
}