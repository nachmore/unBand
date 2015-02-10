using Newtonsoft.Json;
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
    // This class is unique in that doesn't have a TypeConverter, since it comes from its own Cloud request, so
    // we don't need to guess the type dynamically
    public class UserDailyActivity : BandEventBase
    {
        private static List<IEventExporter> _exporters;

        public override List<IEventExporter> Exporters
        {
            get
            {
                if (_exporters == null)
                {
                    _exporters = new List<IEventExporter>() { };
                    _exporters.AddRange(base.Exporters);
                }

                return _exporters;
            }
        }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<BandEventBase> Segments { get; private set; }

        public override string FriendlyEventType { get { return "Daily User Activity"; } }

        public override string PrimaryMetric { get { return StepsTaken + " steps";  } }

        public string DayClassification { get; set; }
        public string ActivityLevel { get; set; }
        public int StepsTaken { get; set; }
        public int UvExposure { get; set; }
        public int TotalDistance { get; set; }

        /// <summary>
        /// Returns a UserActivity object that tracks a User's activity across a given day
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static UserDailyActivity Create(JObject json)
        {
            var activity = new UserDailyActivity(json);

            activity.Segments = new List<BandEventBase>();

            // add this as the first segment of the day
            activity.AddSegment(json);

            return activity;
        }

        /// <summary>
        /// Called to indicate that this is the constructor for a UserActivity that will be part of a segment, not a top level
        /// which we then treat a little different
        /// </summary>
        /// <param name="json"></param>
        /// <param name="toplevel"></param>
        private UserDailyActivity(JObject json) : base()
        {
            dynamic eventSummary = (dynamic)json;

            StartTime = eventSummary.TimeOfDay;
            DayClassification = eventSummary.DayClassification;
            ActivityLevel = eventSummary.ActivityLevel;
            StepsTaken = eventSummary.StepsTaken;
            HeartRate = new HeartRateSummary((int)eventSummary.AverageHeartRate, (int)eventSummary.LowestHeartRate, (int)eventSummary.PeakHeartRate);
            CaloriesBurned = eventSummary.CaloriesBurned;
            UvExposure = eventSummary.UvExposure;
            TotalDistance = eventSummary.TotalDistance;
            
            // Location is always null, so ignore for now
            // ItCal always seems to be 0?

            EventType = BandEventType.UserActivity;
        }

        public void AddSegment(JObject json)
        {
            // 1. Add the segment as a new UserActivity segment to this one
            // 2. Add the values to the running tally for the day

            var segment = new UserDailyActivity(json);

            Segments.Add(segment);

            // add in the values that need to be totalled over the lifetime of the day
            // Note: this means that asking for values such as UvExposure or HeartRate
            //       at the day level will return a value (the value for the first hour)
            //       but don't really make much sense
            StepsTaken += segment.StepsTaken;
            TotalDistance += segment.TotalDistance;
            CaloriesBurned += segment.CaloriesBurned;
        }

        public override Dictionary<string, object> DumpBasicEventData()
        {
            return base.DumpBasicEventData();
        }

        public override void InitFullEventData(JObject json)
        {
        }
    }
}
