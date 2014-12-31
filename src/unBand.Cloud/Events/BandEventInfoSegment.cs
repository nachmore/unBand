using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public class BandEventInfoSegment
    {
        public DateTime TimeOfDay { get; set; }
        public string DayClassification { get; set; }
        public string ActivityLevel { get; set; }
        public int StepsTaken { get; set; }
        public int CaloriesBurned { get; set; }
        public int UvExposure { get; set; }
        public object Location { get; set; }
        public int PeakHeartRate { get; set; }
        public int LowestHeartRate { get; set; }
        public int AverageHeartRate { get; set; }
        public string TotalDistance { get; set; } // unclear why this is a string, but it is defined as a string in the JSON
        public int ItCal { get; set; }

        public BandEventInfoSegment(JObject jsonEvent)
        {
            dynamic rawEvent = (dynamic)jsonEvent;

            TimeOfDay         = TimeZone.CurrentTimeZone.ToLocalTime(rawEvent.TimeOfDay.Value);
            DayClassification = rawEvent.DayClassification;
            ActivityLevel     = rawEvent.ActivityLevel;
            StepsTaken        = rawEvent.StepsTaken;
            CaloriesBurned    = rawEvent.CaloriesBurned;
            UvExposure        = rawEvent.UvExposure;
            Location          = rawEvent.Location; // always seem to be null? If this ever shows up, will have to parse it properly
            PeakHeartRate     = rawEvent.PeakHeartRate;
            LowestHeartRate   = rawEvent.LowestHeartRate;
            AverageHeartRate  = rawEvent.AverageHeartRate;
            TotalDistance     = rawEvent.TotalDistance;
            ItCal             = rawEvent.ItCal;
        }
    }
}
