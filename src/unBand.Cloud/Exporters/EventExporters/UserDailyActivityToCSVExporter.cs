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
    public class UserDailyActivityToCSVExporter : IEventExporter
    {
        #region Singleton

        private static IEventExporter _theOne;

        public static IEventExporter Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new UserDailyActivityToCSVExporter();
                }

                return _theOne;
            }
        }

        private UserDailyActivityToCSVExporter() { }

        #endregion
        
        public string DefaultExtension { get { return ".csv"; } }
        public string DefaultExportSuffix { get { return "info"; } }

        public async Task ExportToFile(BandEventBase eventBase, string filePath)
        {
            if (!(eventBase is UserDailyActivity)) 
            {
                throw new ArgumentException("eventBase must be of type UserDailyActivity to use the UserDailyActivityToCSVExporter");
            }

            await Task.Run(() =>
            {
                var dataDump = new List<Dictionary<string, object>>();

                foreach (UserDailyActivity info in ((UserDailyActivity)eventBase).Segments)
                {
                    dataDump.Add(new Dictionary<string, object>()
                    {
                        {"Start Time", info.StartTime},
                        {"Day Classification", info.DayClassification},
                        {"Activity Level", info.ActivityLevel},
                        {"Steps Taken", info.StepsTaken},
                        {"Calories Burned", info.CaloriesBurned},
                        {"UV Exposure", info.UvExposure},
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
