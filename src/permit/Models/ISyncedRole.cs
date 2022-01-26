using System.Collections.Generic;

namespace PermitSDK.Models
{
    public interface ISyncedRole
    {
        string id { get; set; }
        string name { get; set; }
        string tenantId { get; set; }
        Dictionary<string, object> metadata { get; set; }
        string[] permissions { get; set; }
    }
}
