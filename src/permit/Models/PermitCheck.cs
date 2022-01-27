namespace PermitSDK.Models
{
    public class PermitCheckQuery
    {
        public string action { get; private set; }
        public ResourceInput resource { get; private set; }
        public User user { get; private set; }

        public PermitCheckQuery(string action, ResourceInput resource, User user)
        {
            this.action = action;
            this.resource = resource;
            this.user = user;
        }

        public PermitCheckQuery() { }
    }

    public class PermitCheck
    {
        public bool allow { get; private set; }
        public PermitCheckQuery query { get; private set; }

        public PermitCheck(bool allow, PermitCheckQuery query)
        {
            this.allow = allow;
            this.query = query;
        }

        public PermitCheck() { }
    }
}
