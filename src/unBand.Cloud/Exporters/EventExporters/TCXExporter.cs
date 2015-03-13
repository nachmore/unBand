using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace unBand.Cloud.Exporters.EventExporters
{
    public class TCXExporter : IEventExporter
    {
        #region Singleton

        private static IEventExporter _theOne;

        public static IEventExporter Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new TCXExporter();
                }

                return _theOne;
            }
        }

        private TCXExporter() { }

        #endregion


        public string DefaultExtension { get { return ".tcx"; } }
        public string DefaultExportSuffix { get { return "map"; } }

        public async Task ExportToFile(BandEventBase eventBase, string filePath)
        {
            await Task.Run(() =>
            {
                var xsiSchemaLocation = XNamespace.Get("http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd");
                var ns5 = XNamespace.Get("http://www.garmin.com/xmlschemas/ActivityGoals/v1");
                var ns3 = XNamespace.Get("http://www.garmin.com/xmlschemas/ActivityExtension/v2");
                var ns2 = XNamespace.Get("http://www.garmin.com/xmlschemas/UserProfile/v2");
                var nsRoot = XNamespace.Get("http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2");
                var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
                var ns4 = XNamespace.Get("http://www.garmin.com/xmlschemas/ProfileExtension/v1");

                var doc = new XDocument(new XDeclaration("1.0", "utf-8", null));

                var xroot = new XElement(nsRoot + "TrainingCenterDatabase",
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XAttribute(xsi + "schemaLocation", xsiSchemaLocation),
                    new XAttribute("xmlns", nsRoot),
                    new XAttribute(XNamespace.Xmlns + "ns5", ns5),
                    new XAttribute(XNamespace.Xmlns + "ns3", ns3),
                    new XAttribute(XNamespace.Xmlns + "ns2", ns2),
                    new XAttribute(XNamespace.Xmlns + "ns4", ns4),
                    new XAttribute("creator", "unBand - http://unband.nachmore.com/"),
                    new XAttribute("version", "1.0"),
                    new XElement(nsRoot + "Activities",
                        new XElement(nsRoot + "Activity",
                            new XAttribute("Sport", "Running"),
                            new XElement(nsRoot + "Id", eventBase.EventID),
                            new XElement(nsRoot + "Lap",
                                new XAttribute("StartTime", eventBase.StartTime.ToUniversalTime().ToString("o")),
                                new XElement(nsRoot + "TotalTimeSeconds", eventBase.Duration),
                                new XElement(nsRoot + "Calories", eventBase.CaloriesBurned),
                                new XElement(nsRoot + "AverageHeartRateBpm",
                                    new XElement(nsRoot + "Value", eventBase.HeartRate.Average)
                                ),
                                new XElement(nsRoot + "MaximumHeartRateBpm",
                                    new XElement(nsRoot + "Value", eventBase.HeartRate.Peak)
                                ),
                                new XElement(nsRoot + "TriggerMethod", "manual")
                            )
                        )
                    )
                );

                //Add run specific data. TCX could be also used for basic workout data (future implementation).
                if (eventBase is RunEvent)
                {
                    var runEvent = eventBase as RunEvent;

                    xroot.Element(nsRoot + "Activities").Element(nsRoot + "Activity").Element(nsRoot + "Lap").Add(
                        new XElement(nsRoot + "DistanceMeters", runEvent.TotalDistance / 100),
                        new XElement(nsRoot + "Track")
                    );

                    foreach (var mp in runEvent.MapPoints)
                    {
                        var trackpoint = new XElement(nsRoot + "Trackpoint",
                            new XElement(nsRoot + "Time", runEvent.StartTime.AddSeconds(mp.SecondsSinceStart).ToUniversalTime().ToString("o")),
                            new XElement(nsRoot + "HeartRateBpm",
                                new XElement(nsRoot + "Value", mp.HeartRate)
                            )
                        );

                        if (mp.Latitude != 0 && mp.Latitude != 0)
                        {
                            trackpoint.Add(
                                new XElement(nsRoot + "AltitudeMeters", mp.Altitude.ToString("0.0000000000000000", new CultureInfo("en-US", false))),
                                new XElement(nsRoot + "Position",
                                    new XElement(nsRoot + "LatitudeDegrees", mp.Latitude.ToString("0.00000000", new CultureInfo("en-US", false))),
                                    new XElement(nsRoot + "LongitudeDegrees", mp.Longitude.ToString("0.00000000", new CultureInfo("en-US", false)))
                                )
                            );
                        }

                        xroot.Element(nsRoot + "Activities").Element(nsRoot + "Activity").Element(nsRoot + "Lap").Element(nsRoot + "Track").Add(
                            trackpoint
                        );
                    }
                }

                doc.Add(xroot);

                xroot.Save(filePath);
            });
        }
    }
}
