using System;

namespace WebApplication.models
{
    public class Message
    {
        public Message()
        {
            SentTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        }
        public string Type { get; set; }
        public string ConnectionId { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
        public string GroupName { get; set; }
        public string SentTime { get; set; }
    }



}