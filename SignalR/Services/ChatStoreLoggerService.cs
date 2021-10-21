using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using WebApplication.models;
using WebApplication.Services.Options;

namespace WebApplication.Services
{

    public class ChatStoreLoggerService : ChatStoreService
    {
        protected MongoDBSettings _mongoDatabaseSettings;
        protected MongoClient MongoDatabaseClient { get; set; }
        protected IMongoDatabase MongoDB { get; set; }
        public ChatStoreLoggerService(IDistributedCache cache, IOptions<MongoDBSettings> mongoDatabaseSettings,IOptions<RedisDatabaseSettings> redisDatabaseSettings) : base(cache, redisDatabaseSettings)
        {
            _mongoDatabaseSettings = mongoDatabaseSettings.Value;

            MongoDatabaseClient = new MongoClient(_mongoDatabaseSettings.ConnectionString);
            MongoDB = MongoDatabaseClient.GetDatabase(_mongoDatabaseSettings.DatabaseName);

            Init().Wait();

        }
        protected virtual async Task Init()
        {
            
            bool found = MongoDB.ListCollectionNames().ToList().Exists(c => c == GetChatCollectionName());
            if (!found)
            {
                MongoDB.CreateCollection(GetChatCollectionName());
                if (Groups != null)
                    await MongoDB.GetCollection<Group>(GetChatCollectionName()).InsertManyAsync(Groups);

            }

        }
        public override async Task<bool> SaveAsync()
        {
            bool IsSaved = await base.SaveAsync();

            foreach (var g in Groups)
                MongoDB.GetCollection<Group>(GetChatCollectionName()).ReplaceOne(a => a.GroupName == g.GroupName, g);

            return IsSaved;
        }
        private string GetChatCollectionName()
        {
            return $"{_mongoDatabaseSettings.ChatCollectionName}_{DateTime.Now.ToString("MM_dd_yyyy")}";
        }
    }

}