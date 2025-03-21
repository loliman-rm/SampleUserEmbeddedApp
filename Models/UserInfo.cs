using Microsoft.Extensions.Options;

namespace SampleUserEmbeddedApp.Models
{
    public class UserInfo
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CostCenter { get; set; }
        public string Organization { get; set; }

        public string AllowedOrigin { get; set; }

        public UserInfo() { }

        public UserInfo(IOptions<AppSettings> settings)
        {
            AllowedOrigin = settings.Value.AllowedOrigin;
        }
    }
}
