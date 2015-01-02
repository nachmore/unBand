using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace unBand.Cloud.Exporters
{
    public class GPXExporter
    {
        public void ExportToFile(BandEventBase eventBase, string filePath)
        {
            if (!(eventBase is RunEvent))
            {
                throw new ArgumentException("eventBase must be of type RunEvent to use the GPXExporter");
            }

            var runEvent = eventBase as RunEvent;

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
                new XAttribute("creator", "unBand - http://unband.nachmore.com/"),
                new XAttribute("version", "1.1"),
                new XElement(nsRoot + "metadata",
                    new XElement(nsRoot + "name", runEvent.Name),
                    new XElement(nsRoot + "description", runEvent.EventID),
                    new XElement(nsRoot + "time", runEvent.StartTime.ToUniversalTime().ToString("s"))
                ),
                new XElement(nsRoot + "trk",
                    new XElement(nsRoot + "src", "Microsoft Band"),
                    new XElement(nsRoot + "trkseg")
                )
            );

            foreach (var mp in runEvent.MapPoints)
            {
                xroot.Element(nsRoot + "trk").Element(nsRoot + "trkseg").Add(
                    new XElement(nsRoot + "trkpt",
                        new XElement(nsRoot + "ele", mp.Altitude.ToString("0.0000000000000000", new CultureInfo("en-US", false))),
                        new XElement(nsRoot + "time", runEvent.StartTime.AddSeconds(mp.SecondsSinceStart).ToUniversalTime().ToString("s")),
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

            filePath = Path.Combine(filePath, runEvent.StartTime.ToString("yyyymmdd") + "_" + runEvent.EventID + ".gpx");

            xroot.Save(filePath);
        }
    }
}
