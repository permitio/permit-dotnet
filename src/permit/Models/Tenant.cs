namespace Permit.Models
{
    public class Tenant : ITenant
    {
        public string id { get; set; }
        public string externalId { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public Tenant(string externalId, string name, string description = null)
        {
            this.externalId = externalId;
            this.name = name;
            this.description = description;
        }

        private Tenant() { }
    }
}
