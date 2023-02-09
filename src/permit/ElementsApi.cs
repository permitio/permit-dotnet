using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using PermitSDK.NewAPI;
using Role = PermitSDK.Models.Role;

public class EmbeddedLoginContentRequestOutput
{
    public Dictionary<string,string> Content { get; set; }
    public string Error { get; set; }
    public int ErrorCode { get; set; }
    public string RedirectUrl { get; set; }
    public string Token { get; set; }
    public string Extra { get; set; }

}



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
        public async Task<EmbeddedLoginContentRequestOutput> LoginAs(string userId, string tenantId)
        {
            var userLogin =  new UserLoginRequestInput();
            userLogin.User_id = userId;
            userLogin.Tenant_id = tenantId;
            var response = await _client.Login_AsAsync(userLogin);
            var output = new EmbeddedLoginContentRequestOutput();
            output.Content = new Dictionary<string, string>();
            output.Content.Add("token", response.Token);
            output.Error = response.Error;
            output.ErrorCode = response.Error_code;
            output.RedirectUrl = response.Redirect_url;
            output.Token = response.Token;
            output.Extra = response.Extra;
            return output;
        }

    }
}