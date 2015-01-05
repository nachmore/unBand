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
    public class UserWorkoutInfoSegment
    {
    }

    public class UserWorkoutEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new UserWorkoutEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(UserWorkoutEventConverter))]
    public class UserWorkoutEvent : BandExerciseEventBase
    {
        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<UserWorkoutInfoSegment> Segments { get; private set; }

        public override string FriendlyEventType { get { return "User Workout"; } }

        public UserWorkoutEvent(JObject json) : base(json)
        {
            Segments = new List<UserWorkoutInfoSegment>();

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
