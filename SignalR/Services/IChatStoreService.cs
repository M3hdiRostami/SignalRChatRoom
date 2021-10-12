using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.models;

namespace WebApplication.Services
{
    public interface IChatStoreService
    {
        Task AddUserMessage(Message message);
        Task<string> GetStringAsync(string key);
        ICollection<Group> GetUserJoinedList(string id);
        ICollection<Group> GetGroupList();
        Task<bool> AddGroup(Group group);
        Task JoinToGroup(string id, string groupName);
        Task RemoveUserFromJoinedList(string id);
        Task<bool> SaveAsync();
    }
}