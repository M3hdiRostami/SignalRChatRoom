using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace WebApplication.models
{
    public class Group
    {
        public Group()
        {
            Members = new HashSet<string>();
            Messages = new List<Message>();
        }

        public int MembersCount { get => Members?.Count ?? 0; }
        [BsonId]
        public string GroupName { get; set; }
        public string GroupAvatar { get; set; }
        public HashSet<string> Members { get; set; }

        public ICollection<Message> Messages { get; }

    }


}