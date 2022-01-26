using System.Collections.Generic;

namespace PermitDotnet.Models
{
    public class SyncedUser
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public Dictionary<string, string> metadata { get; set; }
        public SyncedRole[] Roles { get; set; }

        public SyncedUser(
            string id,
            string name = "",
            string email = "",
            SyncedRole[] roles = null,
            Dictionary<string, string> metadata = null
        )
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.Roles = roles;
            this.metadata = metadata;
        }

        private SyncedUser() { }
    }



}
