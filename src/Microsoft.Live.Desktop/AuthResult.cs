using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Live.Desktop
{
    public class AuthResult
    {
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public string TokenType { get; private set; }
        public string UserID { get; private set; }
        public string Scope { get; private set; }
        public DateTime Expires { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }

        public AuthResult(Uri resultUri)
        {
            // token results come back at the end of a fragment (starts with "#")
            string fullParams = resultUri.Query.TrimStart('?') + resultUri.Fragment.Replace('#', '&');

            string[] queryParams = fullParams.Split('&');
            foreach (string param in queryParams)
            {
                string[] kvp = param.Split('=');
                switch (kvp[0])
                {
                    case "access_token":
                        this.AccessToken = Uri.UnescapeDataString(kvp[1]);
                        break;
                    case "refresh_token":
                        this.RefreshToken = Uri.UnescapeDataString(kvp[1]);
                        break;
                    case "token_type":
                        this.TokenType = kvp[1];
                        break;
                    case "scope":
                        this.Scope = kvp[1];
                        break;
                    case "user_id":
                        this.UserID = kvp[1];
                        break;
                    case "expires_in":
                        ParseExpires(kvp[1]);
                        break;
                    case "error":
                        this.ErrorCode = kvp[1];
                        break;
                    case "error_description":
                        this.ErrorDescription = Uri.UnescapeDataString(kvp[1]);
                        break;
                }
            }
        }

        private void ParseExpires(string expires)
        {
            int expiresSeconds;

            if (int.TryParse(expires, out expiresSeconds))
            {
                Expires = DateTime.Now.AddSeconds(expiresSeconds);
            }
        }
    }
}
