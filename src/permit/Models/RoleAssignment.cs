using System;

namespace PermitSDK.Models
{
    public class RoleAssignment
    {
        public string id { get; set; }
        public Role role { get; set; }
        public string user { get; set; }
        public string scope { get; set; }

        public RoleAssignment() { }
    }
}
