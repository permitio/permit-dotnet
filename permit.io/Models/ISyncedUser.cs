using System.Collections.Generic;

namespace permit.io.Models
{
    public interface ISyncedUser
    {
        string id { get; set; }
        string name { get; set; }
        string email { get; set; }
        Dictionary<string, object> metadata { get; set; }
        ISyncedRole[] Roles { get; set; }
    }
}