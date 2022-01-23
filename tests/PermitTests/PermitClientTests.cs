using Xunit;
using System.Collections.Generic;
using Moq;
using Permit;
using Permit.Models;

namespace Permit.Tests
{
    public class ClientTest
    {
        //[Fact]
        //public async void TestPermitCheck()
        //{
        //    Dictionary<string, string> openWith = new Dictionary<string, string>();
        //    User myUser = new User("test@email.com");
        //    var testUser = (IUserKey) (new Dictionary<string, string> { { "key" , "test" } });
        //    ResourceInput resource = new ResourceInput("Document");

        //    // Add some elements to the dictionary. There are no
        //    // duplicate keys, but some of the values are duplicates.
        //    openWith.Add("txt", "notepad.exe");

        //    //var enforcer = new Enforcer("testurl");
        //    //Assert.True(await enforcer.Check("user", "action", "resource", openWith));
        //    //Assert.True(await enforcer.Check(testUser, "action", resource, openWith));
        //}

        [Fact]
        public void TestPermitConfig()
        {
            Config config = new Config(testToken);
            Assert.True(config.Token == testToken);
            Assert.True(config.Pdp == Config.DEFAULT_PDP_URL);
        }

        [Fact]
        public async void TestPermitClientEnforcer()
        {
            string testUser = "testUser";
            Mock<IUserKey> testUserKey = new Mock<IUserKey>();
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

            string testAction = "testAction";
            string testResource = "testResource";
            Client permitClient = new Client(testToken);
            Assert.False(await permitClient.Enforcer.Check(testUser, testAction, testResource));
            Assert.False(
                await permitClient.Enforcer.Check(
                    testUserKey.Object,
                    testAction,
                    new ResourceInput(testResource)
                )
            );
        }

        [Fact]
        public async void TestPermitClientCache()
        {
            Mock<IUserKey> testUserKey = new Mock<IUserKey>();
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");
            Client permitClient = new Client(testToken);

            SyncedUser[] users = await permitClient.Cache.GetUsers();
            Assert.True(users.Length > 0);
            SyncedUser user = await permitClient.Cache.getUser(users[0].id);
            Assert.True(await permitClient.Cache.isUser(user.id));
            string[] userTenatns = await permitClient.Cache.getUserTenants(user.id);
            Assert.True(userTenatns.Length > 0);
            SyncedRole[] userRoles = await permitClient.Cache.getAssignedRoles(user.id);
            Assert.True(userRoles != null);
            SyncedRole[] roles = await permitClient.Cache.GetRoles();
            Assert.True(roles != null);
            SyncedRole roleById = await permitClient.Cache.GetRoleById(roles[0].id);
            Assert.True(roleById != null);
            SyncedRole roleByName = await permitClient.Cache.GetRoleByName(roleById.name);
            Assert.True(roleByName != null);

            Assert.True(await permitClient.Cache.TriggerDataUpdate());
            Assert.True(await permitClient.Cache.TriggerPolicyUpdate());
            Assert.True(await permitClient.Cache.TriggerDataAndPolicyUpdate());


        }
    }
}
