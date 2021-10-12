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
        private MongoDBSettings _mongoDatabaseSettings;
        public MongoClient _mongoDatabaseClient { get; set; }
        public IMongoDatabase _db { get; set; }
        public ChatStoreLoggerService(IDistributedCache cache, IOptions<MongoDBSettings> mongoDatabaseSettings,IOptions<RedisDatabaseSettings> redisDatabaseSettings) : base(cache, redisDatabaseSettings)
        {
            _mongoDatabaseSettings = mongoDatabaseSettings.Value;

            _mongoDatabaseClient = new MongoClient(_mongoDatabaseSettings.ConnectionString);
            _db = _mongoDatabaseClient.GetDatabase(_mongoDatabaseSettings.DatabaseName);

            Init().Wait();

        }
        protected virtual async Task Init()
        {
            
            bool found = _db.ListCollectionNames().ToList().Exists(c => c == GetChatCollectionName());
            if (!found)
            {
                _db.CreateCollection(GetChatCollectionName());
                if (Groups != null)
                    await _db.GetCollection<Group>(GetChatCollectionName()).InsertManyAsync(Groups);

            }

        }
        public override async Task<bool> SaveAsync()
        {
            bool IsSaved = await base.SaveAsync();

            foreach (var g in Groups)
                _db.GetCollection<Group>(GetChatCollectionName()).ReplaceOne(a => a.GroupName == g.GroupName, g);

            return IsSaved;
        }
        private string GetChatCollectionName()
        {
            return $"{_mongoDatabaseSettings.ChatCollectionName}_{DateTime.Now.ToString("MM_dd_yyyy")}";
        }
    }

}