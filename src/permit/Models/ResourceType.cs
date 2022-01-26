using System.Collections.Generic;

namespace PermitDotnet.Models
{
    public class ResourceType
    {
        public string type { get; set; }

        //public string Description;
        //public Dictionary<string, string> attributes;
        public Dictionary<string, ActionProperties> actions { get; set; }

        public ResourceType(string type, Dictionary<string, ActionProperties> actions)
        {
            this.type = type;
            this.actions = actions;
        }

        public ResourceType() { }
    }
}
