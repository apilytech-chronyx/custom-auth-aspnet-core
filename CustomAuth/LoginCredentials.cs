namespace CustomAuth
{
    // Model to deserialize login credentials from request body
    public class LoginCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
