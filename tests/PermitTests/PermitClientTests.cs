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

        [Fact]
        public async void TestPermitClientApi()
        {
            Client permitClient = new Client(testToken);
            string testKey = "testKey";
            string testFirstName = "testFirstName";
            string testLastName = "testlastName";
            string testEmail = "testEmail@email.com";
            Mock<IUserKey> testUserKey = new Mock<IUserKey>();
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

            // Test User Api
            UserKey userObj = new UserKey(testKey, testFirstName, testLastName, testEmail);
            UserKey user = await permitClient.Api.SyncUser(userObj);
            Assert.True(user.firstName == testFirstName);
            UserKey getUser = await permitClient.Api.getUser(user.customId);
            Assert.True(getUser.email == testEmail);

            // test Tenant Api
            //string desc = "desc";
            string desc2 = "desc2";
            string tenantName = "tName";
            string tenantKey = "tKey";
            Tenant tenantObj = new Tenant(tenantKey, tenantName);
            Tenant tenant = await permitClient.Api.CreateTenant(tenantObj);
            Tenant getTenant = await permitClient.Api.getTenant(tenantKey);
            Assert.True(tenant.externalId == getTenant.externalId);
            getTenant.description = desc2;
            Tenant getTenant2 = await permitClient.Api.UpdateTenant(getTenant);
            Assert.True(getTenant2.description == desc2);
            Assert.True(getTenant.externalId == tenantKey);

            // test roles Api
            string roleName = "rName";
            string roleDesc = "rDesc";
            Role roleObj = new Role(roleName, roleDesc);
            Role createdRole = await permitClient.Api.CreateRole(roleObj);
            Role[] roles = await permitClient.Api.GetRoles();
            Assert.True(roles.Length > 0);
            Role getRole = await permitClient.Api.GetRoleById(createdRole.id);
            var assignedRole = await permitClient.Api.AssignRole(
                user.customId,
                getRole.id,
                getTenant.externalId
            );
            RoleAssignment[] assignedRoles = await permitClient.Api.getAssignedRoles(
                getUser.customId
            );
            RoleAssignment[] assignedRoles2 = await permitClient.Api.getAssignedRoles(
                getUser.customId,
                getTenant.externalId
            );
            Assert.True(assignedRoles.Length == assignedRoles2.Length);

            await permitClient.Api.unassignRole(user.customId, getRole.id, getTenant.externalId);
            assignedRoles = await permitClient.Api.getAssignedRoles(getUser.customId);
            Assert.True(assignedRoles.Length == (assignedRoles2.Length - 1));

            await permitClient.Api.DeleteUser(getUser.customId);
            getUser = await permitClient.Api.getUser(user.key);
            Assert.True(getUser == null);
            await permitClient.Api.DeleteTenant(tenantKey);
            getTenant = await permitClient.Api.getTenant(getTenant.externalId);
            Assert.True(getTenant == null);

            // sync resources

        }
    }
}
