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

    public enum SleepEventSequenceType 
    {
        Dozing,
        Sleep,
        Awake,
        Snoozing
    }

    public enum SleepType 
    {
        Unknown,
        RestlessSleep,
        RestfulSleep,
    }

    public class SleepEventSequenceItem : EventBaseSequenceItem
    {
        public SleepEventSequenceType SequenceType { get; private set; }
        public int SleepTime { get; private set; }
        public DateTime DayId { get; private set; }
        public SleepType SleepType { get; private set; }

        public SleepEventSequenceItem(JObject json) : base(json)
        {
            dynamic rawSequence = (dynamic)json;
            
            SequenceType = rawSequence.SequenceType;
            SleepTime = rawSequence.SleepTime;
            DayId = rawSequence.DayId;
            SleepType = rawSequence.SleepType;
        }
    }

    [TypeConverter(typeof(SleepEventConverter))]
    public class SleepEvent : BandEventBase
    {
        private static List<IEventExporter> _exporters;

        public override List<IEventExporter> Exporters
        {
            get
            {
                if (_exporters == null)
                {
                    _exporters = new List<IEventExporter>() { SleepSequencesToCSVExporter.Instance };
                    _exporters.AddRange(base.Exporters);
                }

                return _exporters;
            }
        }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public int AwakeTime { get; private set; }
        public int SleepTime { get; private set; }
        public int NumberOfWakeups { get; private set; }
        public int TimeToFallAsleep { get; private set; }
        public int SleepEfficiencyPercentage { get; private set; }
        public int SleepRecoveryIndex { get; private set; }
        public int RestingHeartRate { get; private set; }

        public override string FriendlyEventType { get { return "Sleep"; } }
        public override string PrimaryMetric { 
            get 
            {
                var span = TimeSpan.FromSeconds(SleepTime);
                return string.Format("{0}h {1}m", span.Hours, span.Minutes);
            } 
        } 

        public SleepEvent(JObject json)
            : base(json)
        {
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
            base.InitFullEventData(json);

            dynamic fullEvent = (dynamic)json;

            foreach (dynamic sequenceData in fullEvent.value[0].Sequences)
            {
                Sequences.Add(new SleepEventSequenceItem(sequenceData));
            }
        }
    }
}
