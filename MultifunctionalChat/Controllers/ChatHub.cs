using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;
using System;
using Microsoft.AspNetCore.Authorization;

namespace SignalRApp
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task Send(string message, string userName, string roomId)
        {
            await Clients.All.SendAsync("Receive", message, userName, roomId);
        }
        public async Task RefreshUsers(string roomId)
        {
            await Clients.All.SendAsync("RefreshUsers", roomId);
        }
    }
}