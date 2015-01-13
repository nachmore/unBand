using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public class BandEventInfoItem
    {
        public DateTime TimeOfDay { get; set; }
        public string DayClassification { get; set; }
        public string ActivityLevel { get; set; }
        public int StepsTaken { get; set; }
        public int CaloriesBurned { get; set; }
        public int UvExposure { get; set; }
        public object Location { get; set; }
        public HeartRateSummary HeartRate { get; set; }
        public string TotalDistance { get; set; } // unclear why this is a string, but it is defined as a string in the JSON
        public int ItCal { get; set; }

        public BandEventInfoItem(JObject jsonEvent)
        {
            dynamic rawEvent = (dynamic)jsonEvent;

            TimeOfDay         = TimeZone.CurrentTimeZone.ToLocalTime(rawEvent.TimeOfDay.Value);
            DayClassification = rawEvent.DayClassification;
            ActivityLevel     = rawEvent.ActivityLevel;
            StepsTaken        = rawEvent.StepsTaken;
            CaloriesBurned    = rawEvent.CaloriesBurned;
            UvExposure        = rawEvent.UvExposure;
            Location          = rawEvent.Location; // always seem to be null? If this ever shows up, will have to parse it properly

            HeartRate = new HeartRateSummary((int)rawEvent.AverageHeartRate, (int)rawEvent.LowestHeartRate, (int)rawEvent.PeakHeartRate);

            TotalDistance     = rawEvent.TotalDistance;
            ItCal             = rawEvent.ItCal;
        }
    }
}
