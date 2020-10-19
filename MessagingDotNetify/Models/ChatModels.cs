using DotNetify;
using System;
using System.Threading;
using UAParser;

namespace MessagingDotNetify.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Text { get; set; }
    }

    public class ChatUser
    {
        private static int _counter = 0;

        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }

        public ChatUser(IConnectionContext connectionContext, string correlationId)
        {
            Id = connectionContext.ConnectionId;
            CorrelationId = correlationId;
            Name = $"user{Interlocked.Increment(ref _counter)}";
            IpAddress = connectionContext.HttpConnection.RemoteIpAddress.ToString();

            var browserInfo = Parser.GetDefault().Parse(connectionContext.HttpRequestHeaders.UserAgent);
            if (browserInfo != null)
                Browser = $"{browserInfo.UserAgent.Family}/{browserInfo.OS.Family} {browserInfo.OS.Major}";
        }
    }
}
