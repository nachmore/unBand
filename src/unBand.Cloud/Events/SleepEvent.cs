using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public class SleepInfoSegment
    {
        public DateTime TimeOfDay { get; set;}
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

        internal static SleepInfoSegment FromDynamic(dynamic rawEvent)
        {
            var rv = new SleepInfoSegment();

            rv.TimeOfDay = TimeZone.CurrentTimeZone.ToLocalTime(rawEvent.TimeOfDay.Value);
            rv.DayClassification = rawEvent.DayClassification;
            rv.ActivityLevel = rawEvent.ActivityLevel;
            rv.StepsTaken = rawEvent.StepsTaken;
            rv.CaloriesBurned = rawEvent.CaloriesBurned;
            rv.UvExposure = rawEvent.UvExposure;
            rv.Location = rawEvent.Location;
            rv.PeakHeartRate = rawEvent.PeakHeartRate;
            rv.LowestHeartRate = rawEvent.LowestHeartRate;
            rv.AverageHeartRate = rawEvent.AverageHeartRate;
            rv.TotalDistance = rawEvent.TotalDistance;
            rv.ItCal = rawEvent.ItCal;

            return rv;
        }
    }

    public class SleepEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new SleepEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(SleepEventConverter))]
    public class SleepEvent : BandEventBase
    {

        public List<SleepInfoSegment> Segments { get; private set; }

        public SleepEvent(JObject json)
            : base(json)
        {
            Segments = new List<SleepInfoSegment>();
            InitFromDynamic((dynamic)json);
        }

        public override async Task DownloadAllData()
        {

        }

        /// <summary>
        /// Creates a SleepEvent object that is intialized from the summary JSON returned by GetEvents()
        ///  
        /// To fill in detailed information about this event a call to DownloadAllData() is required.
        /// </summary>
        /// <param name="rawEvent"></param>
        /// <returns></returns>
        private void InitFromDynamic(dynamic rawEvent)
        {

        }

        public override string ToCSV()
        {
            throw new NotImplementedException();
        }
    }
}
