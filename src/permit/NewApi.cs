using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PermitSDK.NewAPI;


namespace PermitSDK
{
    public class NewApiConfig
    {
        public static readonly string DefaultApiUrl = "http://api.permit.io";
        public static readonly string DefaultPdpUrl = "http://localhost:7000";
        public string ApiURL { get; set; }
        public string PdpURL { get; set; }
        public bool DebugMode { get; set; }
        public string Token { get; set; }
    }

    public class APINew
    {
        private NewApiConfig _config;
        private NewAPI.PermitClient _client; 

        private string _projId;
        private string _envId;

        public APINew(NewApiConfig config)
        {
            _config = config;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.Token);
            _client = new NewAPI.PermitClient(_config.ApiURL, httpClient);

            var apiKeyScope = _client.Get_api_key_scopeAsync().Result;
            _projId = apiKeyScope.Project_id.ToString();
            _envId = apiKeyScope.Environment_id.ToString();
        }

        public APINew() : this(new NewApiConfig
            { ApiURL = NewApiConfig.DefaultApiUrl, PdpURL = NewApiConfig.DefaultPdpUrl, DebugMode = false })
        {

        }

        public async Task<UserRead> GetUser(string userKey)
        {
            return await _client.Get_userAsync(_projId, _envId, userKey);
        }

        public async Task<RoleRead> GetRole(string roleKey)
        {
            return await _client.Get_roleAsync(_projId, _envId, roleKey);
        }

        public async Task<TenantRead> GetTenant(string tenantKey)
        {
            return await _client.Get_tenantAsync(_projId, _envId, tenantKey);
        }

        public async Task<ICollection<RoleAssignmentRead>> GetAssignedRoles(string userKey, string tenantKey = default, int page = default, int perPage = default)
        {
            return await _client.List_role_assignmentsAsync(_projId, _envId, userKey, tenantKey, page: page, per_page: perPage);
        }
        
        public async Task<ICollection<TenantRead>> ListTenants(int page = default, int perPage = default)
        {
            return await _client.List_tenantsAsync(_projId, _envId, page: page, per_page: perPage);
        }

        public async Task<ResourceRead> GetResource(string resourceKey)
        {
            return await _client.Get_resourceAsync(_projId, _envId, resourceKey);
        }

        public async Task<UserRead> SyncUser(UserCreate userCreate)
        {
            try
            {
                var user = await _client.Get_userAsync(_projId, _envId, userCreate.Key);

                return await _client.Update_userAsync(_projId, _envId, user.Id.ToString(),
                    new UserUpdate()
                    {
                        Attributes = user.Attributes, Email = user.Email, First_name = user.First_name,
                        Last_name = user.Last_name
                    });
            }
            catch (PermitApiException ex)
            {
                if (ex.StatusCode == 404)
                {
                    return await _client.Create_userAsync(_projId, _envId, userCreate);
                }
                else
                {
                    throw;
                }
            }
        }

        public async void DeleteUser(string userKey)
        {
            await _client.Delete_userAsync(_projId, _envId, userKey);
        }

        public async Task<TenantRead> CreateTenant(TenantCreate tenantCreate)
        {
            return await _client.Create_tenantAsync(_projId, _envId, tenantCreate);
        }

        public async Task<TenantRead> UpdateTenant(string tenantKey, TenantUpdate tenantUpdate)
        {
            return await _client.Update_tenantAsync(_projId, _envId, tenantKey, tenantUpdate);
        }

        public async void DeleteTenant(string tenantKey)
        {
            await _client.Delete_tenantAsync(_projId, _envId, tenantKey);
        }

        public async Task<RoleRead> CreateRole(RoleCreate role)
        {
            return await _client.Create_roleAsync(_projId, _envId, role);
        }

        public async Task<RoleRead> UpdateRole(string roleKey, RoleUpdate roleUpdate)
        {
            return await _client.Update_roleAsync(_projId, _envId, roleKey, roleUpdate);
        }

        public async Task<RoleAssignmentRead> AssignRole(string userKey, string roleKey, string tenantKey)
        {
            return await _client.Assign_roleAsync(_projId, _envId,
                new RoleAssignmentCreate { Role = roleKey, User = userKey, Tenant = tenantKey });
        }

        public async void UnassignRole(string userKey, string roleKey, string tenantKey)
        {
            await _client.Unassign_roleAsync(_projId, _envId, new RoleAssignmentRemove
            {
                Role = roleKey, Tenant = tenantKey, User = userKey
            });
        }

        public async void DeleteRole(string roleKey)
        {
            await _client.Delete_roleAsync(_projId, _envId, roleKey);
        }

        public async Task<ResourceRead> CreateResource(ResourceCreate resourceCreate)
        {
            return await _client.Create_resourceAsync(_projId, _envId, resourceCreate);
        }

        public async Task<ResourceRead> UpdateResource(string resourceKey, ResourceUpdate resourceUpdate)
        {
            return await _client.Update_resourceAsync(_projId, _envId, resourceKey, resourceUpdate);
        }

        public async void DeleteResource(string resourceKey)
        {
            await _client.Delete_resourceAsync(_projId, _envId, resourceKey);
        }
    }
}