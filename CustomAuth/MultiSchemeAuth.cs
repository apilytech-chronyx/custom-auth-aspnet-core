using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CustomAuth
{
    public class CustomMultiAuthHandler : AuthenticationHandler<CustomMultiAuthOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public const string SchemeName = "CombinedScheme";

        public CustomMultiAuthHandler(
            IOptionsMonitor<CustomMultiAuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IHttpClientFactory httpClientFactory)
            : base(options, logger, encoder)
        {
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // 1. First, delegate to the standard JWT Bearer handler to validate the token
                // This uses the same configuration and validation logic as the built-in handler
                var jwtResult = await Context.AuthenticateAsync(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme);

                if (!jwtResult.Succeeded)
                {
                    return AuthenticateResult.Fail("JWT validation failed");
                }

                // Get the existing claims from the successful JWT authentication
                var claims = new List<Claim>(jwtResult.Principal.Claims);

                // Extract user identifier from JWT claims
                var servicePrincipleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
                if (servicePrincipleId == null)
                {
                    return AuthenticateResult.Fail("JWT token does not contain required user identifier claim");
                }

                // 2. Read credentials from request body if it's a POST request with content
                string username = null;
                string password = null;

                if (Context.Request.Method == "POST" && Context.Request.ContentLength > 0)
                {
                    // Enable buffering so we can read the body multiple times if needed
                    Context.Request.EnableBuffering();

                    using var reader = new StreamReader(Context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    Context.Request.Body.Position = 0; // Reset position for other middleware

                    try
                    {
                        // Try to deserialize as JSON
                        var credentials = System.Text.Json.JsonSerializer.Deserialize<LoginCredentials>(body);
                        username = credentials?.Username;
                        password = credentials?.Password;

                    }
                    catch
                    {
                        // If not valid JSON, try to parse as form data
                        var form = await Context.Request.ReadFormAsync();
                        username = form["username"].ToString();
                        password = form["password"].ToString();
                    }
                }

                // 2. Call downstream service to get user profile, simulating this now but this is where we would fetch the user profile
                var userProfile = await GetUserProfileByIdAsync(username, password);

                if (userProfile == null)
                {
                    return AuthenticateResult.Fail("Could not retrieve user profile");
                }

                // 3. Add claims from user profile
                claims.AddRange(ConvertProfileToClaims(userProfile));


                // Create final identity with combined claims
                var identity = new ClaimsIdentity(claims, SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception occurred while authenticating");
                return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
            }
        }

        // The ValidateJwtToken method is no longer needed since we delegate to the standard JWT handler


        private async Task<UserProfile> GetUserProfileByIdAsync(string userId, string password)
        {
            //var client = _httpClientFactory.CreateClient("UserProfileApi");

            //// No credentials in request body - using JWT identity instead
            //var request = new HttpRequestMessage(HttpMethod.Get, $"{Options.UserProfileApiUrl}/{userId}");

            //var response = await client.SendAsync(request);
            //if (!response.IsSuccessStatusCode)
            //{
            //    return null;
            //}

            //return await response.Content.ReadFromJsonAsync<UserProfile>();

            var demoProfile = new UserProfile
            {
                UserId = userId,
                FullName = "Demo User",
                Email = "demo@example.com",
                Roles = new List<string> { "User", "Admin" },
                Permissions = new List<string> { "ReadData", "WriteData", "ManageUsers" }
            };

            return await Task.FromResult(demoProfile);
        }

        private List<Claim> ConvertProfileToClaims(UserProfile profile)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, profile.FullName),
                new Claim(ClaimTypes.Email, profile.Email)
            };

            // Add roles
            foreach (var role in profile.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permissions
            foreach (var permission in profile.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            return claims;
        }
    }
}
