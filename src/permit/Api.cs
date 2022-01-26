using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PermitSDK.Models;

namespace PermitSDK
{
    public class Api
    {
        string Url;
        Config Config;
        HttpClient Client = new HttpClient();
        public JsonSerializerOptions options { get; private set; }

        public Api(Config config, string remotePermitUrl)
        {
            this.Url = remotePermitUrl;
            this.Config = config;
            Client.BaseAddress = new Uri(remotePermitUrl);
            Client.DefaultRequestHeaders.Add(
                "Authorization",
                string.Format("Bearer {0}", config.Token)
            );
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );

            this.options = new JsonSerializerOptions();
            this.options.IgnoreNullValues = true;
        }

        public async Task<T> CloudRequest<T>(string uri)
        {
            try
            {
                var response = await Client.GetAsync(uri).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Sending {0} request to cloud service", uri));
                    }
                    return JsonSerializer.Deserialize<T>(responseContent);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(T);
            }
        }

        public async Task<T> CloudRequestList<T>(string uri)
        {
            try
            {
                var response = await Client.GetAsync(uri).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Sending {0} request to cloud service", uri));
                    }
                    return JsonSerializer.Deserialize<ResponseData<T>>(responseContent).data;
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default(T);
            }
        }

        public async Task<UserKey> SyncUser(UserKey user)
        {
            try
            {
                var serializedUser = JsonSerializer.Serialize(user, options);
                var httpContent = new StringContent(
                    serializedUser,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PutAsync("cloud/users", httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing user: {0}", serializedUser));
                    }
                    return JsonSerializer.Deserialize<UserKey>(responseContent);
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while syncing user: {0}", serializedUser));
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing user");
                return null;
            }
        }

        public async Task<UserKey> getUser(string userId)
        {
            return await CloudRequest<UserKey>(string.Format("cloud/users/{0}", userId));
        }

        public async Task<bool> DeleteUser(string userKey)
        {
            try
            {
                var response = await Client
                    .DeleteAsync(string.Format("cloud/users/{0}", userKey))
                    .ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write(string.Format("Error while deleting user {0}", userKey));
                return false;
            }
        }

        public async Task<bool> DeleteTenant(string tenantKey)
        {
            try
            {
                var response = await Client
                    .DeleteAsync(string.Format("cloud/tenants/{0}", tenantKey))
                    .ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write(string.Format("Error while deleting tenant {0}", tenantKey));
                return false;
            }
        }

        public async Task<Tenant> getTenant(string tenantId)
        {
            return await CloudRequest<Tenant>(string.Format("cloud/tenants/{0}", tenantId));
        }

        public async Task<Tenant> CreateTenant(ITenant tenant)
        {
            try
            {
                //var modifiedTenant = new Dictionary<string, string>
                //{
                //    { "externalId", tenant.externalId },
                //    { "name", tenant.name }
                //};
                //if (tenant.description != null)
                //{
                //    modifiedTenant.Add("description", tenant.description);
                //}
                var serializedTenant = JsonSerializer.Serialize(tenant, options);
                var httpContent = new StringContent(
                    serializedTenant,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PutAsync("cloud/tenants", httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing tenant: {0}", serializedTenant));
                    }
                    return JsonSerializer.Deserialize<Tenant>(responseContent);
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(
                        string.Format("Error while syncing tenant: {0}", serializedTenant)
                    );
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing tenant");
                return null;
            }
        }

        public async Task<Tenant> UpdateTenant(Tenant tenant)
        {
            try
            {
                //var modifiedTenant = new Dictionary<string, string> { { "name", tenant.name } };
                //if (tenant.description != null)
                //{
                //    modifiedTenant.Add("description", tenant.description);
                //}
                var serializedTenant = JsonSerializer.Serialize(tenant, options);
                var httpContent = new StringContent(
                    serializedTenant,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PatchAsync(string.Format("cloud/tenants/{0}", tenant.externalId), httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing tenant: {0}", tenant.externalId));
                    }
                    return JsonSerializer.Deserialize<Tenant>(responseContent);
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(
                        string.Format("Error while syncing tenant: {0}", tenant.externalId)
                    );
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing tenant");
                return null;
            }
        }

        public async Task<Role> CreateRole(Role role)
        {
            try
            {
                var serializedRole = JsonSerializer.Serialize(role, options);
                var httpContent = new StringContent(
                    serializedRole,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PostAsync("cloud/roles", httpContent)
                    .ConfigureAwait(false);
                if (
                    response.StatusCode == HttpStatusCode.OK
                    || response.StatusCode == HttpStatusCode.Created
                )
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Creating role: {0}", serializedRole));
                    }
                    return JsonSerializer.Deserialize<Role>(responseContent);
                }
                else
                {
                    Console.Write(string.Format("Error while syncing role: {0}", serializedRole));
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing role");
                return null;
            }
        }

        public async Task<Role> GetRoleById(string roleId)
        {
            return await CloudRequest<Role>(string.Format("cloud/roles/{0}", roleId));
        }

        public async Task<Role[]> GetRoles()
        {
            return await CloudRequestList<Role[]>("cloud/roles");
        }

        public async Task<RoleAssignment> AssignRole(
            string userKey,
            string roleKey,
            string tenantKey
        )
        {
            try
            {
                var assignRoleData = new Dictionary<string, string>
                {
                    { "role", roleKey },
                    { "user", userKey },
                    { "scope", tenantKey }
                };

                var serializedRole = JsonSerializer.Serialize(assignRoleData, options);
                var httpContent = new StringContent(
                    serializedRole,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PostAsync("cloud/role_assignments", httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Assigning role: {0}", serializedRole));
                    }
                    return JsonSerializer.Deserialize<RoleAssignment>(responseContent);
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while assigning role: {0}", serializedRole));
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while assigning role");
                return null;
            }
        }

        public async Task<RoleAssignment[]> getAssignedRoles(string userId, string tenantId = null)
        {
            var uri = string.Format("cloud/role_assignments?user={0}", userId);
            uri = (tenantId != null) ? uri + string.Format("&tenant={0}", tenantId) : uri;
            return await CloudRequestList<RoleAssignment[]>(uri);
        }

        public async Task<bool> unassignRole(string userKey, string roleKey, string tenantKey)
        {
            try
            {
                var response = await Client
                    .DeleteAsync(
                        string.Format(
                            "cloud/role_assignments?role={0}&user={1}&scope={2}",
                            roleKey,
                            userKey,
                            tenantKey
                        )
                    )
                    .ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write(string.Format("Error while deleting role {0}", roleKey));
                return false;
            }
        }

        public async Task<bool> SyncResources(ResourceType[] resourceTypes)
        {
            try
            {
                var parameters = new Dictionary<string, ResourceType[]>
                {
                    { "resources", resourceTypes }
                };
                var serializedResources = JsonSerializer.Serialize(parameters, options);
                var httpContent = new StringContent(
                    serializedResources,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client
                    .PutAsync("cloud/resources", httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing resources: {0}", serializedResources));
                    }
                    return true;
                }
                else
                {
                    Console.Write(
                        string.Format("Error while syncing resources: {0}", serializedResources)
                    );
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing resources");
                return false;
            }
        }
    }
}
