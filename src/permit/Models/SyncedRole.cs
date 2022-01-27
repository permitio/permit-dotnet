using System.Collections.Generic;

namespace PermitSDK.Models
{
    public class SyncedRole : ISyncedRole
    {
        public string id { get; set; }
        public string name { get; set; }
        public string tenantId { get; set; }
        public Dictionary<string, object> metadata { get; set; }
        public string[] permissions { get; set; }

        public SyncedRole(
            string id,
            string name,
            string tenantId,
            Dictionary<string, object> metadata,
            string[] permissions
        )
        {
            this.id = id;
            this.name = name;
            this.tenantId = tenantId;
            this.metadata = metadata;
            this.permissions = permissions;
        }

        private SyncedRole() { }
    }
}
