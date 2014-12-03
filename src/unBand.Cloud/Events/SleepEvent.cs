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
        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<SleepInfoSegment> Segments { get; private set; }

        public SleepEvent(JObject json)
            : base(json)
        {
            Segments = new List<SleepInfoSegment>();
            InitBasicEventData((dynamic)json);
        }

        public int AwakeTime { get; private set; }
        public int SleepTime { get; private set; }
        public int NumberOfWakeups { get; private set; }
        public int TimeToFallAsleep { get; private set; }
        public int SleepEfficiencyPercentage { get; private set; }
        public int SleepRecoveryIndex { get; private set; }
        public int RestingHeartRate { get; private set; }

        /// <summary>
        /// Creates a SleepEvent object that is intialized from the summary JSON returned by GetEvents()
        ///  
        /// To fill in detailed information about this event a call to DownloadAllData() is required.
        /// </summary>
        /// <param name="basicData"></param>
        /// <returns></returns>
        private void InitBasicEventData(dynamic basicData)
        {
            AwakeTime                 = basicData.AwakeTime;
            SleepTime                 = basicData.SleepTime;
            NumberOfWakeups           = basicData.NumberOfWakeups;
            TimeToFallAsleep          = basicData.TimeToFallAsleep;
            SleepEfficiencyPercentage = basicData.SleepEfficiencyPercentage;
            SleepRecoveryIndex        = basicData.SleepRecoveryIndex;
            RestingHeartRate          = basicData.RestingHeartRate;
        }

        
        public override Dictionary<string, string> GetRawSummary()
        {
            var rv = new Dictionary<string, string>(base.GetRawSummary());

            rv.Add("Awake Time", AwakeTime.ToString());
            rv.Add("Sleep Time", SleepTime.ToString());
            rv.Add("Number of Wakeups", NumberOfWakeups.ToString());
            rv.Add("Time to Fall Asleep", TimeToFallAsleep.ToString());
            rv.Add("Sleep Efficiency Percentage", SleepEfficiencyPercentage.ToString());
            rv.Add("Sleep Recovery Index", SleepRecoveryIndex.ToString());

            return rv;
        }

        public override void InitFullEventData(JObject json)
        {
            dynamic fullData = (dynamic)json;

            System.Diagnostics.Debug.WriteLine(fullData.ToString());
        }
    }
}
