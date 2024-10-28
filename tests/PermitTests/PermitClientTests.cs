using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using PermitSDK;
using PermitSDK.Models;
using PermitSDK.OpenAPI;
using Xunit;

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

        // [Fact]
        // public async void TestPermitClientEnforcer()
        // {
        //     string testUser = "testUser";
        //     Mock<IUserKey> testUserKey = new Mock<IUserKey>();
        //     testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

        //     string testAction = "testAction";
        //     string testResource = "testResource";
        //     Permit permitClient = new Permit(testToken, useDefaultTenantIfEmpty: true);
        //     Assert.False(await permitClient.Enforcer.Check(testUser, testAction, testResource));
        //     // create dictionary with partition key and value 11
        //     Dictionary<string, dynamic> attributes = new Dictionary<string, dynamic>();
        //     attributes.Add("partition", 11);
        //     Assert.False(
        //         await permitClient.Enforcer.Check(
        //             new UserKey(testUserKey.Object.key),
        //             testAction,
        //             new ResourceInput(testResource, null, null, attributes)
        //         )
        //     );

        //     // create user resource and assign role to it to get the permission
        //     string postfix = Guid.NewGuid().ToString();
        //     string testKey = string.Concat("testKey", postfix);
        //     string testFirstName = "testFirstName";
        //     string testLastName = "testlastName";
        //     string testEmail = "testEmail@email.com";
        //     testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

        //     // Test User Api
        //     var userObj = new UserCreate
        //     {
        //         Key = testKey,
        //         Email = testEmail,
        //         First_name = testFirstName,
        //         Last_name = testLastName,
        //         Attributes = { }
        //     };
        //     var user = await permitClient.Api.CreateUser(userObj);

        //     var resourceKey = String.Concat("testResource", postfix);
        //     // create resource actions
        //     System.Collections.Generic.IDictionary<
        //         string,
        //         PermitSDK.OpenAPI.ActionBlockEditable
        //     > actions = new Dictionary<string, PermitSDK.OpenAPI.ActionBlockEditable>();
        //     actions.Add("read", new PermitSDK.OpenAPI.ActionBlockEditable { Description = "read" });
        //     var resource = await permitClient.Api.CreateResource(
        //         new ResourceCreate
        //         {
        //             Key = resourceKey,
        //             Name = resourceKey,
        //             Actions = actions
        //         }
        //     );
        //     var roleName = String.Concat("rName", postfix);
        //     string roleDesc = "rDesc";
        //     // add permission to the role
        //     System.Collections.Generic.ICollection<string> permissions = new List<string>();
        //     permissions.Add(string.Concat(resourceKey, ":read"));
        //     var roleCreate = new RoleCreate
        //     {
        //         Name = roleName,
        //         Description = roleDesc,
        //         Key = roleName,
        //         Permissions = permissions
        //     };
        //     var createdRole = await permitClient.Api.CreateRole(roleCreate);
        //     // create tenant
        //     string tenantKey = String.Concat("tKey", postfix);
        //     var tenantCreate = new TenantCreate { Key = tenantKey, Name = tenantKey };
        //     var tenant = await permitClient.Api.CreateTenant(tenantCreate);
        //     // assign role to the user
        //     var assignedRole = await permitClient.Api.AssignRole(
        //         user.Key,
        //         createdRole.Key,
        //         tenant.Key
        //     );
        //     // sleep for 10 second to let the role be assigned
        //     System.Threading.Thread.Sleep(1000);
        //     // check if the user has the permission
        //     Assert.True(
        //         await permitClient.Enforcer.Check(
        //             new UserKey(user.Key),
        //             "read",
        //             new ResourceInput(resourceKey, null, tenant.Key, attributes)
        //         )
        //     );
        //     // get user permissions
        //     var userPermissions = await permitClient.Enforcer.GetUserPermissions(
        //         new PermitSDK.PDP.OpenAPI.User() { Key = user.Key }
        //     );
        //     Assert.True(userPermissions.Count > 0);
        // }

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
                Last_name = testLastName
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
                    Tenant = tenantKey
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
                Key = roleName
            };
            var createdRole = await permitClient.Api.CreateRole(roleCreate);
            var roles = await permitClient.Api.ListRoles();
            Assert.True(roles.Count > 0);

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
            Assert.True(assignedRoles.Count == assignedRoles2.Count);

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
            Assert.True(assignedRoles.Count == (assignedRoles2.Count - 1));

            await permitClient.Api.DeleteUser(getUser.Key);

            await permitClient.Api.DeleteTenant(tenantKey);

            await permitClient.Api.DeleteResource(resource.Id.ToString());
        }

        [Fact]
        public async void TestBulkCheck()
        {
            Permit permit = new Permit(testToken, pdpUrl);
            const string TEST_USER = "test_user";
            const string TEST_TENANT = "test_tenant";
            const string TEST_RESOURCE = "test_doc";
            const string TEST_ROLE = "test_reader";
            UserKey user = new UserKey(TEST_USER);

            // Policy
            var actions = new Dictionary<string, PermitSDK.OpenAPI.ActionBlockEditable>
            {
                { "read", new() },
                { "write", new() }
            };
            try
            {
                var resource = await permit.Api.CreateResource(
                    new ResourceCreate { Key = TEST_RESOURCE, Name = TEST_RESOURCE, Actions = actions }
                );

                var role = await permit.Api.CreateRole(new RoleCreate
                {
                    Key = TEST_ROLE,
                    Name = TEST_ROLE,
                    Permissions = new[] { String.Format("{0}:read", TEST_RESOURCE) }
                });

                // Directory
                var userObj = await permit.Api.CreateUser(new UserCreate
                {
                    Key = user.key,
                    Email = "test@user.com",
                    First_name = "test",
                    Last_name = "user"
                });
                var tenant = await permit.Api.CreateTenant(new TenantCreate { Key = TEST_TENANT, Name = TEST_TENANT });
                await permit.Api.AssignRole(userObj.Key, role.Key, tenant.Key);
            }
            catch (PermitApiException e)
            {
                if (e.StatusCode != 409)
                {
                    throw;
                }
                Console.WriteLine("skipped 409 error on setup", e.Message);
            }

            // bulk check inputs
            var inputs = new List<CheckQueryObj>{
                new CheckQueryObj(user, "read", new ResourceInput(TEST_RESOURCE, tenant: TEST_TENANT), new()),
                new CheckQueryObj(user, "write", new ResourceInput(TEST_RESOURCE, tenant: TEST_TENANT), new()),
            };

            // wait until all changes arrive to the PDP
            Console.WriteLine("sleeping for 10 seconds");
            await Task.Delay(10000); // Delay for 10 seconds

            // bulk check
            var results = await permit.BulkCheck(inputs);

            // verify bulk check results
            Assert.True(results[0]);
            Assert.False(results[1]);

            // bulk check verbose
            var verboseResults = await permit.BulkCheckVerbose(inputs);

            // verify bulk check verbose results
            Assert.Equal(verboseResults[0].Query, inputs[0]);
            Assert.True(verboseResults[0].Result);
            Assert.Equal(verboseResults[1].Query, inputs[1]);
            Assert.False(verboseResults[1].Result);

            // cleanup
            try
            {
                await permit.Api.UnassignRole(TEST_USER, TEST_ROLE, TEST_TENANT);
                await permit.Api.DeleteUser(TEST_USER);
                await permit.Api.DeleteTenant(TEST_TENANT);
                await permit.Api.DeleteResource(TEST_RESOURCE);
            }
            catch (PermitApiException e)
            {
                if (e.StatusCode != 404)
                {
                    throw;
                }
                Console.WriteLine("skipped 404 error on cleanup", e.Message);
            }
        }
    }
}
