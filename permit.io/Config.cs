using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

//.net standart
//ClassLib.Model ?
//namespace for project contains the folder structure
//add project reference by right click on dependencies



namespace permit.io
{

    public class LoggerConfig
    {
        string Level;
        string Label;
        bool LogAsJson;

        public LoggerConfig ( string level = "info", string label = "permit.io-sdk", bool logAsJson = false)
        {
            this.Level = level;
            this.Label = label;
            this.LogAsJson = logAsJson;
        }
    }

    public class Config
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";

        public string Token { get; private set; }
        public string Pdp { get; private set; }
        public bool DebugMode { get; private set; }
        public string defaultTenant { get; private set; }
        LoggerConfig Log;

        public Config(string token, string pdp = DEFAULT_PDP_URL, bool debugMode = false, string level = "info", string label = "permitio-sdk", string defaultTenant = null, bool logAsJson = false)
        {
            this.Token = token;
            this.Pdp = pdp;
            this.DebugMode = debugMode;
            this.defaultTenant = defaultTenant;
            this.Log = new LoggerConfig(level, label, logAsJson);
        }
    }

}