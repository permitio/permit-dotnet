using System;
using System.Collections.Generic;

namespace permit.io.Models
{
    public class User
    {
        public string firstName = "";
        public string lastName = "";
        public string email = "";
        public AssignedRole[] roles = null;
        public Dictionary<string, string> attributes = null;

        public User(string email, string firstName = "", string lastName = "", AssignedRole[] roles = null, Dictionary<string, string> attributes = null)
        {
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.roles = roles;
            this.attributes = attributes;
        }

    }


}
