using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace unBand.Cloud
{
    public class RunInfoSegment
    {

    }

    public class RunMapPoint
    {
        public int SecondsSinceStart { get; set; }
        public int Ordinal { get; set; }
        public int Distance { get; set; }
        public int HeartRate { get; set; }
        public double Pace { get; set; }
        public int ScaledPace { get; set; } //0 to 100. Useful for graphs
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double EHPE { get; set; } //Expected Horizontal Position Error
        public double EVPE { get; set; } //Expected Vertical Position Error
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

        public List<RunInfoSegment> Segments { get; private set; }
        public List<RunMapPoint> MapPoints { get; private set; }

        public int TotalDistance { get; set; }
        public int ActualDistance { get; set; }
        public int WayPointDistance { get; set; }
        public int Pace { get; set; }

        public RunEvent(JObject json) : base(json)
        {
            Segments = new List<RunInfoSegment>();
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
            dynamic eventSummary = (dynamic)json;
            //TODO: Bad syntax...
            foreach (dynamic mp in eventSummary.value[0].MapPoints)
            {
                MapPoints.Add(new RunMapPoint
                {
                    SecondsSinceStart = mp.SecondsSinceStart,
                    Ordinal = mp.MapPointOrdinal,
                    Distance = mp.TotalDistance,
                    HeartRate = mp.HeartRate,
                    Pace = ((double)mp.Pace * 0.001),
                    ScaledPace = mp.ScaledPace,
                    Latitude = ((double)mp.Location.Latitude * 1E-07),
                    Longitude = ((double)mp.Location.Longitude * 1E-07),
                    Altitude = ((double)mp.Location.AltitudeFromMSL * 0.01),
                    EHPE = ((double)mp.Location.EHPE * 0.01),
                    EVPE = ((double)mp.Location.EVPE * 0.01),
                    IsPaused = mp.IsPaused,
                    IsResume = mp.IsResume
                });
            }
        }

        public void WriteGPXFile(string filePath)
        {
            var gpxtpx = XNamespace.Get("http://www.garmin.com/xmlschemas/TrackPointExtension/v1");
            var gpxx = XNamespace.Get("http://www.garmin.com/xmlschemas/GpxExtensions/v3");
            var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            var nsRoot = XNamespace.Get("http://www.topografix.com/GPX/1/1");
            var xsiSchemaLocation = XNamespace.Get("http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.garmin.com/xmlschemas/GpxExtensions/v3 http://www.garmin.com/xmlschemas/GpxExtensionsv3.xsd http://www.garmin.com/xmlschemas/TrackPointExtensionv1.xsd");

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));

            var xroot = new XElement(nsRoot + "gpx",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XAttribute(xsi + "schemaLocation", xsiSchemaLocation),
                new XAttribute("xmlns", nsRoot),
                new XAttribute(XNamespace.Xmlns + "gpxx", gpxx),
                new XAttribute(XNamespace.Xmlns + "gpxtpx", gpxtpx),
                new XAttribute("creator", "unBand - https://github.com/nachmore/unBand/"),
                new XAttribute("version", "1.1"),
                new XElement(nsRoot + "metadata",
                    new XElement(nsRoot + "name", this.Name),
                    new XElement(nsRoot + "description", this.EventID),
                    new XElement(nsRoot + "time", this.StartTime.ToUniversalTime().ToString("s"))
                ),
                new XElement(nsRoot + "trk",
                    new XElement(nsRoot + "src", "Microsoft Band"),
                    new XElement(nsRoot + "trkseg")
                )
            );

            foreach (var mp in this.MapPoints)
            {
                xroot.Element(nsRoot + "trk").Element(nsRoot + "trkseg").Add(
                    new XElement(nsRoot + "trkpt",
                        new XElement(nsRoot + "ele", mp.Altitude.ToString("0.0000000000000000", new CultureInfo("en-US", false))),
                        new XElement(nsRoot + "time", this.StartTime.AddSeconds(mp.SecondsSinceStart).ToUniversalTime().ToString("s")),
                        new XElement(nsRoot + "geoidheight", mp.Altitude.ToString("0.0000000000000000", new CultureInfo("en-US", false))),
                        new XElement(nsRoot + "extensions",
                            new XElement(gpxtpx + "TrackPointExtension",
                                new XElement(gpxtpx + "hr", mp.HeartRate)
                            )
                        ),
                        new XAttribute("lat", mp.Latitude.ToString("0.00000000", new CultureInfo("en-US", false))),
                        new XAttribute("lon", mp.Longitude.ToString("0.00000000", new CultureInfo("en-US", false)))
                    )
                );
            }

            doc.Add(xroot);

            filePath = Path.Combine(filePath, this.StartTime.ToString("yyyymmdd") + "_" + this.EventID + ".gpx");

            xroot.Save(filePath);
        }
    }
}
