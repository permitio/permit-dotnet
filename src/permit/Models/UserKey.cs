using System.Collections.Generic;

namespace PermitDotnet.Models
{
    public interface IUserKey
    {
        public string key { get; }
    }

    public class UserKey : IUserKey, IUser
    {
        public string key { get; set; }
        public string customId { get; set; }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public Dictionary<string, string> attributes { get; set; }
        public ISyncedRole[] Roles { get; set; }

        public UserKey(
            string key,
            string firstName = "",
            string lastName = "",
            string email = "",
            Dictionary<string, string> attributes = null,
            SyncedRole[] roles = null
        )
        {
            this.key = key;
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.attributes = attributes;
            this.Roles = roles;
        }

        private UserKey() { }


    }
}
