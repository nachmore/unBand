using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace unBand.Cloud
{
    public class SleepInfoEvent
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

        internal static SleepInfoEvent FromDynamic(dynamic rawEvent)
        {
            var rv = new SleepInfoEvent();

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
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (typeof(string) == sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string) 
            {
                return SleepEvent.FromJson((string)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(SleepEventConverter))]
    public class SleepEvent
    {

        public List<SleepInfoEvent> Segments { get; private set; }

        public SleepEvent()
        {
            Segments = new List<SleepInfoEvent>();
        }

        public static implicit operator SleepEvent(string json)
        {
            return SleepEvent.FromJson(json);
        }

        public static SleepEvent FromJson(string json)
        {
            dynamic parsed = JObject.Parse(json);

            var rv = new SleepEvent();

            if (parsed.value != null && parsed.value[0].Info != null)
            {
                foreach (object rawEvent in parsed.value[0].Info)
                {
                    rv.Segments.Add(SleepInfoEvent.FromDynamic(rawEvent));
                }
            }

            return rv;
        }

    }
}
