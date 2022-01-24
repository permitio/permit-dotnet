using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Permit.Models;

//.net standart
//ClassLib.Model ?
//namespace for project contains the folder structure
//add project reference by right click on dependencies

public interface IResponseData<T>
{
    public T data { get; }
}

public class ResponseData<T>
{
    public T data { get; set; }

    ResponseData() { }
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
            Client.DefaultRequestHeaders.Add(
                "Authorization",
                string.Format("Bearer {0}", config.Token)
            );
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );


        }

        public async Task<T> GetCache<T>(string uri)
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
                        Console.Write(string.Format("Sending {0} request to local sidecar", uri));
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

        public async Task<bool> isUser(string userId)
        {
            return await GetCache<SyncedUser>(string.Format("local/users/{0}", userId)) != null;
        }

        public async Task<SyncedUser> getUser(string userId)
        {
            return await GetCache<SyncedUser>(string.Format("local/users/{0}", userId));
        }

        public async Task<string[]> getUserTenants(string userId)
        {
            return await GetCache<string[]>(string.Format("local/users/{0}/tenants", userId));
        }

        public async Task<SyncedRole[]> getAssignedRoles(string userId)
        {
            return await GetCache<SyncedRole[]>(string.Format("local/users/{0}/roles", userId));
        }

        public async Task<SyncedUser[]> GetUsers()
        {
            return await GetCache<SyncedUser[]>("local/users");
        }

        public async Task<SyncedRole[]> GetRoles()
        {
            return await GetCache<SyncedRole[]>("local/roles");
        }

        public async Task<SyncedRole> GetRoleById(string roleId)
        {
            return await GetCache<SyncedRole>(string.Format("local/roles/{0}", roleId));
        }

        public async Task<SyncedRole> GetRoleByName(string roleName)
        {
            return await GetCache<SyncedRole>(string.Format("local/roles/by-name/{0}", roleName));
        }

        public async Task<bool> TriggerPolicyUpdate()
        {
            try
            {
                var httpContent = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await Client
                    .PostAsync("policy-updater/trigger", httpContent)
                    .ConfigureAwait(false);
                Console.Write("Sending Trigger Policy update request to local sidecar");
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> TriggerDataUpdate()
        {
            try
            {
                var httpContent = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await Client
                    .PostAsync("data-updater/trigger", httpContent)
                    .ConfigureAwait(false);
                Console.Write("Sending Trigger Data update request to local sidecar");
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> TriggerDataAndPolicyUpdate()
        {
            return await TriggerDataUpdate() && await TriggerPolicyUpdate();
        }
    }
}
