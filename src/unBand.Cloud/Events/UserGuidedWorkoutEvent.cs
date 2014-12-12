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
    public class UserGuidedWorkoutInfoSegment
    {
    }

    public class UserGuidedWorkoutEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new UserGuidedWorkoutEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(UserGuidedWorkoutEventConverter))]
    public class UserGuidedWorkoutEvent : BandExerciseEventBase
    {
        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<UserGuidedWorkoutInfoSegment> Segments { get; private set; }

        public UserGuidedWorkoutEvent(JObject json) : base(json)
        {
            Segments = new List<UserGuidedWorkoutInfoSegment>();
            
            dynamic eventSummary = (dynamic)json;
        }

        public override Dictionary<string, object> DumpBasicEventData()
        {
            return base.DumpBasicEventData();
        }

        public override void InitFullEventData(JObject json)
        {
            throw new NotImplementedException();
        }
    }
}
