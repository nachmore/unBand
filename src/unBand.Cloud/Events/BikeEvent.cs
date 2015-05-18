using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unBand.Cloud.Events;
using unBand.Cloud.Exporters.EventExporters;

namespace unBand.Cloud
{
    // for now this looks to be a superset of ExerciseEventSequenceItem. If it diverges significantly
    // then we'll need to split ExerciseEventSequenceItem back into a Base abstract class.
    public class BikeEventSequenceItem : ExerciseEventSequenceItem
    {
        public int TotalDistance { get; private set; }
        public int SplitDistance { get; private set; }
        public int ActualDistance { get; private set; }
        public int PausedTime { get; private set; }
        
        public BikeEventSequenceItem(JObject json) : base(json)
        {
            dynamic rawSequence = (dynamic)json;

            TotalDistance = rawSequence.TotalDistance;
            SplitDistance = rawSequence.SplitDistance;
            ActualDistance = rawSequence.ActualDistance;
            PausedTime = rawSequence.PausedTime;
        }
    }

    public class BikeEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new BikeEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(BikeEventConverter))]
    public class BikeEvent : BandExerciseEventBase, IBandEventWithMapPoints
    {
        private readonly BandMapPointCollection _mapPoints = new BandMapPointCollection();
        private static List<IEventExporter> _exporters;

        public override List<IEventExporter> Exporters
        {
            get 
            {
                if (_exporters == null)
                {
                    _exporters = new List<IEventExporter>() { RunSequencesToCSVExporter.Instance, GPXExporter.Instance, TCXExporter.Instance };
                    _exporters.AddRange(base.Exporters);
                }

                return _exporters;
            }
        }

        public override BandEventExpandType[] Expanders
        {
            get { return new BandEventExpandType[] { BandEventExpandType.Info, BandEventExpandType.Sequences, BandEventExpandType.MapPoints }; }
        }

        public IEnumerable<BandMapPoint> MapPoints { get{return _mapPoints;} }

        /// <summary>
        /// Calculated property which indicates whether or not any actual GPS points were
        /// recorded on this run
        /// </summary>
        public bool HasGPSPoints { get; set; }

        public override string FriendlyEventType { get { return "Biking"; } }
        public override string PrimaryMetric { get { return (TotalDistance / 100000.0).ToString("N", CultureInfo.InvariantCulture) + "km"; } }

        public int TotalDistance { get; set; }
        public int ActualDistance { get; set; }
        public int WayPointDistance { get; set; }
        public int Pace { get; set; }

        public BikeEvent(JObject json) : base(json)
        {           
            dynamic eventSummary = (dynamic)json;

            TotalDistance     = eventSummary.TotalDistance;
            ActualDistance    = eventSummary.ActualDistance;
            WayPointDistance  = eventSummary.WayPointDistance;
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
            base.InitFullEventData(json);

            dynamic fullEvent = (dynamic)json;

            foreach (dynamic sequenceData in fullEvent.value[0].Sequences)
            {
                Sequences.Add(new BikeEventSequenceItem(sequenceData));
            }

            // parse out map points
            foreach (dynamic mapData in fullEvent.value[0].MapPoints)
            {
                var runMapPoint = new BandMapPoint()
                {
                    Type = mapData.MapPointType,                    
                    SecondsSinceStart = mapData.SecondsSinceStart,
                    Ordinal = mapData.MapPointOrdinal,
                    Distance = mapData.TotalDistance,
                    HeartRate = mapData.HeartRate,
                    Pace = ((double)mapData.Pace * 0.001),
                    ScaledPace = mapData.ScaledPace,
                    IsPaused = mapData.IsPaused,
                    IsResume = mapData.IsResume
                };

                if (mapData.Location != null) 
                {
                    runMapPoint.Latitude = ((double)mapData.Location.Latitude * 1E-07);
                    runMapPoint.Longitude = ((double)mapData.Location.Longitude * 1E-07);
                    runMapPoint.Altitude = ((double)mapData.Location.AltitudeFromMSL * 0.01);
                    runMapPoint.EHPE = ((double)mapData.Location.EHPE * 0.01);
                    runMapPoint.EVPE = ((double)mapData.Location.EVPE * 0.01);

                    HasGPSPoints = true;
                }

                _mapPoints.Add(runMapPoint);
            }
        }
    }
}
