

namespace Permit
{

    public class LoggerConfig
    {
        public string Level { get; private set; }
        public string Label { get; private set; }
        public bool LogAsJson { get; private set; }

        public LoggerConfig(string level = "info", string label = "permit-sdk", bool logAsJson = false)
        {
            Level = level;
            Label = label;
            LogAsJson = logAsJson;
        }
    }

    public class Config
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        public string Token { get; private set; }
        public string Pdp { get; private set; }
        public bool DebugMode { get; private set; }
        public string DefaultTenant { get; private set; }
        public bool UseDefaultTenantIfEmpty { get; private set; }
        LoggerConfig Log { get; }

        public Config(string token, string pdp = DEFAULT_PDP_URL, bool debugMode = false, string level = "info", string label = "permitio-sdk", string defaultTenant = null, bool useDefaultTenantIfEmpty = false, bool logAsJson = false)
        {
            this.Token = token;
            this.Pdp = pdp;
            this.DebugMode = debugMode;
            this.DefaultTenant = defaultTenant;
            this.UseDefaultTenantIfEmpty = UseDefaultTenantIfEmpty;
            this.Log = new LoggerConfig(level, label, logAsJson);
        }
    }

}