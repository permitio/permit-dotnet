using System.Collections.Generic;

namespace Permit.Models
{
    public class ResourceInput
    {
        public string type { get; set; }
        public string id { get; set; }
        public string tenant { get; set; }
        public Dictionary<string, string> attributes { get; set; }
        public Dictionary<string, string> context { get; set; }

        public ResourceInput(
            string type,
            string id = null,
            string tenant = null,
            Dictionary<string, string> attributes = null,
            Dictionary<string, string> context = null
        )
        {
            this.type = type;
            this.id = id;
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
                resource.id,
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
