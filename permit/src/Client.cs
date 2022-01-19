
namespace Permit
{
    public class Client
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        Config Config { get; }
        public Enforcer Enforcer;
        public Cache Cache;
        public API Api;

        public Client(string token, string pdp = DEFAULT_PDP_URL, bool debugMode = false, string level = "info", string label = "permitio-sdk", string defaultTenant = "default", bool logAsJson = false)
        {
            this.Config = new Config(token, pdp, debugMode, level, label, defaultTenant, logAsJson);
            this.Enforcer = new Enforcer(this.Config, this.Config.Pdp);
            this.Cache = new Cache(this.Config, this.Config.Pdp);
            this.Api = new API(this.Config, this.Config.Pdp);
        }
    }

}