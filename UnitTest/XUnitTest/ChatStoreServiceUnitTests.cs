using System.Linq;
using System.Threading.Tasks;
using WebApplication.models;
using WebApplication.Services;
using Xunit;
using XUnitPriorityOrderer;
using XUnitTest.helpers;

namespace XUnitTestProject
{
    [TestCaseOrderer(CasePriorityOrderer.TypeName, CasePriorityOrderer.AssembyName)]
    public class ChatStoreServiceUnitTests : DependencyResolverHelper
    {
        private IChatStoreService _chatStoreService;
        public ChatStoreServiceUnitTests()
        {
            _chatStoreService = GetService<ChatStoreService>();

        }
        [Fact, Order(1)]
        public async Task Is_ChatStoreService_Initialed_Correctly()
        {
            Assert.NotNull(_chatStoreService);
        }
        [Fact, Order(2)]
        public async Task Is_AddGroup_Works()
        {

            //Arrange
            Group group = new Group() { GroupName = "test", GroupAvatar = "test" };

            //Act
            _chatStoreService.AddGroup(group);
            var found = (_chatStoreService.GetGroupList().Contains(group));

            //Assert
            Assert.Equal(found, true);

        }
        [Fact, Order(3)]
        public async Task Is_Join_Works()
        {
              
            //Arrange
            Group group = new Group() { GroupName = "test", GroupAvatar = "test" };

            //Act
            await _chatStoreService.JoinToGroup("userID1", group.GroupName);
            var found = (_chatStoreService.GetUserJoinedList("userID1").Any(G => G.GroupName == group.GroupName));

            //Assert
            Assert.Equal(found, true);

        }


    }
}
