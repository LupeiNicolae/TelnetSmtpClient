namespace SI_telnet
{
    public class GoogleAuthenticationCredentials : AuthenticationCredentials
    {
        private string UserName { get; }
        private string Password { get; }

        public GoogleAuthenticationCredentials(string userName, string password)
        {
            UserName = Base64Encode(userName);
            Password = Base64Encode(password);
        }

        public GoogleAuthenticationCredentials(string userName, string base64Password, bool isBase64)
        {
            UserName = Base64Encode(userName);
            Password = isBase64 ? base64Password : Base64Encode(base64Password);
        }

        public override (string userName, string password) GetCredentials()
        {
            return (UserName, Password);
        }
    }
}