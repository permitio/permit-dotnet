using System.Collections.Generic;
using System.Threading.Tasks;
using PermitSDK.Models;

namespace PermitSDK
{
    public class Permit
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        public Config Config { get; private set; }
        public Enforcer Enforcer { get; private set; }
        public Cache Cache { get; private set; }
        public Api Api { get; private set; }

        public Permit(
            string token,
            string pdp = DEFAULT_PDP_URL,
            string defaultTenant = "default",
            bool useDefaultTenantIfEmpty = false,
            bool debugMode = false,
            string level = "info",
            string label = "permitio-sdk",
            bool logAsJson = false
        )
        {
            this.Config = new Config(
                token,
                pdp,
                defaultTenant,
                useDefaultTenantIfEmpty,
                debugMode,
                level,
                label,
                logAsJson
            );
            this.Enforcer = new Enforcer(this.Config, this.Config.Pdp);
            this.Cache = new Cache(this.Config, this.Config.Pdp);
            this.Api = new Api(this.Config, this.Config.Pdp);
        }

        public async Task<bool> Check(
            IUserKey user,
            string action,
            ResourceInput resource,
            Dictionary<string, string> context = null
        )
        {
            return await this.Enforcer.Check(user, action, resource, context);
        }

        public async Task<bool> Check(
            string user,
            string action,
            string resource,
            Dictionary<string, string> context = null
        )
        {
            return await this.Enforcer.Check(user, action, resource, context);
        }
    }
}
