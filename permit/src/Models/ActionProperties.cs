using System.Collections.Generic;

namespace Permit.Models
{
    public class ActionProperties
    {
        public string title;
        public string description;
        public string path; //todo can I remove this?
        public Dictionary<string, string> attributes;

        public ActionProperties() { }
    }
}
