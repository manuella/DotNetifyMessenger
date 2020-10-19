using DotNetify;
using MessagingDotNetify.Models;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessagingDotNetify.Controllers
{
    public class MessengerVM : MulticastVM
    {
        private readonly IConnectionContext _connectionContext;

        [ItemKey(nameof(ChatMessage.Id))]
        public List<ChatMessage> Messages { get; } = new List<ChatMessage>();

        [ItemKey(nameof(ChatUser.Id))]
        public List<ChatUser> Users { get; } = new List<ChatUser>();

        public Action<ChatMessage> SendMessage => chat =>
        {
            string userId = _connectionContext.ConnectionId;
            chat.Id = Messages.Count + 1;
            chat.UserId = userId;
            chat.UserName = UpdateUserName(userId, chat.UserName);

            var privateMessageUser = Users.FirstOrDefault(x => chat.Text.StartsWith($"{x.Name}:"));
            if (privateMessageUser != null)
                base.Send(new List<string> { privateMessageUser.Id, userId }, "PrivateMessage", chat);
            else
            {
                lock (Messages)
                {
                    Messages.Add(chat);
                    this.AddList(nameof(Messages), chat);
                }
            }
        };

        public Action<string> AddUser => correlationId =>
        {
            var user = new ChatUser(_connectionContext, correlationId);
            lock (Users)
            {
                Users.Add(user);
                this.AddList(nameof(Users), user);
            }
        };

        public Action RemoveUser => () =>
        {
            lock (Users)
            {
                var user = Users.FirstOrDefault(x => x.Id == _connectionContext.ConnectionId);
                if (user != null)
                {
                    Users.Remove(user);
                    this.RemoveList(nameof(Users), user.Id);
                }
            }
        };

        public MessengerVM(IConnectionContext connectionContext)
        {
            _connectionContext = connectionContext;
        }

        public override void Dispose()
        {
            RemoveUser();
            PushUpdates();
            base.Dispose();
        }

        private string UpdateUserName(string userId, string userName)
        {
            lock (Users)
            {
                var user = Users.FirstOrDefault(x => x.Id == userId);
                if (user != null)
                {
                    if (!string.IsNullOrEmpty(userName))
                    {
                        user.Name = userName;
                        this.UpdateList(nameof(Users), user);
                    }
                    return user.Name;
                }
            }
            return userId;
        }
    }
 }
