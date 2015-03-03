using Microsoft.Live;
using Microsoft.Live.Desktop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    // TODO: there should be some kind of way to convey login erros
    public delegate void BandCloudAuthComplete();

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
        private const string EVENTS_URL = "v1/Events";
        private const string EVENTS_TOP_100_URL = "v1/Events(eventType='None')?$top={0}";
        private const string GET_EVENTS_URL = "v1/Events(eventType='None')?";
        private const string GET_EVENT_URL = "v1/Events(EventId='{0}',selectedSplitDistance=100000)?$expand={1}";
        private const string GET_USER_ACTIVITY_URL = "v1/UserActivities(period='h')?";

        private const int SLEEP_BETWEEN_REQUESTS = 11 * 1000;
        private const int SLEEP_AFTER_WEBEXCEPTION = 30 * 1000;
        private DateTime _lastRequest = DateTime.MinValue;

        private LiveAuthTokens _tokens;
        private BandCloudAuthentication _cloudAuthentication;

        public event BandCloudAuthComplete AuthenticationCompleted;

        private LiveAuthWindow _loginWindow;

        public BandCloudClient() { }

        // TODO: REMOVE Login() / Logout() / OnLiveAuthCompleted() / etc. from this library, as they should not take a dependency
        //       specifically on the Desktop Live ID library
        public void Login()
        {
            if (_loginWindow != null)
            {
                _loginWindow.Close();
            }

            var liveAuthClient = new LiveAuthClient("000000004811DB42");

            string startUrl = liveAuthClient.GetLoginUrl(new List<string>() { "service::prodkds.dns-cargo.com::MBI_SSL" });

            _loginWindow = new LiveAuthWindow(
                startUrl,
                this.OnLiveAuthCompleted,
                "Login to the Microsoft Account associated with your Band"
            );

            _loginWindow.Login();
        }

        public void Logout()
        {
            var liveAuthClient = new LiveAuthClient("000000004811DB42");

            string logoutUrl = liveAuthClient.GetLogoutUrl();

            var authForm = new LiveAuthWindow(
                logoutUrl,
                null,
                "Logout"
            );

            authForm.LogOut(logoutUrl);
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

            if (AuthenticationCompleted != null)
                AuthenticationCompleted();
        }

        private string GenerateEventsQuery(int? topCount = null, DateTime? startDate = null, DateTime? endDate = null, string timeField = "StartTime")
        {
            var query = new ODataQuery();

            if (topCount != null)
                query.TopItemCount = (int)topCount;

            if (startDate != null)
            {
                if (endDate != null)
                {
                    ODataOperator op1, op2;

                    if (startDate > endDate)
                    {
                        op1 = ODataOperator.le;
                        op2 = ODataOperator.ge;
                    }
                    else
                    {
                        op1 = ODataOperator.ge;
                        op2 = ODataOperator.le;
                    }

                    query.AddFilter(timeField, op1, (DateTime)startDate);
                    query.AddFilter(timeField, op2, (DateTime)endDate);
                }
                else // only StartDate is specified
                {
                    query.AddFilter(timeField, ODataOperator.le, (DateTime)startDate);
                }
            }

            return query.GenerateQuery();
        }

        public async Task<List<BandEventBase>> GetUserActivity(int? topCount = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = GET_USER_ACTIVITY_URL + GenerateEventsQuery(topCount, startDate, endDate, "TimeOfDay");
            var response = await AuthenticatedRequest(url);

            var rv = new List<BandEventBase>();

            dynamic json = JObject.Parse(response);

            var curDay = DateTime.MinValue;
            UserDailyActivity curActivity = null;

            foreach (var rawUserActivity in json.value)
            {
                if (curDay.Date != rawUserActivity.TimeOfDay.Value.Date)
                {
                    if (curActivity != null)
                    {
                        rv.Add(curActivity);
                    }

                    curActivity = UserDailyActivity.Create(rawUserActivity);

                    curDay = rawUserActivity.TimeOfDay.Value;
                }
                else
                {
                    curActivity.AddSegment(rawUserActivity);
                }
            }

            return rv;
        }

        public async Task<List<BandEventBase>> GetEvents(int? topCount = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var url = GET_EVENTS_URL + GenerateEventsQuery(topCount, startDate, endDate);
            var response = await AuthenticatedRequest(url);

            var rv = new List<BandEventBase>();

            dynamic json = JObject.Parse(response);

            foreach (var rawBandEvent in json.value)
            {
                var bandEvent = BandEventBase.FromDynamic(rawBandEvent);
                rv.Add(bandEvent);
            }

            return rv;
        }

        public async Task<JObject> GetFullEventData(string ID, BandEventExpandType[] expanders)
        {
            var expandParam = string.Join(",", expanders);

            var response = await AuthenticatedRequest(string.Format(GET_EVENT_URL, ID, expandParam));

            return JObject.Parse(response);
        }

        private async Task<string> AuthenticatedRequest(string relativeUrl, int retries = 3)
        {
            string rv = null;

            string url = _cloudAuthentication.EndPoint + relativeUrl;
            var request = HttpWebRequest.Create(url);
            request.Headers.Add("Authorization", _cloudAuthentication.AuthorizationHeader);

            try
            {
                using (var response = await request.GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var responseText = await reader.ReadToEndAsync();

                            rv = responseText;
                        }
                    }
                }

                if (_lastRequest.Subtract(DateTime.Now).TotalSeconds < SLEEP_BETWEEN_REQUESTS)
                {
                    // TODO: does this have to be globally thread safe with a lock?
                    Thread.Sleep(SLEEP_BETWEEN_REQUESTS);
                }

                _lastRequest = DateTime.Now;
            }
            catch (WebException e)
            {
                // there are multiple reasons to end up here, for now it doesn't matter which ones they are, just retry
                if (retries == 1)
                    throw;
            }

            // an exception occurred. Here's to the next version of C# which allows await in catch blocks
            if (rv == null)
            {
                await Task.Delay(SLEEP_AFTER_WEBEXCEPTION);
                return await AuthenticatedRequest(relativeUrl, retries - 1);
            }

            // TODO: exceptions etc.
            return rv;
        }
    }

    public class BandCloudAuthentication
    {
        public Guid RegisteredUserId { get; set; }
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
