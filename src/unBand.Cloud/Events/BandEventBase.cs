using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unBand.Cloud.Exporters.EventExporters;

namespace unBand.Cloud
{
    public abstract class EventBaseSequenceItem
    {
        public long SequenceId { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public int CaloriesBurned { get; internal set; }
        public int Order { get; internal set; }
        public HeartRateSummary HeartRate { get; internal set; }
        public int Duration { get; internal set; }

        public EventBaseSequenceItem(JObject json)
        {
            dynamic rawSequence = (dynamic)json;

            SequenceId = rawSequence.SequenceId;
            StartTime = rawSequence.StartTime; // TODO: DateTime needs to be adjusted per export settings
            CaloriesBurned = rawSequence.CaloriesBurned;
            Duration = rawSequence.Duration;
            Order = rawSequence.Order;

            HeartRate = new HeartRateSummary((int)rawSequence.AverageHeartRate, (int)rawSequence.LowestHeartRate, (int)rawSequence.PeakHeartRate);
        }
    }

    public abstract class BandEventBase
    {
        public virtual List<IEventExporter> Exporters { get { return new List<IEventExporter>() { EventInfoToCSVExporter.Instance }; } }

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
        public List<BandEventInfoItem> InfoSegments { get; private set; }
        public List<EventBaseSequenceItem> Sequences { get; private set; }

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

        /// <summary>
        /// Used by special event types that still want to play in the same collections but
        /// need special handling. Any event using this constructor will have to do all of its
        /// own initial parsing
        /// </summary>
        internal BandEventBase()
        {
        }
            
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

            InfoSegments = new List<BandEventInfoItem>();
            Sequences = new List<EventBaseSequenceItem>();
        }

        private static Dictionary<string, Type> _knownEventTypes = new Dictionary<string, Type>()
        {
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.SleepEventDTO", typeof(SleepEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserRunEventDTO", typeof(RunEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserGuidedWorkoutEventDTO", typeof(UserGuidedWorkoutEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserWorkoutEventDTO", typeof(UserWorkoutEvent)},
            {"Microsoft.Khronos.Cloud.Ods.Data.Entities.UserBikeEventDTO", typeof(BikeEvent)}
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

        public virtual void InitFullEventData(JObject json)
        {
            dynamic fullEvent = (dynamic)json;

            // parse out the "Info" section
            foreach (dynamic infoData in fullEvent.value[0].Info)
            {
                InfoSegments.Add(new BandEventInfoItem(infoData));
            }
        }

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
