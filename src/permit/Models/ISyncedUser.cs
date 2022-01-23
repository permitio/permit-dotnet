using System.Collections.Generic;

namespace Permit.Models
{
    public interface ISyncedUser
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public Dictionary<string, string> metadata { get; set; }
        public ISyncedRole[] Roles { get; set; }
    }

    public interface IUser
    {
        string key { get; set; }
        string firstName { get; set; }
        string lastName { get; set; }
        string email { get; set; }
        Dictionary<string, string> attributes { get; set; }
        ISyncedRole[] Roles { get; set; }
    }
}
