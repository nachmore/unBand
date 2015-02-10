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

        public override List<IEventExporter> Exporters { get { return new List<IEventExporter>() { UserDailyActivityToCSVExporter.Instance }; } }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { }; } // nothing to expand for this type
        }

        public List<BandEventBase> Segments { get; private set; }

        public override string FriendlyEventType { get { return "Daily User Activity"; } }

        public override string PrimaryMetric { get { return StepsTaken + " steps";  } }

        public string DayClassification { get; set; }
        public string ActivityLevel { get; set; }
        public int StepsTaken { get; set; }
        public int UvExposure { get; set; }
        public int TotalDistance { get; set; }
        public int ItCal { get; set; }

        private int _aggregateAverageHeartRate = 0;
        private int _averageHeartRateSampleCount = 0;

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
        /// which we then treat a little different (in other words, call Create() to toplevel)
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
            DayId = StartTime; // TODO: we could 0 this out to be 0:00 UTC based like other activities?
            ItCal = eventSummary.ItCal;

            // Location is always null, so ignore for now

            EventType = BandEventType.UserDailyActivity;
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

            if (HeartRate.Lowest == 0 || HeartRate.Lowest > segment.HeartRate.Lowest)
                HeartRate.Lowest = segment.HeartRate.Lowest;

            if (HeartRate.Peak < segment.HeartRate.Peak)
                HeartRate.Peak = segment.HeartRate.Peak;

            // calculate average heart rate, ignoring segments that have no value
            if (segment.HeartRate.Average != 0)
            {
                _averageHeartRateSampleCount++;
                _aggregateAverageHeartRate += segment.HeartRate.Average;

                HeartRate.Average = _aggregateAverageHeartRate / _averageHeartRateSampleCount;
            }
        }

        public override Dictionary<string, object> DumpBasicEventData()
        {
            var rv = new Dictionary<string, object>(base.DumpBasicEventData());

            rv.Add("Steps Taken", StepsTaken);
            rv.Add("UV Exposure", UvExposure);
            rv.Add("Total Distance (cm)", TotalDistance);

            return rv;
        }

        public override void InitFullEventData(JObject json)
        {
        }
    }
}
