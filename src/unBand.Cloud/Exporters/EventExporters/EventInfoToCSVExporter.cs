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
            var sleepEvent = eventBase as SleepEvent;

            if (!(eventBase is RunEvent))
            {
                throw new ArgumentException("eventBase must be of type SleepEvent to use the SleepToCSVExporter");
            }

            await Task.Run(() =>
            {
                var dataDump = new List<Dictionary<string, object>>();

                foreach (var info in sleepEvent.InfoSegments)
                {
                    dataDump.Add(new Dictionary<string, object>()
                    {

                    });
                }
            });
        }
    }
}
