using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Permit.Models;

namespace Permit
{
    public class Resources
    {
        string Url;
        Config config;
        HttpClient Client = new HttpClient();

        public string SyncResourcesUri { get; private set; } = "cloud/resources";

        public Resources(Config config, string remotePermitUrl)
        {
            this.Url = remotePermitUrl;
            this.config = config;
            Client.BaseAddress = new Uri(remotePermitUrl);
            Client.DefaultRequestHeaders.Add(
                "Authorization",
                string.Format("Bearer {0}", config.Token)
            );
            Client.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        public async Task<IResource[]> SyncResources(ResourceType[] resourceTypes)
        {
            try
            {
                var serializedResources = JsonSerializer.Serialize(resourceTypes);
                var parameters = new Dictionary<string, string>
                {
                    { "resources", serializedResources }
                };
                var encodedContent = new FormUrlEncodedContent(parameters);
                var response = await Client
                    .PutAsync(SyncResourcesUri, encodedContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing resources: {0}", serializedResources));
                    }
                    return JsonSerializer.Deserialize<IResponseData<IResource[]>>(
                        responseContent
                    ).data;
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(
                        string.Format("Error while syncing resources: {0}", serializedResources)
                    );
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Write("Error while syncing resources");
                return null;
            }
        }
    }
}
