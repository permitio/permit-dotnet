using System.Collections.Generic;

namespace PermitSDK.Models
{
    public class User
    {
        public string id = "";
        public string firstName = "";
        public string lastName = "";
        public string email = "";
        public RoleAssignment[] roles = null;
        public Dictionary<string, dynamic> attributes = null;

        public User(
            string email,
            string firstName = "",
            string lastName = "",
            RoleAssignment[] roles = null,
            Dictionary<string, dynamic> attributes = null
        )
        {
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.roles = roles;
            this.attributes = attributes;
        }

        private User() { }
    }


}
