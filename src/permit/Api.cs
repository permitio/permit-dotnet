using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PermitSDK.Models;
using PermitSDK.OpenAPI;

namespace PermitSDK
{
    public class Api
    {
        readonly Config _config;
        readonly HttpClient _client = new HttpClient();
        readonly ILogger _logger;
        private JsonSerializerOptions _options;
        private string _projectId;
        private string _environmentId;
        private PermitClient _api_client;

        public Api(Config config, ILogger logger = null)
        {
            _config = config;
            _client.BaseAddress = new Uri(config.ApiUrl);
            _client.DefaultRequestHeaders.Add(
                "Authorization",
                string.Format("Bearer {0}", config.Token)
            );
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            _options = new JsonSerializerOptions();
            _options.IgnoreNullValues = true;
            _logger = logger;

            _api_client = new OpenAPI.PermitClient(_config.ApiUrl, _client);
            var scope = _api_client.Get_api_key_scope();
            _projectId = scope.Project_id.Value!.ToString();
            _environmentId = scope.Environment_id.Value!.ToString();
        }
        
        public async Task<UserRead> CreateUser(UserCreate user)
        {
            return await _api_client.Create_userAsync(_projectId, _environmentId, user);
        }
        
        public async Task<UserRead> SyncUser(string userId, UserCreate user)
        {
            return await _api_client.Replace_userAsync(_projectId, _environmentId, userId, user);
        }
        
        public async Task<UserRead> GetUser(string userId)
        {
            return await _api_client.Get_userAsync(_projectId, _environmentId, userId);
        }

        public async Task DeleteUser(string userId)
        {
            await _api_client.Delete_userAsync(_projectId, _environmentId, userId);
        }

        public async Task DeleteTenant(string tenantId)
        {
            await _api_client.Delete_tenantAsync(_projectId, _environmentId, tenantId);
        }

        public async Task<ICollection<TenantRead>> ListTenants()
        {
            return await _api_client.List_tenantsAsync(_projectId, _environmentId);
        }

        public async Task<TenantRead> GetTenant(string tenantId)
        {
            return await _api_client.Get_tenantAsync(_projectId, _environmentId, tenantId);
        }

        public async Task<TenantRead> CreateTenant(TenantCreate tenant)
        {
            return await _api_client.Create_tenantAsync(_projectId, _environmentId, tenant);
        }

        public async Task<TenantRead> UpdateTenant(string tenantId, TenantUpdate tenant)
        {
            return await _api_client.Update_tenantAsync(_projectId, _environmentId, tenantId, tenant);
        }

        public async Task<ResourceRead> CreateResource(ResourceCreate resource)
        {
            ResourceRead resourceObj = await _api_client.Create_resourceAsync(_projectId, _environmentId, resource);
            return resourceObj;
        }

        public async Task<ResourceRead> GetResource(string resourceId)
        {
            return await _api_client.Get_resourceAsync(_projectId, _environmentId, resourceId);
        }

        public async Task DeleteResource(string resourceId)
        {
            await _api_client.Delete_resourceAsync(_projectId, _environmentId, resourceId);
        }

        public async Task DeleteResourceInstance(string resourceInstanceId)
        {
            await _api_client.Delete_resource_instanceAsync(_projectId, _environmentId, resourceInstanceId);
        }
        // create resource instance
        public async Task<ResourceInstanceRead> CreateResourceInstance(ResourceInstanceCreate resourceInstance)
        {
            return await _api_client.Create_resource_instanceAsync(_projectId, _environmentId, resourceInstance);
        }

        public async Task<ResourceInstanceRead> GetResourceInstance(string resourceInstanceId)
        {
            return await _api_client.Get_resource_instanceAsync(_projectId, _environmentId, resourceInstanceId);
        }

        public async Task<RoleRead> CreateRole(RoleCreate role)
        {
            return await _api_client.Create_roleAsync(_projectId, _environmentId, role);
        }

        public async Task<RoleRead> GetRole(string roleId)
        {
            return await _api_client.Get_roleAsync(_projectId, _environmentId, roleId);
        }

        public async Task<ICollection<RoleRead>> ListRoles()
        {
            return await _api_client.List_rolesAsync(_projectId, _environmentId);
        }

        public async Task<RoleAssignmentRead> AssignRole(
            string userId,
            string roleId,
            string tenantId
        )
        {

            return await _api_client.Assign_role_to_userAsync(_projectId, _environmentId, userId,
                new UserRoleCreate
                {
                    Role = roleId, Tenant = tenantId
                });
        }

        public async Task<ICollection<RoleAssignmentRead>> ListAssignedRoles(string userId, string tenantId = null)
        {
            return await _api_client.List_role_assignmentsAsync(_projectId, _environmentId, userId);
        }

        public async Task UnassignRole(string userId, string roleId, string tenantId)
        {
            await _api_client.Unassign_role_from_userAsync(_projectId, _environmentId, userId, new UserRoleRemove
            {
                Role = roleId, Tenant = tenantId
            });
        }
    }
}
