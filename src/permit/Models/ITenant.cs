namespace Permit.Models
{
    public interface ITenant
    {
        public string externalId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}
