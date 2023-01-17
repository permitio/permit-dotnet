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
        private NewAPI.LoginClient _client;

        public ElementsApi(NewApiConfig config)
        {
            _config = config;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.Token);
            _client = new NewAPI.LoginClient(_config.ApiURL, httpClient);

        }

        public ElementsApi() : this(new NewApiConfig
            { ApiURL = NewApiConfig.DefaultApiUrl, PdpURL = NewApiConfig.DefaultPdpUrl, DebugMode = false })
        {

        }
        public async Task<EmbeddedLoginRequestOutput> LoginAs(string userId, string tenantId)
        {
            var userLogin =  new UserLoginRequestInput();
            userLogin.User_id = userId;
            userLogin.Tenant_id = tenantId;
            return await _client.Login_AsAsync(userLogin);
        }

    }
}