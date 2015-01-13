using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using unBand.Cloud.Exporters.EventExporters.Helpers;

namespace unBand.Cloud.Exporters.EventExporters
{
    public class EventInfoToCSVExporter : IEventExporter
    {
        #region Singleton

        private static IEventExporter _theOne;

        public static IEventExporter Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new EventInfoToCSVExporter();
                }

                return _theOne;
            }
        }

        private EventInfoToCSVExporter() { }

        #endregion
        
        public string DefaultExtension { get { return ".csv"; } }
        public string DefaultExportSuffix { get { return "info"; } }

        public async Task ExportToFile(BandEventBase eventBase, string filePath)
        {
            await Task.Run(() =>
            {
                var dataDump = new List<Dictionary<string, object>>();

                foreach (var info in eventBase.InfoSegments)
                {
                    dataDump.Add(new Dictionary<string, object>()
                    {
                        {"Time of Day", info.TimeOfDay},
                        {"Day Classification", info.DayClassification},
                        {"Activity Level", info.ActivityLevel},
                        {"Steps Taken", info.StepsTaken},
                        {"Calories Burned", info.CaloriesBurned},
                        {"UV Exposure", info.UvExposure},
                        {"Location", info.Location},
                        {"Peak Heart Rate", info.HeartRate.Peak},
                        {"Average Heart Rate", info.HeartRate.Average},
                        {"Lowest Heart Rate", info.HeartRate.Lowest},
                        {"Total Distance", info.TotalDistance},
                        {"It Cal", info.ItCal}
                    });
                }

                CSVExporter.ExportToFile(dataDump, filePath);
            });
        }
    }
}
