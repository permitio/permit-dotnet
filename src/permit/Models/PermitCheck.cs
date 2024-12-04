using System.Collections.Generic;

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

    public class CheckQueryObj
    {
        public IUserKey user { get; set; }
        public string action { get; set; }
        public ResourceInput resource { get; set; }
        public Dictionary<string, string> context { get; set; }

        public CheckQueryObj(IUserKey user, string action, ResourceInput resource, Dictionary<string, string> context)
        {
            this.user = user;
            this.action = action;
            this.resource = resource;
            this.context = context;
        }

        public CheckQueryObj() { }
    }

    public class CheckQuery
    {
        public string user { get; set; }
        public string action { get; set; }
        public string resource { get; set; }
        public Dictionary<string, string> context { get; set; }

        public CheckQuery(string user, string action, string resource, Dictionary<string, string> context)
        {
            this.user = user;
            this.action = action;
            this.resource = resource;
            this.context = context;
        }

        public CheckQuery() { }
    }

    public class CheckQueryResult
    {
        public CheckQueryObj Query { get; set; }
        public bool Result { get; set; }

        public CheckQueryResult(CheckQueryObj query, bool result)
        {
            this.Query = query;
            this.Result = result;
        }
    }


    public class BulkPolicyDecision
    {
        public List<PolicyDecision> allow { get; set; }

        public BulkPolicyDecision(List<PolicyDecision> allow)
        {
            this.allow = allow;
        }

        public BulkPolicyDecision() { }
    }

    public class PolicyDecision
    {
        public bool allow { get; set; }

        public PolicyDecision(bool allow)
        {
            this.allow = allow;
        }

        public PolicyDecision() { }


    }
}


