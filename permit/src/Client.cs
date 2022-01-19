
namespace Permit
{
    public class Client
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        Config config { get; }
        public Enforcer enforcer;
        public Cache cache;

        public Client(string token, string pdp = DEFAULT_PDP_URL, bool debugMode = false, string level = "info", string label = "permitio-sdk", string defaultTenant = "default", bool logAsJson = false)
        {
            this.config = new Config(token, pdp, debugMode, level, label, defaultTenant, logAsJson);
            this.enforcer = new Enforcer(this.config, this.config.Pdp);
            this.cache = new Cache(this.config, this.config.Pdp);
        }
    }

}