using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Accela.Shared.AuthLibrary
{
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class ApplicationInfo
    {
        public string applicationId { get; set; }
        public string applicationSecret { get; set; }
        public string applicationType { get; set; }
        public string agencyName { get; set; }
        public string agencyEnvironment { get; set; }
    }

    public class UserProfile
    {
        public string id { get; set; }
        public string loginName { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string countryCode { get; set; }
        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string phoneCountryCode { get; set; }
        public string phoneAreaCode { get; set; }
        public string phoneNumber { get; set; }
        public string avatarUrl { get; set; }
    }

    public class CurrentUserProfile
    {
        public UserProfile userProfile { get; set; }
        public Token token { get; set; }
        public ApplicationInfo applicationInfo { get; set; }
    }
}
