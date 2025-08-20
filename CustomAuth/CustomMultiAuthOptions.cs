using Microsoft.AspNetCore.Authentication;

namespace CustomAuth
{
    public class CustomMultiAuthOptions : AuthenticationSchemeOptions
    {
        public string JwtSecret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string UserProfileApiUrl { get; set; } = string.Empty;
        public string? Tenant { get; internal set; }
    }
}
