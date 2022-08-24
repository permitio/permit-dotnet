using System;
using Xunit;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Moq;
using PermitSDK;
using PermitSDK.Models;
using PermitSDK.NewAPI;
using Role = PermitSDK.Models.Role;
using Tenant = PermitSDK.Models.Tenant;

namespace PermitSDK.Tests
{
    public class ClientTest
    {
        private string testToken = "permit_key_enEfdDEyWzR7em0yldQMKNBYxXssPAf2QsIwsJpuWj2SQC4IGBpOVmAnllCsgWN6Iwlh1pHLpyLsiPPVgj1Ylu";

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
            Permit permitClient = new Permit(testToken, "http://localhost:7000", "http://localhost:8000", "default", true);
            Assert.False(await permitClient.Enforcer.Check(testUser, testAction, testResource));
            Assert.False(
                await permitClient.Enforcer.Check(
                    testUserKey.Object,
                    testAction,
                    new ResourceInput(testResource)
                )
            );
        }

        // [Fact]
        // public async void TestPermitClientCache()
        // {
        //     Mock<IUserKey> testUserKey = new Mock<IUserKey>();
        //     testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");
        //     Permit permitClient = new Permit(testToken, "http://localhost:7000", "http://localhost:8000", "default", true);
        //
        //     SyncedUser[] users = await permitClient.Cache.GetUsers();
        //     Assert.True(users.Length > 0);
        //     SyncedUser user = await permitClient.Cache.getUser(users[0].id);
        //     Assert.True(await permitClient.Cache.isUser(user.id));
        //     string[] userTenatns = await permitClient.Cache.getUserTenants(user.id);
        //     Assert.True(userTenatns.Length > 0);
        //     SyncedRole[] userRoles = await permitClient.Cache.getAssignedRoles(user.id);
        //     Assert.True(userRoles != null);
        //     SyncedRole[] roles = await permitClient.Cache.GetRoles();
        //     Assert.True(roles != null);
        //     SyncedRole roleById = await permitClient.Cache.GetRoleById(roles[0].id);
        //     Assert.True(roleById != null);
        //     SyncedRole roleByName = await permitClient.Cache.GetRoleByName(roleById.name);
        //     Assert.True(roleByName != null);
        //
        //     Assert.True(await permitClient.Cache.TriggerDataUpdate());
        //     Assert.True(await permitClient.Cache.TriggerPolicyUpdate());
        //     Assert.True(await permitClient.Cache.TriggerDataAndPolicyUpdate());
        // }

        [Fact]
        public async void TestPermitClientApi()
        {
            Permit permitClient = new Permit(testToken, "http://localhost:7000", "http://localhost:8000", "default", true);
            string testKey = "testKey";
            string testFirstName = "testFirstName";
            string testLastName = "testlastName";
            string testEmail = "testEmail@email.com";
            Mock<IUserKey> testUserKey = new Mock<IUserKey>();
            testUserKey.Setup(testUserKey => testUserKey.key).Returns("test");

            // Test User Api
            UserCreate userObj = new UserCreate()
                { Email = testEmail, First_name = testFirstName, Last_name = testLastName, Key = testKey };
            UserRead user = await permitClient.Api.SyncUser(userObj);
            Assert.True(user.First_name == testFirstName);
            UserRead getUser = await permitClient.Api.GetUser(user.Key);
            Assert.True(getUser.Email == testEmail);

            // test Tenant Api
            //string desc = "desc";
            string desc2 = "desc2";
            string tenantName = "tName";
            string tenantKey = "tKey";
            try
            {
                await permitClient.Api.DeleteTenant(tenantKey);
            } catch (Exception ex){}
            TenantCreate tenantObj = new TenantCreate() { Name = tenantName, Key = tenantKey };
            TenantRead tenant = await permitClient.Api.CreateTenant(tenantObj);
            TenantRead getTenant = await permitClient.Api.GetTenant(tenantKey);
            Assert.True(tenant.Key == getTenant.Key);
            TenantUpdate updateTenant = new TenantUpdate()
            {
                Description = desc2
            };
            TenantRead getTenant2 = await permitClient.Api.UpdateTenant(getTenant.Key, updateTenant);
            Assert.True(getTenant2.Description == desc2);
            Assert.True(getTenant.Key == tenantKey);

            // test roles Api
            string roleName = "rName";
            string roleDesc = "rDesc";
            try
            {
                await permitClient.Api.DeleteRole(roleName);
            }
            catch (Exception ex)
            {
            }

            var roleObj = new RoleCreate() { Name = roleName, Description = roleDesc, Key = roleName };
            var createdRole = await permitClient.Api.CreateRole(roleObj);
            var roles = await permitClient.Api.ListRoles();
            Assert.True(roles.Count > 0);
            RoleRead getRole = await permitClient.Api.GetRole(createdRole.Id.ToString());
            var assignedRole = await permitClient.Api.AssignRole(
                user.Key,
                getRole.Id.ToString(),
                getTenant.Key
            );
            var assignedRoles = await permitClient.Api.GetAssignedRoles(
                getUser.Key
            );
            var assignedRoles2 = await permitClient.Api.GetAssignedRoles(
                getUser.Key,
                getTenant.Key
            );
            // Assert.True(assignedRoles.Count == assignedRoles2.Count);

            await permitClient.Api.UnassignRole(user.Key, getRole.Key, getTenant.Key);
            assignedRoles = await permitClient.Api.GetAssignedRoles(getUser.Key);
            // Assert.True(assignedRoles.Count == (assignedRoles2.Count - 1));

            await permitClient.Api.DeleteUser(getUser.Key);
            try
            {
                getUser = await permitClient.Api.GetUser(user.Key);
            }
            catch (PermitApiException ex)
            {
                Assert.Equal(404, ex.StatusCode);
            }
            await permitClient.Api.DeleteTenant(tenantKey);

            try
            {
                getTenant = await permitClient.Api.GetTenant(getTenant.Key);
            }
            catch (PermitApiException ex)
            {
                Assert.Equal(404, ex.StatusCode);
            }

            // // sync resources
            // string testType = "document";
            // ActionProperties testActionProperties = new ActionProperties(
            //     "Create document",
            //     "Ability to create document"
            // );
            // Dictionary<string, ActionProperties> testActions = new Dictionary<
            //     string,
            //     ActionProperties
            // >
            // {
            //     { "create", testActionProperties }
            // };
        }
    }
}
