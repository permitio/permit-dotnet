using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using permit.io.Models;

//.net standart
//ClassLib.Model ?
//namespace for project contains the folder structure
//add project reference by right click on dependencies


public class PermissionCheckException : Exception
{
    public PermissionCheckException()
    {
    }

    public PermissionCheckException(string message)
        : base(message)
    {
    }
}

public class CreateResourceException : Exception
{
    public CreateResourceException()
    {
    }

    public CreateResourceException(string message)
        : base(message)
    {
    }
}

namespace permit.io
{
    public class AssignedRole
    {
        string role;
        string tenant;

        public AssignedRole(string role, string tenant)
        {
            this.role = role;
            this.tenant = tenant;
        }
    }

    public interface IPermitCheckData
    {
        public bool allow { get; }
    }

    public interface IPermitCheck
    {
        public IPermitCheckData data { get; }
    }



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
        readonly HttpClient httpClient;
        string Url;
        string CheckURI = "/allowed";
        Config config;
        HttpClient client = new HttpClient();


        public Enforcer(Config config, string url = Client.DEFAULT_PDP_URL)
        {
            this.Url = url;
            this.config = config;
        }


        /// <inheritdoc />
        public async Task<bool> Check(IUserKey user, string action, ResourceInput resource, Dictionary<string, string> context = null)
        {

            //return await Retry(async () => await GetAsyncInternal<T>(this.Url, headers, converters, cancellationToken)).ConfigureAwait(false);

            //object cust = { Name = "Example Customer", Address = "Some example address", Phone = "Some phone number" };
            //var json = Serialize(cust);
            //var response = await PostAsync(HttpMethod.Post, this.Url, json, new Dictionary<string, string>());
            //string responseText = await response.Content.ReadAsStringAsync();

            //{
            //    ApplyHeaders(request.Headers, headers);
            //    return await SendRequest<T>(request, converters, cancellationToken).ConfigureAwait(false);
            //}
            var normalizedResource = JsonSerializer.Serialize(ResourceInput.Normalize(resource, config));
            var parameters = new Dictionary<string, string> { { "user", user.key }, { "action", action }, { "resource", normalizedResource }, { "context", context == null ? JsonSerializer.Serialize(context) : "" } };
            var encodedContent = new FormUrlEncodedContent(parameters);
            try
            {

                var response = await client.PostAsync(
                    Url + CheckURI,
                    encodedContent).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Do something with response. Example get content:
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (config.DebugMode)
                    {
                        Console.Write(string.Format("Checking permission for {0} to perform {1} on {2}", user, action, resource.type));
                    }
                    bool decision = JsonSerializer.Deserialize<IPermitCheck>(responseContent).data.allow || false;
                    return decision;
                }
                else
                {
                    //throw new PermissionCheckException("Permission check failed");
                    Console.Write(string.Format("Error while checking permission for {0} to perform {1} on {2}", user, action, resource.type));
                    return false;

                }
            }
            catch
            {
                //Logger exception 
                Console.Write(string.Format("Error while checking permission for {0} to perform {1} on {2}", user, action, resource.type));
                return false;
            }

        }

        public async Task<bool> Check(string user, string action, string resource, Dictionary<string, string> context = null)
        {
            //return await Retry(async () => await GetAsyncInternal<T>(uri, headers, converters, cancellationToken)).ConfigureAwait(false);
            var resourceObject = ResourceInput.ResourceFromString(resource);
            var userKey = new UserKey(user);
            return await Check(userKey, action, resourceObject, context);
            //var normalizedResource = JsonSerializer.Serialize(ResourceInput.Normalize(resourceObject, config));
            //var parameters = new Dictionary<string, string> { { "user", user }, { "action", action }, { "resource", normalizedResource }, { "context", context == null ? JsonSerializer.Serialize(context) : "" } };
            //var encodedContent = new FormUrlEncodedContent(parameters);

            //var response = await client.PostAsync(
            //    Url + CheckURI,
            //    encodedContent).ConfigureAwait(false);
            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    // Do something with response. Example get content:
            //    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //    Console.Write(responseContent);
            //    return true;
            //}
            //else
            //{
            //    throw new PermissionCheckException("Permission check failed");
            //}

        }

        //private async Check<T> GetAsyncInternal<T>(Uri uri, IDictionary<string, string> headers, JsonConverter[] converters = null, CancellationToken cancellationToken = default)
        //{
        //    using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
        //    {
        //        ApplyHeaders(request.Headers, headers);
        //        return await SendRequest<T>(request, converters, cancellationToken).ConfigureAwait(false);
        //    }
        //}


        //private async Check<T> SendRequest<T>(HttpRequestMessage request, JsonConverter[] converters = null, CancellationToken cancellationToken = default)
        //{
        //    var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        //    {
        //        if (!response.IsSuccessStatusCode)
        //            throw await ApiException.CreateSpecificExceptionAsync(response).ConfigureAwait(false);

        //        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        //        return typeof(T) == typeof(string)
        //            ? (T)(object)content
        //            : JsonConvert.DeserializeObject<T>(content,
        //                converters == null ? jsonSerializerSettings : new JsonSerializerSettings() { Converters = converters });
        //    }
        //}

        //private async Task<TResult> Retry<TResult>(Func<Task<TResult>> retryable)
        //{
        //    int? configuredNrOfTries = options.NumberOfHttpRetries;
        //    var nrOfTries = 0;
        //    var nrOfTriesToAttempt = Math.Min(MAX_NUMBER_RETRIES, configuredNrOfTries ?? DEFAULT_NUMBER_RETRIES);

        //    while (true)
        //    {
        //        try
        //        {
        //            nrOfTries++;

        //            return await retryable();
        //        }
        //        catch (Exception ex)
        //        {
        //            if (!(ex is RateLimitApiException) || nrOfTries >= nrOfTriesToAttempt)
        //            {
        //                throw;
        //            }
        //        }

        //        // Use an exponential back-off with the formula:
        //        // max(MIN_REQUEST_RETRY_DELAY, min(MAX_REQUEST_RETRY_DELAY, (BASE_DELAY * (2 ** attempt - 1)) + random_between(0, MAX_REQUEST_RETRY_JITTER)))
        //        //
        //        // ✔ Each attempt increases base delay by (100ms * (2 ** attempt - 1))
        //        // ✔ Randomizes jitter, adding up to MAX_REQUEST_RETRY_JITTER (250ms)
        //        // ✔ Never less than MIN_REQUEST_RETRY_DELAY (100ms)
        //        // ✔ Never more than MAX_REQUEST_RETRY_DELAY (500ms)

        //        var wait = Convert.ToInt32(BASE_DELAY * Math.Pow(2, nrOfTries - 1));
        //        wait = random.Next(wait + 1, wait + MAX_REQUEST_RETRY_JITTER);
        //        wait = Math.Min(wait, MAX_REQUEST_RETRY_DELAY);
        //        wait = Math.Max(wait, MIN_REQUEST_RETRY_DELAY);

        //        await Task.Delay(wait);
        //    }
        //}


    }
}