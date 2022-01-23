namespace Permit.Models
{
    public class Permissions
    {
        public string id { get; set; }
        public string action { get; set; }
        public string scope { get; set; }
        public string actionId { get; set; }
        public string resourceId { get; set; }

        public Permissions(string action, string scope, string actionId, string resourceId)
        {
            this.action = action;
            this.scope = scope;
            this.actionId = actionId;
            this.resourceId = resourceId;
        }

        private Permissions() { }
    }
}
