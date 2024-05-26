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
using System.Collections;

public class PermissionCheckException : Exception
{
    public PermissionCheckException() { }

    public PermissionCheckException(string message) : base(message) { }
}

public class CreateResourceException : Exception
{
    public CreateResourceException() { }

    public CreateResourceException(string message) : base(message) { }
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
        string BulkCheckURI = "/allowed/bulk";
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
                { "user", user },
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
                    return false;
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
                return false;
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


        public Dictionary<string, object> BuildCheckInput(IUserKey user, string action, ResourceInput resource, Dictionary<string, string> context = null)
        {

            var normalizedResource = ResourceInput.Normalize(resource, Config);
            var parameters = new Dictionary<string, object>
            {
                { "user", user },
                { "action", action },
                { "resource", normalizedResource },
                { "context", context }
            };
            return parameters;
        }

        public async Task<List<bool>> BulkCheck(List<CheckQuery> checks, Dictionary<string, string> context = null)
        {
            var inputs = new List<CheckQueryObj>();
            foreach (var check in checks)
            {
                var input = new CheckQueryObj(
                    new UserKey(check.user),
                    check.action,
                    ResourceInput.ResourceFromString(check.resource),
                    context
                );
                inputs.Add(input);
            }
            return await BulkCheck(inputs, context);
        }



        public async Task<List<bool>> BulkCheck(List<CheckQueryObj> checks, Dictionary<string, string> context = null)
        {
            var inputs = new List<Dictionary<string, object>>();
            foreach (var check in checks)
            {
                var input = BuildCheckInput(check.user, check.action, check.resource, context);
                inputs.Add(input);
            }



            try
            {
                var serializeBulkCheck = JsonSerializer.Serialize(inputs, options);
                var httpContent = new StringContent(
                    serializeBulkCheck,
                    Encoding.UTF8,
                    "application/json"
                );
                var response = await Client.PostAsync(
                    Url + BulkCheckURI,
                    httpContent
                ).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (Config.DebugMode)
                    {
                        this.logger.LogInformation(
                            string.Format(
                                "Performing bulk check for {0} queries",
                                checks.Count
                            )
                        );
                    }
                    var bulkData = JsonSerializer.Deserialize<BulkPolicyDecision>(responseContent);
                    var result = new List<bool>();
                    foreach (var decision in bulkData.allow)
                    {
                        result.Add(decision.allow);
                    }
                    return result;
                }
                else
                {
                    this.logger.LogError(
                        string.Format(
                            "Error while performing bulk check"
                        )
                    );
                    return new List<bool>();
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
                this.logger.LogInformation(
                    string.Format(
                        "Error while performing bulk check"
                    )
                );
                return new List<bool>();
            }
        }

    }
}
