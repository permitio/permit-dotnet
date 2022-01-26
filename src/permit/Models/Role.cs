using System.Collections.Generic;

namespace PermitDotnet.Models
{
    public class Role
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string tenantId { get; set; }

        //public Permissions permissions { get; set; }

        public Role(
            string name,
            string description,
            string tenantId = null
        //Permissions permissions = null
        )
        {
            this.name = name;
            this.tenantId = tenantId;
            this.description = description;
            //this.permissions = permissions;
        }

        private Role() { }
    }
}
