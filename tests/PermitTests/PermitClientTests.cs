using System;
using System.Collections.Generic;
using Moq;
using PermitSDK.Models;
using PermitSDK.OpenAPI.Models;
using Xunit;

namespace PermitSDK.Tests
{
    public class ClientTest
    {
        private string testToken = Environment.GetEnvironmentVariable("PERMIT_API_KEY");
        private string pdpUrl =
            Environment.GetEnvironmentVariable("PDP_URL") ?? "http://localhost:7766";

        [Fact]
        public void TestPermitConfig()
        {
            Assert.True(testToken != null, "PERMIT_API_KEY environment variable is not set");
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

            // create user resource and assign role to it to get the permission
            string postfix = Guid.NewGuid().ToString();
            string testKey = string.Concat("testKey", postfix);
            string testFirstName = "testFirstName";
            string testLastName = "testlastName";
            string testEmail = "testEmail@email.com";
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

            // Test User Api
            var userObj = new UserCreate
            {
                Key = testKey,
                Email = testEmail,
                First_name = testFirstName,
                Last_name = testLastName,
                Attributes = { },
            };
            var user = await permitClient.Api.CreateUser(userObj);

            var resourceKey = String.Concat("testResource", postfix);
            // create resource actions
            System.Collections.Generic.IDictionary<string, ActionBlockEditable> actions =
                new Dictionary<string, ActionBlockEditable>();
            actions.Add("read", new ActionBlockEditable { Description = "read" });
            var resource = await permitClient.Api.CreateResource(
                new ResourceCreate
                {
                    Key = resourceKey,
                    Name = resourceKey,
                    Actions = actions,
                }
            );
            var roleName = String.Concat("rName", postfix);
            string roleDesc = "rDesc";
            // add permission to the role
            System.Collections.Generic.ICollection<string> permissions = new List<string>();
            permissions.Add(string.Concat(resourceKey, ":read"));
            var roleCreate = new RoleCreate
            {
                Name = roleName,
                Description = roleDesc,
                Key = roleName,
                Permissions = permissions,
            };
            var createdRole = await permitClient.Api.CreateRole(roleCreate);
            // create tenant
            string tenantKey = String.Concat("tKey", postfix);
            var tenantCreate = new TenantCreate { Key = tenantKey, Name = tenantKey };
            var tenant = await permitClient.Api.CreateTenant(tenantCreate);
            // assign role to the user
            var assignedRole = await permitClient.Api.AssignRole(
                user.Key,
                createdRole.Key,
                tenant.Key
            );
            // sleep for 10 second to let the role be assigned
            System.Threading.Thread.Sleep(1000);
            // check if the user has the permission
            Assert.True(
                await permitClient.Enforcer.Check(
                    new UserKey(user.Key),
                    "read",
                    new ResourceInput(resourceKey, null, tenant.Key, attributes)
                )
            );
            // get user permissions
            var userPermissions = await permitClient.Enforcer.GetUserPermissions(
                new PermitSDK.PDP.OpenAPI.User() { Key = user.Key }
            );
            Assert.True(userPermissions.Count > 0);
        }

