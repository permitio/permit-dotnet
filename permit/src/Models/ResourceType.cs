﻿using System.Collections.Generic;

namespace Permit.Models
{
    public class ResourceType
    {
        public string Type;

        //public string Description;
        //public Dictionary<string, string> attributes;
        public Dictionary<string, ActionProperties> actions;

        public ResourceType() { }
    }
}
