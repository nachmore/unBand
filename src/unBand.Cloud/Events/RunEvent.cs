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
    public class RunMapPoint
    {
        public string Type { get; set; }
        public int SecondsSinceStart { get; set; }
        public int Ordinal { get; set; }
        public int Distance { get; set; }
        public int HeartRate { get; set; }
        public double Pace { get; set; }
        
        /// <summary>
        /// 0 to 100. Useful for graphs
        /// </summary>
        public int ScaledPace { get; set; } 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        /// <summary>
        /// Expected Horizontal Position Error
        /// </summary>
        public double EHPE { get; set; } 
        
        /// <summary>
        /// Expected Vertical Position Error
        /// </summary>
        public double EVPE { get; set; } 
        public bool IsPaused { get; set; }
        public bool IsResume { get; set; }
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

        public List<BandEventInfoSegment> Segments { get; private set; }
        public List<RunMapPoint> MapPoints { get; private set; }

        public int TotalDistance { get; set; }
        public int ActualDistance { get; set; }
        public int WayPointDistance { get; set; }
        public int Pace { get; set; }

        public RunEvent(JObject json) : base(json)
        {
            Segments = new List<BandEventInfoSegment>();
            MapPoints = new List<RunMapPoint>();
            
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
            dynamic fullEvent = (dynamic)json;

            // parse out the "Info" section
            foreach (dynamic infoData in fullEvent.value[0].Info)
            {
                Segments.Add(new BandEventInfoSegment(infoData));
            }

            // parse out map points
            foreach (dynamic mapData in fullEvent.value[0].MapPoints)
            {
                var runMapPoint = new RunMapPoint()
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
                }

                MapPoints.Add(runMapPoint);

                // Potential TODO: There is also a .Sequences array in the JSON, though I haven't found much use for it
                // since it seems to be derived from the full set of data. For now, not exporting that.
            }
        }

    }
}
