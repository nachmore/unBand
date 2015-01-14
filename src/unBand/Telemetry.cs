using Garlic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace unBand
{
    public enum TelemetryCategory
    {
        General,
        Theme,
        Export,
    }

    public static class Telemetry
    {
        private static AnalyticsSession _session;
        private static IAnalyticsPageViewRequest _client;

        static Telemetry()
        {
            Init();
        }

        private static void Init()
        {
            _session = new AnalyticsSession("http://unband.nachmore.com/app", "UA-56820272-3");

            var props = Settings.Current;

            _client = _session.CreatePageViewRequest("/", "Global");

            if (props.FirstRun)
            {
                TrackEvent(TelemetryCategory.General, "Install", props.Device);

                // TODO: would be nice to fire a new OS event on system upgrade
                TrackEvent(TelemetryCategory.General, "OS", GetOS());
            }
            
        }

        public static void TrackEvent(TelemetryCategory category, string action, object label = null, int value = 0)
        {
            _client.SendEvent(category.ToString(), action, (label != null ? label.ToString() : null), value.ToString());
        }

        internal static void TrackException(Exception exception)
        {
            // The exception will be truncated to 500bytes (GA limit for Labels), at some point it may be better to extract more pertinant information
            TrackEvent(TelemetryCategory.General, TelemetryEvent.Exception, exception.ToString());
        }

        private static string GetOS()
        {
            return Environment.OSVersion.VersionString + 
                " (" + GetFriendlyOS() + ")" +
                " (" + (Environment.Is64BitOperatingSystem ? "x64" : "x86") + ")";
        }

        private static string GetFriendlyOS()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
                                  select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        /// <summary>
        /// Poor mans enum -> expanded string.
        /// 
        /// Once I've been using this for a while I may change this to a pure enum if 
        /// spaces in names prove to be annoying for querying / sorting the data
        /// </summary>
        public static class TelemetryEvent
        {
            public const string Exception = "Exception";

            public const string AppLaunch = "Launch";
            public const string DeclinedFirstRunWarning = "Declined First Run Warning";
            public const string DeclinedTelemetry = "Declined Telemetry";

            public const string ChangeBackground = "Change Background";
            public const string ChangeThemeColor = "Change Theme Color";

            public static class Export
            {
                public const string Summary = "Export Summary";
                public const string Full = "Export Full";
                public const string FullCancelled = "Full Export Cancelled";
            }
        }
    }
}
