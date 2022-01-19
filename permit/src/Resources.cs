using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Permit.Models;



namespace Permit
{

    public class Resources
    {
        string Url;
        Config config;
        HttpClient client = new HttpClient();

        public string SyncResourcesUri { get; private set; } = "cloud/resources";

        public Resources(Config config, string remotePermitUrl)
        {
            this.Url = remotePermitUrl;
            this.config = config;
            client.BaseAddress = new Uri(remotePermitUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token); // TODO change to permit token, is it the same?
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        }


        public async Task<IResource[]> SyncResources(ResourceType[] resourceTypes)
        {
            try
            {
                var serializedResources = JsonSerializer.Serialize(resourceTypes);
                var parameters = new Dictionary<string, string> { { "resources", serializedResources } };
                var encodedContent = new FormUrlEncodedContent(parameters);
                var response = await client.PutAsync(
                    SyncResourcesUri,
                    encodedContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Do something with response. Example get content:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Syncing resources: {0}", serializedResources));
                    }
                    return (IResource[])JsonSerializer.Deserialize<IResponseData>(responseContent).data;

                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while syncing resources: {0}", serializedResources));
                    return null;

                }

            }
            catch
            {
                //todo add exception / error
                Console.Write("Error while syncing resources");
                return null;
            }
        }
    }

}