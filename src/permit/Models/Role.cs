namespace PermitSDK.Models
{
    public class Role
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string tenantId { get; set; }

        public Role(string name, string description, string tenantId = null)
        {
            this.name = name;
            this.tenantId = tenantId;
            this.description = description;
        }

        public Role() { }
    }
}
