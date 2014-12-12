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
    public class RunInfoSegment
    {
    }

    public class RunEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new RunEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(RunEventConverter))]
    public class RunEvent : BandExerciseEventBase
    {

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences, BandEventExpandType.MapPoints }; }
        }

        public List<RunInfoSegment> Segments { get; private set; }

        public int TotalDistance { get; set; }
        public int ActualDistance { get; set; }
        public int WayPointDistance { get; set; }
        public int Pace { get; set; }

        public RunEvent(JObject json) : base(json)
        {
            Segments = new List<RunInfoSegment>();
            
            dynamic eventSummary = (dynamic)json;

            TotalDistance     = eventSummary.TotalDistance;
            ActualDistance    = eventSummary.ActualDistance;
            WayPointDistance  = eventSummary.WayPointDistance;
            Pace              = eventSummary.Pace;
        }

        public override Dictionary<string, object> DumpBasicEventData()
        {
            var rv = new Dictionary<string, object>(base.DumpBasicEventData());

            rv.Add("Total Distance (cm)", TotalDistance.ToString());
            rv.Add("Actual Distance (cm)", ActualDistance.ToString());
            rv.Add("WayPoint Distance (cm)", WayPointDistance.ToString());
            rv.Add("Pace", Pace.ToString());

            return rv;
        }

        public override void InitFullEventData(JObject json)
        {
            
        }

    }
}
