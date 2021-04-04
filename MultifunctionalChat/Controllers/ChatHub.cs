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
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Send", message, userName);
        }
    }
}