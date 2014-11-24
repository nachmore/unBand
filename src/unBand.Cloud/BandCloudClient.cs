using Microsoft.Live;
using Microsoft.Live.Desktop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    // TODO: there should be some kind of way to convey login erros
    public delegate void BandCloudAuthComplete(LiveAuthTokens tokens);

    public struct LiveAuthTokens
    {
        public string AccessToken { get; internal set; }
        public string RefreshToken { get; internal set; }
        public DateTime Expires { get; internal set; }
    }

    public class BandCloudClient
    {
        private const string LOGIN_HOST_URL = "https://prodkds.dns-cargo.com";
        private const string LOGIN_URL = "/api/v1/user";
        private const string EVENTS_URL = "/v1/Events";
        private const string EVENTS_TOP_100_URL = "/v1/Events(eventType='None')?$top=100";

        private LiveAuthTokens _tokens;
        private BandCloudAuthentication _cloudAuthentication;

        public BandCloudClient() { }

        public BandCloudClient(LiveAuthTokens tokens)
        {
            _tokens = tokens;
        }

        public void Login()
        {
            var liveAuthClient = new LiveAuthClient("000000004811DB42");

            string startUrl = liveAuthClient.GetLoginUrl(new List<string>() { "service::prodkds.dns-cargo.com::MBI_SSL" });

            var authForm = new LiveAuthWindow(
                startUrl,
                this.OnLiveAuthCompleted);

            authForm.Show();
        }

        private async void OnLiveAuthCompleted(AuthResult result)
        {
            _tokens = new LiveAuthTokens() 
            { 
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                Expires = result.Expires
            };

            var req = HttpWebRequest.Create(LOGIN_HOST_URL + LOGIN_URL);
            req.Headers.Add("Authorization", _tokens.AccessToken);
            var response = await req.GetResponseAsync();
            var stream = response.GetResponseStream();

            using (var reader = new StreamReader(stream))
            {
                var responseText = await reader.ReadToEndAsync();
                System.Diagnostics.Debug.WriteLine(responseText);

                _cloudAuthentication = JsonConvert.DeserializeObject<BandCloudAuthentication>(responseText);

                _cloudAuthentication.AuthorizationHeader = response.Headers["Authorization"];
            }

            GetEvents();
        }

        public async void GetEvents()
        {
            var response = await AuthenticatedRequest(EVENTS_TOP_100_URL);
        }

        private async Task<string> AuthenticatedRequest(string relativeUrl)
        {
            var request = HttpWebRequest.Create(_cloudAuthentication.EndPoint + relativeUrl);
            request.Headers.Add("Authorization", _cloudAuthentication.AuthorizationHeader);

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream)) {
                        var responseText = await reader.ReadToEndAsync();

                        return responseText;
                    }
                }
            }

            // TODO: exceptions etc.
            return null;
        }
    }

    public class BandCloudAuthentication
    {
        public Guid RegisteredUserId {get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }        
        public DateTime LastUserUpdateOn { get; set; }
        public string EndPoint { get; set; }
        public string FUSEndPoint { get; set; }
        public string HnFEndPoint { get; set; }
        public string HnFQueryParameters { get; set; }
        public string HnFAutoSuggestEndpoint { get; set; }
        public string HnFAutoSuggestQueryParameters { get; set; }
        public Guid ODSUserID { get; set; }
        public Guid LFSUserID { get; set; }
        public Guid ContainerName { get; set; }

        // There is also: 
        // "ApplicationSettings":{"ApplicationId":"00000000-0000-0000-0000-000000000000"}}
        // which we don't use for anything, so we don't parse it out

        public string AuthorizationHeader { get; set; }
    }
}
