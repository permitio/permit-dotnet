using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Permit.Models;

//.net standart
//ClassLib.Model ?
//namespace for project contains the folder structure
//add project reference by right click on dependencies

public interface IResponseData
{
    public object data { get; }
}

namespace Permit
{

    public class Cache
    {
        string Url;
        Config Config;
        HttpClient Client = new HttpClient();

        public Cache(Config config, string url = Permit.Client.DEFAULT_PDP_URL)
        {
            this.Url = url;
            this.Config = config;
            Client.BaseAddress = new Uri(url);
            Client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer", config.Token));
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


        }

        public async Task<T> GetCache<T>(string uri)
        {
            try
            {
                var response = await Client.GetAsync(uri).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Do something with response. Example get content:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        Console.Write(string.Format("Sending {0} request to local sidecar", uri));
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

        public async Task<bool> isUser(string userId)
        {
            return await GetCache<ISyncedUser>(string.Format("local/users/{0}", userId)) != null;
        }

        public async Task<ISyncedUser> getUser(string userId)
        {
            return await GetCache<ISyncedUser>(string.Format("local/users/{0}", userId));
        }
        public async Task<string[]> getUserTenants(string userId)
        {
            return await GetCache<string[]>(string.Format("local/users/{0}/tenants", userId));
        }
        public async Task<ISyncedRole[]> getAssignedRoles(string userId)
        {
            return await GetCache<ISyncedRole[]>(string.Format("local/users/{0}/roles", userId));
        }
        public async Task<User[]> GetUsers()
        {
            return await GetCache<User[]>("local/users");
        }
        public async Task<ISyncedRole[]> GetRoles()
        {
            return await GetCache<ISyncedRole[]>("local/roles");
        }
        public async Task<ISyncedRole> GetRoleById(string roleId)
        {
            return await GetCache<ISyncedRole>(string.Format("local/roles/{0}", roleId));
        }
        public async Task<ISyncedRole> GetRoleByName(string roleName)
        {
            return await GetCache<ISyncedRole>(string.Format("local/roles/by-name/{0}", roleName));
        }

        public async Task<bool> TriggerPolicyUpdate()
        {
            try
            {
                var response = await Client.GetAsync("policy-updater/trigger").ConfigureAwait(false);
                Console.Write("Sending Trigger Policy update request to local sidecar");
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> TriggerDataUpdate()
        {
            try
            {
                var response = await Client.GetAsync("data-updater/trigger").ConfigureAwait(false);
                Console.Write("Sending Trigger Data update request to local sidecar");
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TriggerDataAndPolicyUpdate()
        {
            return await TriggerDataUpdate() && await TriggerPolicyUpdate();
        }

    }
}