namespace PermitSDK
{
    public class LoggerConfig
    {
        public string Level { get; private set; }
        public string Label { get; private set; }
        public bool LogAsJson { get; private set; }

        public LoggerConfig(
            string level = "info",
            string label = "permit-sdk",
            bool logAsJson = false
        )
        {
            Level = level;
            Label = label;
            LogAsJson = logAsJson;
        }
    }

    public class Config
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7766";

        public string Token { get; private set; }
        public string Pdp { get; private set; }
        public bool DebugMode { get; private set; }
        public string DefaultTenant { get; private set; }
        public bool UseDefaultTenantIfEmpty { get; private set; }
        LoggerConfig Log { get; }
        public string ApiUrl { get; private set; }

        public Config(
            string token,
            string pdp = DEFAULT_PDP_URL,
            string defaultTenant = null,
            bool useDefaultTenantIfEmpty = false,
            bool debugMode = false,
            string level = "info",
            string label = "permitio-sdk",
            bool logAsJson = false,
            string apiUrl = "https://api.permit.io"
        )
        {
            Token = token;
            Pdp = pdp;
            DebugMode = debugMode;
            DefaultTenant = defaultTenant;
            UseDefaultTenantIfEmpty = useDefaultTenantIfEmpty;
            Log = new LoggerConfig(level, label, logAsJson);
            ApiUrl = apiUrl;
        }
    }
}
