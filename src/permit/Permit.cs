using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PermitSDK.Models;

namespace PermitSDK
{
    public class Permit
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7766";
        public const string DEFAULT_API_URL = "https://api.permit.io";

        public Config Config { get; private set; }
        public Enforcer Enforcer { get; private set; }
        public Api Api { get; private set; }
        public ElementsApi Elements { get; private set; }

        public Permit(
            string token,
            string pdp = DEFAULT_PDP_URL,
            string defaultTenant = "default",
            bool useDefaultTenantIfEmpty = true,
            bool debugMode = false,
            string apiUrl = DEFAULT_API_URL,
            string level = "info",
            string label = "permitio-sdk",
            bool logAsJson = false,
            string projectId = null,
            string envId = null
        )
        {
            Config = new Config(
                token,
                pdp,
                defaultTenant,
                useDefaultTenantIfEmpty,
                debugMode,
                level,
                label,
                logAsJson,
                apiUrl,
                projectId,
                envId
            );

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = loggerFactory.CreateLogger<Permit>();

            this.Enforcer = new Enforcer(this.Config, this.Config.Pdp, logger);

            this.Api = new Api(this.Config, logger);
            // todo need to move elements to work with the regular config
            this.Elements = new ElementsApi(
                new NewApiConfig
                {
                    ApiURL = apiUrl,
                    PdpURL = pdp,
                    DebugMode = debugMode,
                    Token = token
                }
            );
        }

        public async Task<bool> Check(
            UserKey user,
            string action,
            ResourceInput resource,
            Dictionary<string, string> context = null
        )
        {
            return await Enforcer.Check(user, action, resource, context);
        }

        public async Task<bool> Check(
            string user,
            string action,
            string resource,
            Dictionary<string, string> context = null
        )
        {
            return await Enforcer.Check(user, action, resource, context);
        }

        public async Task<List<bool>> BulkCheck(
            List<CheckQuery> checks,
            Dictionary<string, string> context = null
        )
        {
            return await Enforcer.BulkCheck(checks, context);
        }

        public async Task<List<bool>> BulkCheck(
            List<CheckQueryObj> checks,
            Dictionary<string, string> context = null
        )
        {
            return await Enforcer.BulkCheck(checks, context);
        }
    }
}
