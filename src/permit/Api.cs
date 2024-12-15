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

        /// <summary>
        /// Create User
        /// </summary>
        /// <remarks>
        /// Creates a new user inside the Permit.io system, from that point forward
        /// <br/>you may run permission checks on that user.
        /// <br/>
        /// <br/>Returns 201 if the user is created, 409 if the user already exists.
        /// <br/>User is identified by its key, and you can only create one user with the same key inside a Permit environment.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<UserRead> CreateUser(UserCreate user)
        {
            return await _api_client.Create_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: user
            );
        }

        /// <summary>
        /// Get User
        /// </summary>
        /// <remarks>
        /// Gets a user, if such user exists.
        /// </remarks>
        /// <param name="userKey">Either the unique id of the user, or the URL-friendly key of the user (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">The user is not found or an error has occurred</exception>
        public async Task<UserRead> GetUser(string userKey)
        {
            return await _api_client.Get_userAsync(
                user_id: userKey,
                env_id: _environmentId,
                proj_id: _projectId
            );
        }

        /// <summary>
        /// Get Role
        /// </summary>
        /// <remarks>
        /// Gets a single tenant role, if such role exists.
        /// </remarks>
        /// <param name="roleKey">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<RoleRead> GetRole(string roleKey)
        {
            return await _api_client.Get_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey
            );
        }

        /// <summary>
        /// Get Tenant
        /// </summary>
        /// <remarks>
        /// Gets a tenant, if such tenant exists.
        /// </remarks>
        /// <param name="tenantKey">Either the unique id of the tenant, or the URL-friendly key of the tenant (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<TenantRead> GetTenant(string tenantKey)
        {
            return await _api_client.Get_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey
            );
        }

        /// <summary>
        /// List Role Assignments
        /// </summary>
        /// <remarks>
        /// Lists the role assignments defined within an environment.
        /// <br/>
        /// <br/>- If the `user` filter is present, will only return the role assignments of that user (supports multiple).
        /// <br/>- If the `tenant` filter is present, will only return the role assignments in that tenant (supports multiple).
        /// <br/>- If the `role` filter is present, will only return role assignments that are granting that role (supports multiple).
        /// <br/>- If the `resource` filter is present, will only return role assignments for resource instances of that resource type.
        /// <br/>- If the `resource_instance` filter is present, will only return role assignments for that resource instance.
        /// <br/>
        /// <br/>Providing both `tenant` and `resource_instance` filters will only return role assignments if the resource instance is in that tenant.
        /// <br/>If multiple tenants are received, the last tenant will be compared with the resource instance.
        /// </remarks>
        /// <param name="userKey">optional user filter, will only return role assignments granted to this user.</param>
        /// <param name="roleKey">optional role filter, will only return role assignments granting this role.</param>
        /// <param name="tenantKey">optional tenant filter, will only return role assignments granted in that tenant.</param>
        /// <param name="resourceKey">optional resource **type** filter, will only return role assignments granted on that resource type.</param>
        /// <param name="instanceKey">optional resource instance filter, will only return role assignments granted on that resource instance.</param>
        /// <param name="page">Page number of the results to fetch, starting at 1.</param>
        /// <param name="perPage">The number of results per page (max 100).</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<PaginatedResult_RoleAssignmentRead_> GetAssignedRoles(
            string userKey,
            string tenantKey = default,
            string roleKey = default,
            string resourceKey = default,
            string instanceKey = default,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user: new List<string> { userKey },
                tenant: tenantKey != null ? new List<string> { tenantKey } : null,
                role: roleKey != null ? new List<string> { roleKey } : null,
                resource_instance: instanceKey,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        /// <summary>
        /// List Tenants
        /// </summary>
        /// <remarks>
        /// Lists all the tenants defined within an env.
        /// </remarks>
        /// <param name="search">Text search for the tenant name or key</param>
        /// <param name="page">Page number of the results to fetch, starting at 1.</param>
        /// <param name="perPage">The number of results per page (max 100).</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<PaginatedResult_TenantRead_> ListTenants(
            string search = default,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_tenantsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                search: search,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        /// <summary>
        /// Get Resource
        /// </summary>
        /// <remarks>
        /// Gets a single resource, if such resource exists.
        /// </remarks>
        /// <param name="resourceKey">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<ResourceRead> GetResource(string resourceKey)
        {
            return await _api_client.Get_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceKey
            );
        }

        /// <summary>
        /// Replace User
        /// </summary>
        /// <remarks>
        /// Replace User / Sync User upsert a user in the system.
        /// <br/>If the user already exists, it will update the user with the new data.
        /// <br/>If the user does not exist, it will create a new user with the provided data.
        /// <br/>
        /// <br/>The user is identified by its key, and you can only create one user with the same key inside a Permit environment.
        /// <br/>A 200 status code will be returned if the user already exists, and a 201 status code will be returned if the user is created.
        /// </remarks>
        /// <param name="userCreate">The user to create or update</param>
        /// <returns>An existing user was replaced</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<UserRead> SyncUser(UserCreate userCreate)
        {
            return await _api_client.Replace_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user_id: userCreate.Key,
                body: userCreate
            );
        }

        /// <summary>
        /// Delete User
        /// </summary>
        /// <remarks>
        /// Deletes the user and all its related data.
        /// </remarks>
        /// <param name="userKey">Either the unique id of the user, or the URL-friendly key of the user (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteUser(string userKey)
        {
            await _api_client.Delete_userAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user_id: userKey
            );
        }

        /// <summary>
        /// Create Tenant
        /// </summary>
        /// <remarks>
        /// Creates a new tenant inside the Permit.io system.
        /// <br/>
        /// <br/>If the tenant is already created: will return 200 instead of 201,
        /// <br/>and will return the existing tenant object in the response body.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<TenantRead> CreateTenant(TenantCreate tenantCreate)
        {
            return await _api_client.Create_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: tenantCreate
            );
        }

        /// <summary>
        /// Update Tenant
        /// </summary>
        /// <remarks>
        /// Partially updates the tenant definition.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="tenantKey">Either the unique id of the tenant, or the URL-friendly key of the tenant (i.e: the "slug").</param>
        /// <param name="tenantUpdate">The tenant to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<TenantRead> UpdateTenant(string tenantKey, TenantUpdate tenantUpdate)
        {
            return await _api_client.Update_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey,
                body: tenantUpdate
            );
        }

        /// <summary>
        /// Delete Tenant
        /// </summary>
        /// <remarks>
        /// Deletes the tenant and all its related data.
        /// </remarks>
        /// <param name="tenantKey">Either the unique id of the tenant, or the URL-friendly key of the tenant (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteTenant(string tenantKey)
        {
            await _api_client.Delete_tenantAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant_id: tenantKey
            );
        }

        /// <summary>
        /// Create Role
        /// </summary>
        /// <remarks>
        /// Creates a new tenant role.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<RoleRead> CreateRole(RoleCreate role)
        {
            return await _api_client.Create_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: role
            );
        }

        /// <summary>
        /// Update Role
        /// </summary>
        /// <remarks>
        /// Partially updates a tenant role.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="roleKey">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <param name="roleUpdate">The role to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<RoleRead> UpdateRole(string roleKey, RoleUpdate roleUpdate)
        {
            return await _api_client.Update_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey,
                body: roleUpdate
            );
        }

        /// <summary>
        /// Assign Role
        /// </summary>
        /// <remarks>
        /// Assigns a role to a user within a tenant.
        /// <br/>
        /// <br/>The tenant defines the scope of the assignment. In other words, the role is effective only within the tenant.
        /// </remarks>
        /// <param name="userKey">The user to assign the role to.</param>
        /// <param name="roleKey">The role to assign.</param>
        /// <param name="tenantKey">The tenant where the role is assigned.</param>
        /// <param name="resourceInstanceId">The resource instance where the role is assigned.</param>
        /// <param name="resource_type">The resource type where the role is assigned.</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Unassign Role
        /// </summary>
        /// <remarks>
        /// Unassigns a user role within a tenant.
        /// <br/>
        /// <br/>The tenant defines the scope of the assignment. In other words, the role is effective only within the tenant.
        /// <br/>
        /// <br/>If the role is not actually assigned, will return 404.
        /// </remarks>
        /// <param name="userKey">The user to assign the role to.</param>
        /// <param name="roleKey">The role to assign.</param>
        /// <param name="tenantKey">The tenant where the role is assigned.</param>
        /// <param name="resourceInstanceId">The resource instance where the role is assigned.</param>
        /// <param name="resource_type">The resource type where the role is assigned.</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Role
        /// </summary>
        /// <remarks>
        /// Deletes a tenant role and all its related data.
        /// <br/>This includes any permissions granted to said role.
        /// </remarks>
        /// <param name="roleKey">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteRole(string roleKey)
        {
            await _api_client.Delete_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                role_id: roleKey
            );
        }

        /// <summary>
        /// Create Resource
        /// </summary>
        /// <remarks>
        /// Creates a new resource (a type of object you may protect with permissions).
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<ResourceRead> CreateResource(ResourceCreate resourceCreate)
        {
            return await _api_client.Create_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: resourceCreate
            );
        }

        /// <summary>
        /// Update Resource
        /// </summary>
        /// <remarks>
        /// Partially updates the resource definition.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="resourceKey">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceUpdate">The resource to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Resource
        /// </summary>
        /// <remarks>
        /// Deletes the resource and all its related data.
        /// </remarks>
        /// <param name="resourceKey">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteResource(string resourceKey)
        {
            await _api_client.Delete_resourceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceKey
            );
        }

        /// <summary>
        /// List Roles
        /// </summary>
        /// <remarks>
        /// Lists all tenant roles.
        /// </remarks>
        /// <param name="search">Text search for the object name or key</param>
        /// <param name="page">Page number of the results to fetch, starting at 1.</param>
        /// <param name="perPage">The number of results per page (max 100).</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<PaginatedResult_RoleRead_> ListRoles(
            string search = default,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_rolesAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                search: search,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }

        /// <summary>
        /// List Resource Instances
        /// </summary>
        /// <remarks>
        /// Lists all the resource instances defined within an environment.
        /// </remarks>
        /// <param name="resourceId">The resource key or id to filter by</param>
        /// <param name="page">Page number of the results to fetch, starting at 1.</param>
        /// <param name="perPage">The number of results per page (max 100).</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Resource Instance
        /// </summary>
        /// <remarks>
        /// Deletes the instance and all its related data.
        /// </remarks>
        /// <param name="resourceInstanceId">f'Either the unique id of the resource instance, or the URL-friendly key of the &lt;resource_key:resource_instance_key&gt; (i.e: file:my_file.txt).'</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteResourceInstance(string resourceInstanceId)
        {
            await _api_client.Delete_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                instance_id: resourceInstanceId
            );
        }

        /// <summary>
        /// Create Resource Instance
        /// </summary>
        /// <remarks>
        /// Creates a new instance inside the Permit.io system.
        /// <br/>
        /// <br/>If the instance is already created: will return 200 instead of 201,
        /// <br/>and will return the existing instance object in the response body.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Get Resource Instance
        /// </summary>
        /// <remarks>
        /// Gets a instance, if such instance exists.
        /// </remarks>
        /// <param name="resourceInstanceId">f'Either the unique id of the resource instance, or the URL-friendly key of the &lt;resource_key:resource_instance_key&gt; (i.e: file:my_file.txt).'</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<ResourceInstanceRead> GetResourceInstance(string resourceInstanceId)
        {
            return await _api_client.Get_resource_instanceAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                instance_id: resourceInstanceId
            );
        }

        /// <summary>
        /// Update Resource Instance
        /// </summary>
        /// <remarks>
        /// Partially updates the instance definition.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="resourceInstanceId">f'Either the unique id of the resource instance, or the URL-friendly key of the &lt;resource_key:resource_instance_key&gt; (i.e: file:my_file.txt).'</param>
        /// <param name="resourceInstanceUpdate">The instance to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Create Resource Role
        /// </summary>
        /// <remarks>
        /// Creates a new role associated with the resource.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceRole">The role to create</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Get Resource Role
        /// </summary>
        /// <remarks>
        /// Gets a single role defined on the resource, if such role exists.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceRoleId">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Update Resource Role
        /// </summary>
        /// <remarks>
        /// Partially updates the role defined on a resource.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceRoleId">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <param name="resourceRoleUpdate">The role to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Resource Role
        /// </summary>
        /// <remarks>
        /// Deletes the role and all its related data.
        /// <br/>This includes any permissions granted to said role.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceRoleId">Either the unique id of the role, or the URL-friendly key of the role (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteResourceRole(string resourceId, string resourceRoleId)
        {
            await _api_client.Delete_resource_roleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                role_id: resourceRoleId
            );
        }

        /// <summary>
        /// Create Resource Action
        /// </summary>
        /// <remarks>
        /// Creates a new action that can affect the resource.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceAction">The action to create</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Get Resource Action
        /// </summary>
        /// <remarks>
        /// Gets a single action defined on the resource, if such action exists.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceActionId">Either the unique id of the action, or the URL-friendly key of the action (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Update Resource Action
        /// </summary>
        /// <remarks>
        /// Partially updates the action defined on a resource.
        /// <br/>Fields that will be provided will be completely overwritten.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceActionId">Either the unique id of the action, or the URL-friendly key of the action (i.e: the "slug").</param>
        /// <param name="resourceActionUpdate">The action to update</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Resource Action
        /// </summary>
        /// <remarks>
        /// Deletes the action and all its related data.
        /// <br/>This includes any permissions granted to perform the action.
        /// </remarks>
        /// <param name="resourceId">Either the unique id of the resource, or the URL-friendly key of the resource (i.e: the "slug").</param>
        /// <param name="resourceActionId">Either the unique id of the action, or the URL-friendly key of the action (i.e: the "slug").</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteResourceAction(string resourceId, string resourceActionId)
        {
            await _api_client.Delete_resource_actionAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                resource_id: resourceId,
                action_id: resourceActionId
            );
        }

        /// <summary>
        /// List Role Assignments
        /// </summary>
        /// <remarks>
        /// Lists the role assignments defined within an environment.
        /// <br/>
        /// <br/>- If the `user` filter is present, will only return the role assignments of that user (supports multiple).
        /// <br/>- If the `tenant` filter is present, will only return the role assignments in that tenant (supports multiple).
        /// <br/>- If the `role` filter is present, will only return role assignments that are granting that role (supports multiple).
        /// <br/>- If the `resource` filter is present, will only return role assignments for resource instances of that resource type.
        /// <br/>- If the `resource_instance` filter is present, will only return role assignments for that resource instance.
        /// <br/>
        /// <br/>Providing both `tenant` and `resource_instance` filters will only return role assignments if the resource instance is in that tenant.
        /// <br/>If multiple tenants are received, the last tenant will be compared with the resource instance.
        /// </remarks>
        /// <param name="userId">optional user filter, will only return role assignments granted to this user.</param>
        /// <param name="roleId">optional role filter, will only return role assignments granting this role.</param>
        /// <param name="tenantId">optional tenant filter, will only return role assignments granted in that tenant.</param>
        /// <param name="resourceId">optional resource **type** filter, will only return role assignments granted on that resource type.</param>
        /// <param name="instanceId">optional resource instance filter, will only return role assignments granted on that resource instance.</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<PaginatedResult_RoleAssignmentRead_> ListAssignedRoles(
            string userId,
            string roleId = null,
            string tenantId = null,
            string resourceId = null,
            string instanceId = null
        )
        {
            return await _api_client.List_role_assignmentsAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                user: new List<string> { userId },
                include_total_count: true
            );
        }

        /// <summary>
        /// Create Relationship Tuple
        /// </summary>
        /// <remarks>
        /// Create a relationship between two resource instances using a relation.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
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

        /// <summary>
        /// Delete Relationship Tuple
        /// </summary>
        /// <remarks>
        /// Delete a relationship between two resource instances.
        /// </remarks>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task DeleteRelationshipTuple(RelationshipTupleDelete relationshipTuple)
        {
            await _api_client.Delete_relationship_tupleAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                body: relationshipTuple
            );
        }

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>
        /// List Relationship Tuples
        /// </summary>
        /// <remarks>
        /// Lists the relationship tuples defined within an environment.
        /// </remarks>
        /// <param name="page">Page number of the results to fetch, starting at 1.</param>
        /// <param name="perPage">The number of results per page (max 100).</param>
        /// <param name="tenantId">The tenant key or id to filter by</param>
        /// <param name="subjectId">The subject to filter by, accepts either the resource instance id or resource_type:resource_instance</param>
        /// <param name="relationId">The relation id or key to filter by</param>
        /// <param name="objectId">The object to filter by, accepts either the resource instance id or resource_type:resource_instance</param>
        /// <param name="objectTypeId">The object type to filter by, accepts resource type id or key</param>
        /// <param name="subjectTypeId">The subject type to filter by, accepts resource type id or key</param>
        /// <returns>Successful Response</returns>
        /// <exception cref="PermitApiException">A server side error occurred.</exception>
        public async Task<PaginatedResult_RelationshipTupleRead_> ListRelationshipTuples(
            string tenantId = null,
            string subjectId = null,
            string relationId = null,
            string objectId = null,
            string objectTypeId = null,
            string subjectTypeId = null,
            int page = 1,
            int perPage = 30
        )
        {
            return await _api_client.List_relationship_tuplesAsync(
                proj_id: _projectId,
                env_id: _environmentId,
                tenant: tenantId,
                subject: subjectId,
                relation: relationId,
                @object: objectId,
                object_type: objectTypeId,
                subject_type: subjectTypeId,
                page: page,
                per_page: perPage,
                include_total_count: true
            );
        }
    }
}
