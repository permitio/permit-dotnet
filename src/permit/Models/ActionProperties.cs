using System.Collections.Generic;

namespace PermitSDK.Models
{
    public class ActionProperties
    {
        public string title { get; set; }
        public string description { get; set; }
        public string path { get; set; } //todo can I remove this?
        public Dictionary<string, string> attributes { get; set; }

        public ActionProperties(
            string title,
            string description = "",
            string path = "",
            Dictionary<string, string> attributes = null
        )
        {
            this.title = title;
            this.description = description;
            this.path = path;
            this.attributes = attributes;
        }

        public ActionProperties() { }
    }
}
