using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Repository.IRepository;
using System.Collections.Concurrent;

namespace RealEstate_ServicesSystem.SignalR.ChatSignalR
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<Applicationuser> _userManager;
        private readonly IRepository<Massage> _context;
        private static ConcurrentDictionary<string, string> _users =
          new ConcurrentDictionary<string, string>();

        public ChatHub(UserManager<Applicationuser> userManager, IRepository<Massage> context)
        {
            _userManager = userManager;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                _users[userId] = Context.ConnectionId;
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                _users.TryRemove(userId, out _);
            }

            await base.OnDisconnectedAsync(exception);
        }


        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(senderId))
                return;

            var msg = new Massage()
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SentAt = DateTime.UtcNow
            };
            await _context.AddAsync(msg);
            await _context.SaveChangesAsync();

            // لو اليوزر Online
            await Clients.Caller
        .SendAsync("ReceiveMessage", senderId, message);

            // receiver if online
            if (_users.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId)
                    .SendAsync("ReceiveMessage", senderId, message);
            }
        }
    }

}

