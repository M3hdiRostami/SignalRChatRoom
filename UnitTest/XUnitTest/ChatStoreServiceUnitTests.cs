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
        private IChatStoreService ChatStoreService;
        public ChatStoreServiceUnitTests()
        {
            ChatStoreService = GetService<ChatStoreService>();

        }
        [Fact, Order(1)]
        public async Task Is_ChatStoreService_Initialed_Correctly()
        {
            Assert.NotNull(ChatStoreService);
        }
        [Fact, Order(2)]
        public async Task Is_AddGroup_Works()
        {

            //Arrange
            Group g = new Group() { GroupName = "test", GroupAvatar = "test" };

            //Act
            ChatStoreService.AddGroup(g);
            var found = (ChatStoreService.GetGroupList().Contains(g));

            //Assert
            Assert.Equal(found, true);

        }
        [Fact, Order(3)]
        public async Task Is_Join_Works()
        {

            //Arrange
            Group g = new Group() { GroupName = "test", GroupAvatar = "test" };

            //Act
            await ChatStoreService.JoinToGroup("userID1", g.GroupName);
            var found = (ChatStoreService.GetUserJoinedList("userID1").Any(G => G.GroupName == g.GroupName));

            //Assert
            Assert.Equal(found, true);

        }


    }
}
