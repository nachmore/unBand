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
        private static List<IEventExporter> _exporters;

        public override List<IEventExporter> Exporters
        {
            get
            {
                if (_exporters == null)
                {
                    _exporters = new List<IEventExporter>() { ExerciseSequencesToCSVExporter.Instance };
                    _exporters.AddRange(base.Exporters);
                }

                return _exporters;
            }
        }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences }; }
        }

        public List<UserGuidedWorkoutInfoSegment> Segments { get; private set; }

        public override string FriendlyEventType { get { return "Guided Workout"; } }

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
            base.InitFullEventData(json);

            dynamic fullEvent = (dynamic)json;

            foreach (dynamic sequenceData in fullEvent.value[0].Sequences)
            {
                Sequences.Add(new ExerciseEventSequenceItem(sequenceData));
            }
        }
    }
}
