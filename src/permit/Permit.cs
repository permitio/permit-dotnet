using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PermitSDK.Models;

namespace PermitSDK
{
    public class Permit
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        private Config _config;
        public Enforcer Enforcer { get; private set; }
        public Api Api { get; private set; }

        public Permit(
            string token,
            string pdp = DEFAULT_PDP_URL,
            string defaultTenant = "default",
            bool useDefaultTenantIfEmpty = false,
            bool debugMode = false,
            string level = "info",
            string label = "permitio-sdk",
            bool logAsJson = false,
            string apiUrl = "https://api.permit.io"
        )
        {
            _config = new Config(
                token,
                pdp,
                defaultTenant,
                useDefaultTenantIfEmpty,
                debugMode,
                level,
                label,
                logAsJson,
                apiUrl
            );

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = loggerFactory.CreateLogger<Permit>();

            Enforcer = new Enforcer(_config, _config.Pdp, logger);
            Api = new Api(_config, logger);
        }

        public async Task<bool> Check(
            IUserKey user,
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
    }
}
