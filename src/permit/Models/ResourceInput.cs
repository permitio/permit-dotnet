using System.Collections.Generic;

namespace PermitSDK.Models
{
    public class ResourceInput
    {
        public string type { get; set; }
        public string key { get; set; }
        public string tenant { get; set; }
        public Dictionary<string, string> attributes { get; set; }
        public Dictionary<string, string> context { get; set; }

        public ResourceInput(
            string type,
            string key = null,
            string tenant = null,
            Dictionary<string, string> attributes = null,
            Dictionary<string, string> context = null
        )
        {
            this.type = type;
            this.key = key;
            this.tenant = tenant;
            this.attributes = attributes;
            this.context = context;
        }

        static public ResourceInput ResourceFromString(string strResource)
        {
            var resourceParts = strResource.Split(':');
            if (resourceParts.Length == 0 || resourceParts.Length > 2)
            {
                throw new CreateResourceException(
                    "Resource string should be in <type>:<optional:id> format"
                );
            }
            return new ResourceInput(
                resourceParts[0],
                resourceParts.Length == 2 ? resourceParts[1] : null
            );
        }

        static public ResourceInput Normalize(ResourceInput resource, Config config)
        {
            ResourceInput normalizedResource = new ResourceInput(
                resource.type,
                resource.key,
                resource.tenant,
                resource.attributes,
                resource.context
            );
            if (resource.tenant == null && config.UseDefaultTenantIfEmpty)
            {
                normalizedResource.tenant = config.DefaultTenant;
            }
            if (normalizedResource.tenant != null)
            {
                if (normalizedResource.context == null)
                {
                    normalizedResource.context = new Dictionary<string, string>()
                    {
                        { "tenant", normalizedResource.tenant }
                    };
                }
            }

            return normalizedResource;
        }
    }
}