        // test resource input parsing
        [Fact]
        public async void TestResourceInput()
        {
            string testResource = "testResource";
            string testResourceKey = "testResourceKey";
            string testTenant = "testTenant";
            Dictionary<string, dynamic> attributes = new Dictionary<string, dynamic>();
            attributes.Add("partition", 11);
            ResourceInput resourceInput = new ResourceInput(
                testResource,
                testResourceKey,
                testTenant,
                attributes
            );
            Assert.True(resourceInput.type == testResource);
            Assert.True(resourceInput.key == testResourceKey);
            Assert.True(resourceInput.tenant == testTenant);
            Assert.True(resourceInput.attributes == attributes);

            // ReBAC parsing
            string rebacResource = "Folder:project1";
            ResourceInput rebacResourceInput = ResourceInput.ResourceFromString(rebacResource);
            Assert.True(rebacResourceInput.type == "Folder");
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
            {
                Key = testKey,
                Email = testEmail,
                First_name = testFirstName,
                Last_name = testLastName,
            };
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

            var getTenant2 = await permitClient.Api.UpdateTenant(
                tenantKey,
                new TenantUpdate { Description = desc2 }
            );
            Assert.True(getTenant2.Description == desc2);
            Assert.True(getTenant.Key == tenantKey);

            // test Resource Api
            var resourceKey = String.Concat("testResource", postfix);
            var resource = await permitClient.Api.CreateResource(
                new ResourceCreate { Key = resourceKey, Name = resourceKey }
            );
            var getResource = await permitClient.Api.GetResource(resource.Id.ToString());
            Assert.True(resource.Key == getResource.Key);

            // test ResourceInstance Api
            var resourceInstanceKey = String.Concat("testResourceInstance", postfix);
            var resourceInstance = await permitClient.Api.CreateResourceInstance(
                new ResourceInstanceCreate
                {
                    Key = resourceInstanceKey,
                    Resource = resourceKey,
                    Tenant = tenantKey,
                }
            );
            var getResourceInstance = await permitClient.Api.GetResourceInstance(
                resourceInstance.Id.ToString()
            );
            Assert.True(resourceInstance.Key == getResourceInstance.Key);

            // test roles Api
            string roleName = String.Concat("rName", postfix);
            string roleDesc = "rDesc";
            var roleCreate = new RoleCreate
            {
                Name = roleName,
                Description = roleDesc,
                Key = roleName,
            };
            var createdRole = await permitClient.Api.CreateRole(roleCreate);
            var roles = await permitClient.Api.ListRoles();
            Assert.True(roles.Total_count > 0);

            var getRole = await permitClient.Api.GetRole(createdRole.Id.ToString());
            var assignedRole = await permitClient.Api.AssignRole(
                user.Key,
                getRole.Key,
                getTenant.Key
            );
            var assignedRoles = await permitClient.Api.ListAssignedRoles(getUser.Key);
            var assignedRoles2 = await permitClient.Api.ListAssignedRoles(
                getUser.Key,
                getTenant.Key
            );
            Assert.True(assignedRoles.Total_count == assignedRoles2.Total_count);

            // test elements login as with this user only after assigning roles to it
            var elementsLogin = await permitClient.Elements.LoginAs(user.Key, getTenant.Key);
            Assert.True(elementsLogin.Token != null);
            Assert.True(elementsLogin.RedirectUrl != null);

            // test resource role api
            var ResourceRoleKey = String.Concat("ResourceRoleKey", postfix);
            var resourceRole = await permitClient.Api.CreateResourceRole(
                resourceKey,
                new ResourceRoleCreate { Key = ResourceRoleKey, Name = ResourceRoleKey }
            );
            var getResourceRole = await permitClient.Api.GetResourceRole(
                resourceKey,
                resourceRole.Id.ToString()
            );
            Assert.True(resourceRole.Key == getResourceRole.Key);

            // assign this resource role to the user
            var assignedResourceRole = await permitClient.Api.AssignRole(
                user.Key,
                getResourceRole.Key,
                getTenant.Key,
                getResourceInstance.Key,
                getResource.Key
            );

            await permitClient.Api.UnassignRole(
                user.Key,
                getResourceRole.Id.ToString(),
                getTenant.Key,
                getResourceInstance.Key,
                getResource.Key
            );

            await permitClient.Api.UnassignRole(user.Key, getRole.Id.ToString(), getTenant.Key);

            assignedRoles = await permitClient.Api.ListAssignedRoles(getUser.Key);
            Assert.True(assignedRoles.Total_count == (assignedRoles2.Total_count - 1));

            await permitClient.Api.DeleteUser(getUser.Key);

            await permitClient.Api.DeleteTenant(tenantKey);

            await permitClient.Api.DeleteResource(resource.Id.ToString());
        }
    }
}
