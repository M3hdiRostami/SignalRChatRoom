using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication.models;
using WebApplication.Services.Options;

namespace WebApplication.Services
{
    public class ChatStoreService : IChatStoreService
    {
        private readonly IDistributedCache _cache;
        private readonly RedisDatabaseSettings _redisDatabaseSettings;
        protected ICollection<Group> Groups;
        
        public ChatStoreService(IDistributedCache cache, IOptions<RedisDatabaseSettings> redisDatabaseSettings)
        {
            _cache = cache;
            _redisDatabaseSettings = redisDatabaseSettings.Value;

            if (Groups is null)
            { 

                Groups = new List<Group>() { new() { GroupName = "family", GroupAvatar = "group-2.jpg" }, new() { GroupName = "work", GroupAvatar = "group-1.png" }, new() { GroupName = "university", GroupAvatar = "group-3.jpg" } };

                string data = GetStringAsync(_redisDatabaseSettings.ChatCollectionName).Result;
                if (data != null)
                    Groups = JsonConvert.DeserializeObject<ICollection<Group>>(data);
            }
        }

        public virtual ICollection<Group> GetGroupList()
        {
            return Groups;
        }
        public virtual async Task AddUserMessage(Message message)
        {
            Groups.Where(g => g.GroupName == message.GroupName).FirstOrDefault()?.Messages?.Add(message);
            await SaveAsync();
        }
        public virtual ICollection<Group> GetUserJoinedList(string id)
        {
            return Groups.Where(g => g.Members.Contains(id)).ToList();
        }

        public virtual async Task<string> GetStringAsync(string key)
        {
            return await _cache.GetStringAsync(key);
        }
        public virtual async Task JoinToGroup(string id, string groupName)
        {
            Groups.Where(g => g.GroupName == groupName).FirstOrDefault()?.Members.Add(id);
            await SaveAsync();
        }

        public virtual async Task RemoveUserFromJoinedList(string id)
        {
            //if (Groups is null)
            //    return;

            foreach (var i in Groups)
            {
                i.Members.Remove(id);
            }
            await SaveAsync();
        }
        public virtual async Task<bool> SaveAsync()
        {
            var content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Groups));
            await _cache.SetAsync(_redisDatabaseSettings.ChatCollectionName, content, new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddDays(1)));
            return true;
        }

        public virtual async Task<bool> AddGroup(Group group)
        {
            Groups.Add(group);
            return await SaveAsync();
        }
    }
}