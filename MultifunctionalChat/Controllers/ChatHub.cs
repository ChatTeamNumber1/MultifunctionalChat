using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using MultifunctionalChat.Models;
using MultifunctionalChat.Services;
using System;

namespace SignalRApp
{
    public class ChatHub : Hub
    {
        public async Task Send(string message, string userName)
        {
            await Clients.All.SendAsync("Send", message, userName);
        }
    }
}