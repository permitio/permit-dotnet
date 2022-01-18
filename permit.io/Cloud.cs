using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace permit.io
{
    public class Mutations
    {

        string Url;
        Config config;
        HttpClient client = new HttpClient();

        public string SyncResourcesUri { get; private set; } = "v1/resources";

        public Mutations(Config config, string remotePermitUrl)
        {
            this.Url = remotePermitUrl;
            this.config = config;
            client.BaseAddress = new Uri(remotePermitUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token); // TODO change to permit.io token, is it the same?
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        }

    }
}
