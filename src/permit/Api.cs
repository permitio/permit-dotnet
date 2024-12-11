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

            _api_client = new OpenAPI.PermitClient(baseUrl: _config.ApiUrl, httpClient: _client);
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
            return await _api_client.Create_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: user
            );
        }

        public async Task<UserRead> GetUser(string userKey)
        {
            return await _api_client.Get_userAsync(
                user_id: userKey,
                env_id: _environmentId,
                proj_id: _projectId
            );
        }

        public async Task<RoleRead> GetRole(string roleKey)
        {
            return await _api_client.Get_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey
            );
        }

        public async Task<TenantRead> GetTenant(string tenantKey)
        {
            return await _api_client.Get_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey
            );
        }

        public async Task<PaginatedResult_RoleAssignmentRead_> GetAssignedRoles(
            string userKey,
            string tenantKey = default,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user: new List<string> { userKey },
                tenant: tenantKey != null ? new List<string> { tenantKey } : null,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        public async Task<PaginatedResult_TenantRead_> ListTenants(int page = 1, int perPage = 30)
        {
            return await _api_client.List_tenantsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        public async Task<ResourceRead> GetResource(string resourceKey)
        {
            return await _api_client.Get_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceKey
            );
        }

        public async Task<UserRead> SyncUser(UserCreate userCreate)
        {
            return await _api_client.Replace_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user_id: userCreate.Key,
                body: userCreate
            );
        }

        public async Task DeleteUser(string userKey)
        {
            await _api_client.Delete_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user_id: userKey
            );
        }

        public async Task<TenantRead> CreateTenant(TenantCreate tenantCreate)
        {
            return await _api_client.Create_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: tenantCreate
            );
        }

        public async Task<TenantRead> UpdateTenant(string tenantKey, TenantUpdate tenantUpdate)
        {
            return await _api_client.Update_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey,
                body: tenantUpdate
            );
        }

        public async Task DeleteTenant(string tenantKey)
        {
            await _api_client.Delete_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey
            );
        }

        public async Task<RoleRead> CreateRole(RoleCreate role)
        {
            return await _api_client.Create_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: role
            );
        }

        public async Task<RoleRead> UpdateRole(string roleKey, RoleUpdate roleUpdate)
        {
            return await _api_client.Update_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey,
                body: roleUpdate
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
                resourceInstanceIdent = string.Concat(resource_type, ":", resourceInstanceId);
            }

            return await _api_client.Assign_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: new RoleAssignmentCreate
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
                proj_id: _projectId,
                env_id: _environmentId,
                body: new RoleAssignmentRemove
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
            await _api_client.Delete_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey
            );
        }

        public async Task<ResourceRead> CreateResource(ResourceCreate resourceCreate)
        {
            return await _api_client.Create_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: resourceCreate
            );
        }

        public async Task<ResourceRead> UpdateResource(
            string resourceKey,
            ResourceUpdate resourceUpdate
        )
        {
            return await _api_client.Update_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceKey,
                body: resourceUpdate
            );
        }

        public async Task DeleteResource(string resourceKey)
        {
            await _api_client.Delete_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceKey
            );
        }

        public async Task<PaginatedResult_RoleRead_> ListRoles()
        {
            return await _api_client.List_rolesAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                include_total_count: true
            );
        }

        public async Task<PaginatedResult_ResourceInstanceRead_> ListResourceInstances(
            string resourceId,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_resource_instancesAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource: resourceId,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        public async Task DeleteResourceInstance(string resourceInstanceId)
        {
            await _api_client.Delete_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                instance_id: resourceInstanceId
            );
        }

        public async Task<ResourceInstanceRead> CreateResourceInstance(
            ResourceInstanceCreate resourceInstance
        )
        {
            return await _api_client.Create_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: resourceInstance
            );
        }

        public async Task<ResourceInstanceRead> GetResourceInstance(string resourceInstanceId)
        {
            return await _api_client.Get_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                instance_id: resourceInstanceId
            );
        }

        public async Task<ResourceInstanceRead> UpdateResourceInstance(
            string resourceInstanceId,
            ResourceInstanceUpdate resourceInstanceUpdate
        )
        {
            return await _api_client.Update_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                instance_id: resourceInstanceId,
                body: resourceInstanceUpdate
            );
        }

        public async Task<ResourceRoleRead> CreateResourceRole(
            string resourceId,
            ResourceRoleCreate resourceRole
        )
        {
            return await _api_client.Create_resource_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                body: resourceRole
            );
        }

        public async Task<ResourceRoleRead> GetResourceRole(
            string resourceId,
            string resourceRoleId
        )
        {
            return await _api_client.Get_resource_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                role_id: resourceRoleId
            );
        }

        public async Task<ResourceRoleRead> UpdateResourceRole(
            string resourceId,
            string resourceRoleId,
            ResourceRoleUpdate resourceRoleUpdate
        )
        {
            return await _api_client.Update_resource_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                role_id: resourceRoleId,
                body: resourceRoleUpdate
            );
        }

        public async Task DeleteResourceRole(string resourceId, string resourceRoleId)
        {
            await _api_client.Delete_resource_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                role_id: resourceRoleId
            );
        }

        public async Task<ResourceActionRead> CreateResourceAction(
            string resourceId,
            ResourceActionCreate resourceAction
        )
        {
            return await _api_client.Create_resource_actionAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                body: resourceAction
            );
        }

        public async Task<ResourceActionRead> GetResourceAction(
            string resourceId,
            string resourceActionId
        )
        {
            return await _api_client.Get_resource_actionAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                action_id: resourceActionId
            );
        }

        public async Task<ResourceActionRead> UpdateResourceAction(
            string resourceId,
            string resourceActionId,
            ResourceActionUpdate resourceActionUpdate
        )
        {
            return await _api_client.Update_resource_actionAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                action_id: resourceActionId,
                body: resourceActionUpdate
            );
        }

        public async Task DeleteResourceAction(string resourceId, string resourceActionId)
        {
            await _api_client.Delete_resource_actionAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                action_id: resourceActionId
            );
        }

        public async Task<PaginatedResult_RoleAssignmentRead_> ListAssignedRoles(
            string userId,
            string tenantId = null
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user: new List<string> { userId },
                include_total_count: true
            );
        }

        public async Task<RelationshipTupleRead> CreateRelationshipTuple(
            RelationshipTupleCreate relationshipTuple
        )
        {
            return await _api_client.Create_relationship_tupleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: relationshipTuple
            );
        }

        public async Task DeleteRelationshipTuple(RelationshipTupleDelete relationshipTuple)
        {
            await _api_client.Delete_relationship_tupleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: relationshipTuple
            );
        }

        public async Task<PaginatedResult_RelationshipTupleRead_> ListRelationshipTuples(
            string tenant = null,
            string subject = null,
            string relation = null,
            string @object = null,
            string objectType = null,
            string subjectType = null,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_relationship_tuplesAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant: tenant,
                subject: subject,
                relation: relation,
                @object: @object,
                object_type: objectType,
                subject_type: subjectType,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }
    }
}
