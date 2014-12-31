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
        public int AwakeTime { get; private set; }
        public int SleepTime { get; private set; }
        public int NumberOfWakeups { get; private set; }
        public int TimeToFallAsleep { get; private set; }
        public int SleepEfficiencyPercentage { get; private set; }
        public int SleepRecoveryIndex { get; private set; }
        public int RestingHeartRate { get; private set; }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<BandEventInfoSegment> Segments { get; private set; }

        public SleepEvent(JObject json)
            : base(json)
        {
            Segments = new List<BandEventInfoSegment>();
            
            dynamic eventSummary = (dynamic)json;

            AwakeTime                 = eventSummary.AwakeTime;
            SleepTime                 = eventSummary.SleepTime;
            NumberOfWakeups           = eventSummary.NumberOfWakeups;
            TimeToFallAsleep          = eventSummary.TimeToFallAsleep;
            SleepEfficiencyPercentage = eventSummary.SleepEfficiencyPercentage;
            SleepRecoveryIndex        = eventSummary.SleepRecoveryIndex;
            RestingHeartRate          = eventSummary.RestingHeartRate;
        }
        
        public override Dictionary<string, object> DumpBasicEventData()
        {
            var rv = new Dictionary<string, object>(base.DumpBasicEventData());

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
        }
    }
}
