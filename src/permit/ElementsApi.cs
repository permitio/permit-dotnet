using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PermitSDK.NewAPI;
using Role = PermitSDK.Models.Role;


namespace PermitSDK
{
    public class ElementsApi
    {
        private NewApiConfig _config;
        private NewAPI.PermitClient _client; 

        private string _projId;
        private string _envId;

        public ElementsApi(NewApiConfig config)
        {
            _config = config;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.Token);
            _client = new NewAPI.PermitClient(_config.ApiURL, httpClient);

            var apiKeyScope = _client.Get_api_key_scopeAsync().Result;
            _projId = apiKeyScope.Project_id.ToString();
            _envId = apiKeyScope.Environment_id.ToString();
        }

        public ElementsApi() : this(new NewApiConfig
            { ApiURL = NewApiConfig.DefaultApiUrl, PdpURL = NewApiConfig.DefaultPdpUrl, DebugMode = false })
        {

        }
        public async Task<LoginClient> LoginAs(string userId, string tenantId)
        {
            var userLogin =  new UserLoginRequestInput(userId, tenantId);
            return await _client.Login_AsAsync(_projId, _envId, userKey);
        }

    }
}