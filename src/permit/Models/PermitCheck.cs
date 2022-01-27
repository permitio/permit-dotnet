namespace PermitSDK.Models
{
    public class PermitCheckQuery
    {
        public string action { get; set; }
        public ResourceInput resource { get; set; }

        public PermitCheckQuery(string action, ResourceInput resource, User user)
        {
            this.action = action;
            this.resource = resource;
        }

        public PermitCheckQuery() { }
    }

    public class PermitCheck
    {
        public bool allow { get; set; }
        public PermitCheckQuery query { get; set; }

        public PermitCheck(bool allow, PermitCheckQuery query)
        {
            this.allow = allow;
            this.query = query;
        }

        public PermitCheck() { }
    }
}
