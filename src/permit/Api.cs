using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PermitSDK.OpenAPI;
using PermitSDK.OpenAPI.Models;

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
            // if config.ProjectId and config.EnvId are not set, get them from the API
            if (string.IsNullOrEmpty(_config.ProjectId) || string.IsNullOrEmpty(_config.EnvId))
            {
                var scope = _api_client.Get_api_key_scope();
                _projectId = scope.Project_id.Value!.ToString();
                _environmentId = scope.Environment_id.Value!.ToString();
            }
            else
            {
                _projectId = _config.ProjectId;
                _environmentId = _config.EnvId;
            }
        }

        public async Task<UserRead> CreateUser(UserCreate user)
        {
            return await _api_client.Create_userAsync(_projectId, _environmentId, user);
        }

        public async Task<UserRead> GetUser(string userKey)
        {
            return await _api_client.Get_userAsync(_projectId, _environmentId, userKey);
        }

        public async Task<RoleRead> GetRole(string roleKey)
        {
            return await _api_client.Get_roleAsync(_projectId, _environmentId, roleKey);
        }

        public async Task<TenantRead> GetTenant(string tenantKey)
        {
            return await _api_client.Get_tenantAsync(_projectId, _environmentId, tenantKey);
        }

        public async Task<PaginatedResult_RoleAssignmentRead_> GetAssignedRoles(
            string userKey,
            string tenantKey = default,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                _projectId,
                _environmentId,
                new List<string> { userKey },
                new List<string> { tenantKey },
                page: page,
                per_page: perPage
            );
        }

        public async Task<PaginatedResult_TenantRead_> ListTenants(int page = 1, int perPage = 30)
        {
            return await _api_client.List_tenantsAsync(
                _projectId,
                _environmentId,
                page: page,
                per_page: perPage
            );
        }

        public async Task<ResourceRead> GetResource(string resourceKey)
        {
            return await _api_client.Get_resourceAsync(_projectId, _environmentId, resourceKey);
        }

        public async Task<UserRead> SyncUser(UserCreate userCreate)
        {
            try
            {
                var user = await _api_client.Get_userAsync(
                    _projectId,
                    _environmentId,
                    userCreate.Key
                );

                return await _api_client.Update_userAsync(
                    _projectId,
                    _environmentId,
                    user.Id.ToString(),
                    new UserUpdate()
                    {
                        Attributes = user.Attributes,
                        Email = user.Email,
                        First_name = user.First_name,
                        Last_name = user.Last_name,
                    }
                );
            }
            catch (PermitApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    return await _api_client.Create_userAsync(
                        _projectId,
                        _environmentId,
                        userCreate
                    );
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task DeleteUser(string userKey)
        {
            await _api_client.Delete_userAsync(_projectId, _environmentId, userKey);
        }

        public async Task<TenantRead> CreateTenant(TenantCreate tenantCreate)
        {
            return await _api_client.Create_tenantAsync(_projectId, _environmentId, tenantCreate);
        }

        public async Task<TenantRead> UpdateTenant(string tenantKey, TenantUpdate tenantUpdate)
        {
            return await _api_client.Update_tenantAsync(
                _projectId,
                _environmentId,
                tenantKey,
                tenantUpdate
            );
        }

        public async Task DeleteTenant(string tenantKey)
        {
            await _api_client.Delete_tenantAsync(_projectId, _environmentId, tenantKey);
        }

        public async Task<RoleRead> CreateRole(RoleCreate role)
        {
            return await _api_client.Create_roleAsync(_projectId, _environmentId, role);
        }

        public async Task<RoleRead> UpdateRole(string roleKey, RoleUpdate roleUpdate)
        {
            return await _api_client.Update_roleAsync(
                _projectId,
                _environmentId,
                roleKey,
                roleUpdate
            );
        }

        public async Task<RoleAssignmentRead> AssignRole(
            string userKey,
            string roleKey,
            string tenantKey,
            string resourceInstanceId = null,
            string resource_type = null
        )
        {
            string resourceInstanceIdent = null;
            if (
                (resource_type != null && resourceInstanceId == null)
                || (resource_type == null && resourceInstanceId != null)
            )
            {
                throw new ArgumentException(
                    "Both resource_type and resourceInstanceId must be provided"
                );
            }
            else if (resource_type != null && resourceInstanceId != null)
            {
                // create a resource instance ident string variable
                resourceInstanceIdent = string.Concat(resource_type, ":", resourceInstanceId);
            }
            return await _api_client.Assign_roleAsync(
                _projectId,
                _environmentId,
                new RoleAssignmentCreate
                {
                    Role = roleKey,
                    User = userKey,
                    Tenant = tenantKey,
                    Resource_instance = resourceInstanceIdent,
                }
            );
        }

        public async Task UnassignRole(
            string userKey,
            string roleKey,
            string tenantKey,
            string resourceInstanceId = null,
            string resource_type = null
        )
        {
            string resourceInstanceIdent = null;
            if (
                (resource_type != null && resourceInstanceId == null)
                || (resource_type == null && resourceInstanceId != null)
            )
            {
                throw new ArgumentException(
                    "Both resource_type and resourceInstanceId must be provided"
                );
            }
            else if (resource_type != null && resourceInstanceId != null)
            {
                resourceInstanceIdent = string.Concat(resource_type, ":", resourceInstanceId);
            }
            await _api_client.Unassign_roleAsync(
                _projectId,
                _environmentId,
                new RoleAssignmentRemove
                {
                    Role = roleKey,
                    Tenant = tenantKey,
                    User = userKey,
                    Resource_instance = resourceInstanceIdent,
                }
            );
        }

        public async Task DeleteRole(string roleKey)
        {
            await _api_client.Delete_roleAsync(_projectId, _environmentId, roleKey);
        }

        public async Task<ResourceRead> CreateResource(ResourceCreate resourceCreate)
        {
            return await _api_client.Create_resourceAsync(
                _projectId,
                _environmentId,
                resourceCreate
            );
        }

        public async Task<ResourceRead> UpdateResource(
            string resourceKey,
            ResourceUpdate resourceUpdate
        )
        {
            return await _api_client.Update_resourceAsync(
                _projectId,
                _environmentId,
                resourceKey,
                resourceUpdate
            );
        }

        public async Task DeleteResource(string resourceKey)
        {
            await _api_client.Delete_resourceAsync(_projectId, _environmentId, resourceKey);
        }

        public async Task<PaginatedResult_RoleRead_> ListRoles()
        {
            return await _api_client.List_rolesAsync(_projectId, _environmentId);
        }

        public async Task DeleteResourceInstance(string resourceInstanceId)
        {
            await _api_client.Delete_resource_instanceAsync(
                _projectId,
                _environmentId,
                resourceInstanceId
            );
        }

        // create resource instance
        public async Task<ResourceInstanceRead> CreateResourceInstance(
            ResourceInstanceCreate resourceInstance
        )
        {
            return await _api_client.Create_resource_instanceAsync(
                _projectId,
                _environmentId,
                resourceInstance
            );
        }

        public async Task<ResourceInstanceRead> GetResourceInstance(string resourceInstanceId)
        {
            return await _api_client.Get_resource_instanceAsync(
                _projectId,
                _environmentId,
                resourceInstanceId
            );
        }

        public async Task<ResourceInstanceRead> UpdateResourceInstance(
            string resourceInstanceId,
            ResourceInstanceUpdate resourceInstanceUpdate
        )
        {
            return await _api_client.Update_resource_instanceAsync(
                _projectId,
                _environmentId,
                resourceInstanceId,
                resourceInstanceUpdate
            );
        }

        // resource roles api
        public async Task<ResourceRoleRead> CreateResourceRole(
            string resourceId,
            ResourceRoleCreate resourceRole
        )
        {
            return await _api_client.Create_resource_roleAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceRole
            );
        }

        public async Task<ResourceRoleRead> GetResourceRole(
            string resourceId,
            string resourceRoleId
        )
        {
            return await _api_client.Get_resource_roleAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceRoleId
            );
        }

        public async Task<ResourceRoleRead> UpdateResourceRole(
            string resourceId,
            string resourceRoleId,
            ResourceRoleUpdate resourceRoleUpdate
        )
        {
            return await _api_client.Update_resource_roleAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceRoleId,
                resourceRoleUpdate
            );
        }

        public async Task DeleteResourceRole(string resourceId, string resourceRoleId)
        {
            await _api_client.Delete_resource_roleAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceRoleId
            );
        }

        // resource actions api
        public async Task<ResourceActionRead> CreateResourceAction(
            string resourceId,
            ResourceActionCreate resourceAction
        )
        {
            return await _api_client.Create_resource_actionAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceAction
            );
        }

        public async Task<ResourceActionRead> GetResourceAction(
            string resourceId,
            string resourceActionId
        )
        {
            return await _api_client.Get_resource_actionAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceActionId
            );
        }

        public async Task<ResourceActionRead> UpdateResourceAction(
            string resourceId,
            string resourceActionId,
            ResourceActionUpdate resourceActionUpdate
        )
        {
            return await _api_client.Update_resource_actionAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceActionId,
                resourceActionUpdate
            );
        }

        public async Task DeleteResourceAction(string resourceId, string resourceActionId)
        {
            await _api_client.Delete_resource_actionAsync(
                _projectId,
                _environmentId,
                resourceId,
                resourceActionId
            );
        }

        public async Task<PaginatedResult_RoleAssignmentRead_> ListAssignedRoles(
            string userId,
            string tenantId = null
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                _projectId,
                _environmentId,
                new List<string> { userId }
            );
        }
    }
}
