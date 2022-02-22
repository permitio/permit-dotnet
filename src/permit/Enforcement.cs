using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using PermitSDK.Models;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;

public class PermitException : Exception
{
    public PermitException() { }

    public PermitException(string message) : base(message) { }
}

public class PermitConnectionError : PermitException
{
    public PermitConnectionError() { }

    public PermitConnectionError(string message) : base(message) { }
}

public class PermitPDPResponseError : PermitException
{
    public PermitPDPResponseError() { }

    public PermitPDPResponseError(string message) : base(message) { }
}

namespace PermitSDK
{
    public interface IResource
    {
        public string type { get; }
        public string id { get; }
        public string tenant { get; }
        public IDictionary<string, string> attributes { get; }
        public IDictionary<string, string> context { get; }
    }

    /// <summary>
    /// Implements Permit Enforcer using API checks against PDP sidecar.
    /// </summary>
    public class Enforcer
    {
        string Url;
        string CheckURI = "/allowed";
        Config Config;
        HttpClient Client = new HttpClient();
        ILogger logger;
        public JsonSerializerOptions options { get; private set; }

        public Enforcer(Config config, string url = Permit.DEFAULT_PDP_URL, ILogger logger = null)
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
            this.options = new JsonSerializerOptions();
            this.options.IgnoreNullValues = true;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> Check(
            IUserKey user,
            string action,
            ResourceInput resource,
            Dictionary<string, string> context = null
        )
        {
            var normalizedResource = ResourceInput.Normalize(resource, Config);
            var parameters = new Dictionary<string, object>
            {
                { "user", user.key },
                { "action", action },
                { "resource", normalizedResource },
                { "context", context }
            };
            var serializedResources = JsonSerializer.Serialize(parameters, options);
            var httpContent = new StringContent(
                serializedResources,
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await Client
                    .PostAsync(Url + CheckURI, httpContent)
                    .ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        this.logger.LogInformation(
                            string.Format(
                                "Checking permission for {0} to perform {1} on {2}",
                                user,
                                action,
                                resource.type
                            )
                        );
                    }
                    bool decision =
                        JsonSerializer.Deserialize<PermitCheck>(responseContent).allow || false;
                    return decision;
                }
                else
                {
                    this.logger.LogError(
                        string.Format(
                            "Error while checking permission for {0} to perform {1} on {2}",
                            user,
                            action,
                            resource.type
                        )
                    );
                    throw new PermitPDPResponseError(
                        string.Format(
                            "Permit SDK got status: {0}, please check your SDK init and make sure the PDP sidecar is configured correctly. \n"
                                + "Read more about setting up the PDP at https://docs.permit.io/reference/SDKs/dotnet/",
                            response.StatusCode
                        )
                    );
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
                this.logger.LogInformation(
                    string.Format(
                        "Error while checking permission for {0} to perform {1} on {2}",
                        user,
                        action,
                        resource.type
                    )
                );
                throw new PermitConnectionError(
                    string.Format(
                        "Permit SDK got error: {0}, \n"
                            + "and cannot connect to the PDP, please check your configuration and make sure the PDP is running at {1} and accepting requests. \n"
                            + "Read more about setting up the PDP at https://docs.permit.io/reference/SDKs/dotnet/",
                        e.ToString(),
                        Url
                    )
                );
            }
        }

        public async Task<bool> Check(
            string user,
            string action,
            string resource,
            Dictionary<string, string> context = null
        )
        {
            var resourceObject = ResourceInput.ResourceFromString(resource);
            var userKey = new UserKey(user);
            return await Check(userKey, action, resourceObject, context);
        }
    }
}
