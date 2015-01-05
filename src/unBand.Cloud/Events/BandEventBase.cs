using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public delegate void EventDataDownloadedEventHandler(BandEventBase bandEvent);

    public abstract class BandEventBase
    {
        public event EventDataDownloadedEventHandler EventDataDownloaded;

        public abstract BandEventExpandType[] Expanders { get; }

        // calculated properties

        public string DisplayName
        {
            get
            {
                return Name ?? FriendlyEventType;
            }
        }

        public virtual string FriendlyEventType
        {
            get
            {
                System.Diagnostics.Debug.Assert(false, "This should never hit");
                return "Unknown";
            }
        }

        public virtual string PrimaryMetric
        {
            get
            {
                System.Diagnostics.Debug.Assert(false, "This should never hit");
                return "N/A";
            }
        }

        // common Properties
        public string EventID { get; set; }
        public string Duration { get; set; }
        public string ParentEventId { get; set; }
        public string Name { get; set; }
        public int DeliveryID { get; set; }
        public BandEventType EventType { get; set; }
        public DateTime StartTime { get; set; }
        public int CaloriesBurned { get; set; }
        public DateTime DayId { get; set; }
        public FeelingType Feeling { get; set; }
        public HeartRateSummary HeartRate { get; set; }
        public int Flags { get; set; } // unclear what this means
            
        public BandEventBase(JObject json)
        {
            dynamic eventSummary = (dynamic)json;

            EventID        = eventSummary.EventId;
            Duration       = eventSummary.Duration;
            ParentEventId  = eventSummary.ParentEventId;
            Name           = eventSummary.Name;
            DeliveryID     = eventSummary.DeliveryID;
            EventType      = eventSummary.EventType;
            StartTime      = eventSummary.StartTime;
            CaloriesBurned = eventSummary.CaloriesBurned;
            DayId          = eventSummary.DayId;
            Feeling        = eventSummary.Feeling;
            HeartRate      = new HeartRateSummary((int)eventSummary.AverageHeartRate, (int)eventSummary.LowestHeartRate, (int)eventSummary.PeakHeartRate);
            Flags          = eventSummary.Flags;
        }

        private static Dictionary<string, Type> _knownEventTypes = new Dictionary<string, Type>()
        {
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.SleepEventDTO", typeof(SleepEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserRunEventDTO", typeof(RunEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserGuidedWorkoutEventDTO", typeof(UserGuidedWorkoutEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserWorkoutEventDTO", typeof(UserWorkoutEvent)}
        };

        internal static BandEventBase FromDynamic(dynamic rawBandEvent)
        {
            // annoying, but couldn't find a nicer way to get a property that has a .
            string type = ((dynamic)((JObject)rawBandEvent).GetValue("odata.type")).Value;

            if (!_knownEventTypes.ContainsKey(type))
            {
                throw new KeyNotFoundException("Unknown event type: " + type);
            }
            
            var converter = TypeDescriptor.GetConverter(_knownEventTypes[type]);
            return converter.ConvertFrom(rawBandEvent);
        }

        public abstract void InitFullEventData(JObject json);

        /// <summary>
        /// Returns a Dictionary with the raw field-value for this object
        /// 
        /// Useful when you want to dump this to a file.
        /// </summary>
        public virtual Dictionary<string, object> DumpBasicEventData()
        {
            return new Dictionary<string, object>()
            {
                {"EventID", EventID},
                {"Duration", Duration},
                {"Parent Event Id", ParentEventId},
                {"Name", Name},
                {"DeliveryID", DeliveryID},
                {"EventType", EventType},
                {"StartTime", StartTime},
                {"CaloriesBurned", CaloriesBurned},
                {"DayId", DayId},
                {"Feeling", Feeling},
                {"Average Heart Rate", HeartRate.Average},
                {"Lowest Heart Rate", HeartRate.Lowest},
                {"Peak Heart Rate", HeartRate.Peak},
            };
        }

        public virtual Dictionary<string, string> DumpFullEventData()
        {
            return null;
        }

    }
}
