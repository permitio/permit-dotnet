using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using permit.io.Models;

//.net standart
//ClassLib.Model ?
//namespace for project contains the folder structure
//add project reference by right click on dependencies

public interface IResponseData
{
    public object data { get; }
}

namespace permit.io
{

    public class Cache
    {
        string Url;
        Config config;
        HttpClient client = new HttpClient();

        public Cache(Config config, string url = Client.DEFAULT_PDP_URL)
        {
            this.Url = url;
            this.config = config;
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.Token);
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");

        }





}
}