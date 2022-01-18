using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using permit.io.Models;

namespace permit.io
{
    public class Cloud
    {

        string Url;
        Config config;
        HttpClient client = new HttpClient();

        public Cloud(Config config, string remotePermitUrl)
        {
            this.Url = remotePermitUrl;
            this.config = config;
            client.BaseAddress = new Uri(remotePermitUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token); // TODO change to permit.io token, is it the same?
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        }


        public async Task<T> CloudRequest<T>(string uri)
        {
            try
            {
                var response = await client.GetAsync(uri).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Do something with response. Example get content:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Sending {0} request to cloud service", uri));
                    }
                    return (T)JsonSerializer.Deserialize<IResponseData>(responseContent).data;
                }
                else
                {
                    return default(T);
                }
            }
            catch
            {
                return default(T);
            }
        }


        public async Task<ISyncedUser> getUser(string userId)
        {
            return await CloudRequest<ISyncedUser>(string.Format("cloud/users/{0}", userId));
        }
        public async Task<string[]> getTenant(string tenantId)
        {
            return await CloudRequest<string[]>(string.Format("cloud/tenants/{0}", tenantId));
        }
        public async Task<ISyncedRole[]> getAssignedRoles(string userId, string tenantId = null)
        {
            var uri = string.Format("cloud/role_assignments?user={0}", userId);
            uri = tenantId != null ? uri + string.Format("&tenant={0}", tenantId) : "";
            return await CloudRequest<ISyncedRole[]>(uri));
        }
        public async Task<ISyncedRole> GetRoleById(string roleId)
        {
            return await CloudRequest<ISyncedRole>(string.Format("cloud/roles/{0}", roleId));
        }
        //public async Task<string[]> getUserTenants(string userId)
        //{
        //    return await CloudRequest<string[]>(string.Format("cloud/users/{0}/tenants", userId));
        //}
        //public async Task<User[]> GetUsers()
        //{
        //    return await CloudRequest<User[]>("cloud/users");
        //}
        //public async Task<ISyncedRole[]> GetRoles()
        //{
        //    return await CloudRequest<ISyncedRole[]>("cloud/roles");
        //}
        //public async Task<ISyncedRole> GetRoleByName(string roleName)
        //{
        //    return await CloudRequest<ISyncedRole>(string.Format("cloud/roles/by-name/{0}", roleName));
        //}

        public async Task<bool> DeleteUser(string userKey)
        {
            try
            {
                var response = await client.DeleteAsync(
                    string.Format("cloud/users/{0}", userKey)).ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //todo add exception / error
                Console.Write(string.Format("Error while deleting user {0}", userKey));
                return false;
            }
        }


        public async Task<bool> DeleteTenant(string tenantKey)
        {
            try
            {
                var response = await client.DeleteAsync(
                    string.Format("cloud/tenants/{0}", tenantKey)).ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //todo add exception / error
                Console.Write(string.Format("Error while deleting tenant {0}", tenantKey));
                return false;
            }
        }

        public async Task<bool> UnassignRole(string userKey, string roleKey, string tenantKey)
        {
            try
            {
                var response = await client.DeleteAsync(
                    string.Format("cloud/role_assignments?role={0}&user={1}&scope={2}", roleKey, userKey, tenantKey)).ConfigureAwait(false);
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //todo add exception / error
                Console.Write(string.Format("Error while requesting to unassign roleKey {0} to userKey {1} in tenant {2}", roleKey, userKey, tenantKey));
                return false;
            }
        }


        public async Task<IUser> SyncUser(IUser user)
        {
            try
            {
                var serializedUser = JsonSerializer.Serialize(user);
                var httpContent = new StringContent(serializedUser, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(
                    "cloud/users",
                    httpContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Do something with response. Example get content:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing user: {0}", serializedUser));
                    }
                    return (IUser)JsonSerializer.Deserialize<IResponseData>(responseContent).data;

                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while syncing user: {0}", serializedUser));
                    return null;

                }

            }
            catch
            {
                //todo add exception / error
                Console.Write("Error while syncing user");
                return null;
            }
        }
        public async Task<ITenant> CreaetTenant(ITenant tenant)
        {
            try
            {
                var modifiedTenant = new Dictionary<string, string> { { "externalId", tenant.key }, { "name", tenant.name } };
                if (tenant.description != null)
                {
                    modifiedTenant.Add("description", tenant.description);
                }
                var serializedTenant = JsonSerializer.Serialize(modifiedTenant);
                var httpContent = new StringContent(serializedTenant, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(
                    "cloud/tenants",
                    httpContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing tenant: {0}", serializedTenant));
                    }
                    return (ITenant)JsonSerializer.Deserialize<IResponseData>(responseContent).data;

                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while syncing tenant: {0}", serializedTenant));
                    return null;

                }

            }
            catch
            {
                //todo add exception / error
                Console.Write("Error while syncing tenant");
                return null;
            }
        }
        public async Task<ITenant> UpdateTenant(ITenant tenant)
        {
            try
            {
                var modifiedTenant = new Dictionary<string, string> { { "name", tenant.name } };
                if (tenant.description != null)
                {
                    modifiedTenant.Add("description", tenant.description);
                }
                var serializedTenant = JsonSerializer.Serialize(modifiedTenant);
                var httpContent = new StringContent(serializedTenant, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(
                    string.Format("cloud/tenants/{0}", tenant.key),
                    httpContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing tenant: {0}", tenant.key));
                    }
                    return (ITenant)JsonSerializer.Deserialize<IResponseData>(responseContent).data;

                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while syncing tenant: {0}", tenant.key));
                    return null;

                }

            }
            catch
            {
                //todo add exception / error
                Console.Write("Error while syncing tenant");
                return null;
            }
        }

        public async Task<ISyncedRole> AssignRole(string userKey, string roleKey, string tenantKey)
        {
            try
            {
                var assignRoleData = new Dictionary<string, string> { { "role", roleKey }, { "user", userKey }, { "scope", tenantKey } };

                var serializedRole = JsonSerializer.Serialize(assignRoleData);
                var httpContent = new StringContent(serializedRole, Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(
                    "cloud/role_assignments",
                    httpContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Assigning role: {0}", serializedRole));
                    }
                    return (ISyncedRole)JsonSerializer.Deserialize<IResponseData>(responseContent).data;

                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while assigning role: {0}", serializedRole));
                    return null;

                }

            }
            catch
            {
                //todo add exception / error
                Console.Write("Error while assigning role");
                return null;
            }
        }
    }
}
