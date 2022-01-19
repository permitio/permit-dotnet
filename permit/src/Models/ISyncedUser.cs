using System.Collections.Generic;

namespace Permit.Models
{
    public interface ISyncedUser
    {
        string id { get; set; }
        string name { get; set; }
        string email { get; set; }
        Dictionary<string, object> metadata { get; set; }
        ISyncedRole[] Roles { get; set; }
    }

    public interface IUser
    {
        string key { get; set; }
        string firstName { get; set; }
        string lastName { get; set; }
        string email { get; set; }
        Dictionary<string, object> attributes { get; set; }
        ISyncedRole[] Roles { get; set; }
    }
}