﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PermitSDK.Models;

namespace PermitSDK
{
    public class Permit
    {
        public const string DEFAULT_PDP_URL = "http://localhost:7000";
        public const string DEFAULT_API_URL = "http://app.permit.io";

        public Config Config { get; private set; }
        public Enforcer Enforcer { get; private set; }
        public Cache Cache { get; private set; }
        public APINew Api { get; private set; }

        public Permit(
            string token,
            string pdp = DEFAULT_PDP_URL,
            string apiUrl = DEFAULT_API_URL,
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

            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger logger = loggerFactory.CreateLogger<Permit>();

            this.Enforcer = new Enforcer(this.Config, this.Config.Pdp, logger);
            this.Cache = new Cache(this.Config, this.Config.Pdp, logger);
            this.Api = new APINew(new NewApiConfig
            {
                ApiURL = apiUrl, DebugMode = debugMode, PdpURL = pdp, Token = token
            });


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
