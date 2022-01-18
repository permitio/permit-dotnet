namespace permit.io.Models
{
    public interface ITenant
    {
        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}