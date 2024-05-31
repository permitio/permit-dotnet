using Xunit;
using System.Collections.Generic;
using Moq;
using PermitSDK;
using PermitSDK.Models;
using PermitSDK.OpenAPI;
using System;

namespace PermitSDK.Tests
{
    public class ClientTest
    {
        private string testToken = "";
        private string pdpUrl = "http://localhost:7766";

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
            Permit permitClient = new Permit(testToken, useDefaultTenantIfEmpty: true);
            Assert.False(await permitClient.Enforcer.Check(testUser, testAction, testResource));
            // create dictionary with partition key and value 11
            Dictionary<string, dynamic> attributes = new Dictionary<string, dynamic>();
            attributes.Add("partition", 11);
            Assert.False(
                await permitClient.Enforcer.Check(
                    new UserKey(testUserKey.Object.key),
                    testAction,
                    new ResourceInput(testResource, null, null, attributes)
                )
            );
        }

        [Fact]
        public async void TestPermitClientApi()
        {
            Permit permitClient = new Permit(testToken, pdpUrl);
            // adding UUID postfix so I will not have conflicts
            string postfix = Guid.NewGuid().ToString();
            string testKey = string.Concat("testKey", postfix);
            string testFirstName = "testFirstName";
            string testLastName = "testlastName";
            string testEmail = "testEmail@email.com";
            Mock<IUserKey> testUserKey = new Mock<IUserKey>();
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

            // Test User Api
            var userObj = new UserCreate
            { Key = testKey, Email = testEmail, First_name = testFirstName, Last_name = testLastName };
            var user = await permitClient.Api.CreateUser(userObj);
            Assert.True(user.First_name == testFirstName);
            var getUser = await permitClient.Api.GetUser(user.Key);
            Assert.True(getUser.Email == testEmail);

            // test Tenant Api
            //string desc = "desc";
            string desc2 = "desc21";
            string tenantName = "tName";
            string tenantKey = String.Concat("tKey", postfix);
            var tenantCreate = new TenantCreate { Key = tenantKey, Name = tenantName };
            var tenant = await permitClient.Api.CreateTenant(tenantCreate);
            var getTenant = await permitClient.Api.GetTenant(tenantKey);
            Assert.True(tenant.Key == getTenant.Key);

            var getTenant2 = await permitClient.Api.UpdateTenant(tenantKey, new TenantUpdate { Description = desc2 });
            Assert.True(getTenant2.Description == desc2);
            Assert.True(getTenant.Key == tenantKey);


            // test Resource Api
            var resourceKey = String.Concat("testResource", postfix);
            var resource = await permitClient.Api.CreateResource(new ResourceCreate { Key = resourceKey, Name = resourceKey });
            var getResource = await permitClient.Api.GetResource(resource.Id.ToString());
            Assert.True(resource.Key == getResource.Key);

            // test ResourceInstance Api
            var resourceInstanceKey = String.Concat("testResourceInstance", postfix);
            var resourceInstance = await permitClient.Api.CreateResourceInstance(
                new ResourceInstanceCreate { Key = resourceInstanceKey, Resource = resourceKey, Tenant = tenantKey }
            );
            var getResourceInstance = await permitClient.Api.GetResourceInstance(resourceInstance.Id.ToString());
            Assert.True(resourceInstance.Key == getResourceInstance.Key);

            // test roles Api
            string roleName = String.Concat("rName", postfix);
            string roleDesc = "rDesc";
            var roleCreate = new RoleCreate { Name = roleName, Description = roleDesc, Key = roleName };
            var createdRole = await permitClient.Api.CreateRole(roleCreate);
            var roles = await permitClient.Api.ListRoles();
            Assert.True(roles.Count > 0);

            var getRole = await permitClient.Api.GetRole(createdRole.Id.ToString());
            var assignedRole = await permitClient.Api.AssignRole(
                user.Key,
                getRole.Key,
                getTenant.Key
            );
            var assignedRoles = await permitClient.Api.ListAssignedRoles(
                getUser.Key
            );
            var assignedRoles2 = await permitClient.Api.ListAssignedRoles(
                getUser.Key,
                getTenant.Key
            );
            Assert.True(assignedRoles.Count == assignedRoles2.Count);

            await permitClient.Api.UnassignRole(user.Key, getRole.Id.ToString(), getTenant.Key);
            assignedRoles = await permitClient.Api.ListAssignedRoles(getUser.Key);
            Assert.True(assignedRoles.Count == (assignedRoles2.Count - 1));

            await permitClient.Api.DeleteUser(getUser.Key);

            await permitClient.Api.DeleteTenant(tenantKey);
        }
    }
}
