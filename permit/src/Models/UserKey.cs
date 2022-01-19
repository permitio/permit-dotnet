namespace Permit.Models
{
    public interface IUserKey
    {
        public string key { get; }
    }

    public class UserKey : IUserKey
    {
        public string key { get; set; }

        public UserKey(string key)
        {
            this.key = key;
        }
    }
}
