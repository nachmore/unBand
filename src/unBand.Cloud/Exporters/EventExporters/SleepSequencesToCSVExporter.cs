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
    public class SleepSequencesToCSVExporter : IEventExporter
    {
        #region Singleton

        private static IEventExporter _theOne;

        public static IEventExporter Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new SleepSequencesToCSVExporter();
                }

                return _theOne;
            }
        }

        private SleepSequencesToCSVExporter() { }

        #endregion
        
        public string DefaultExtension { get { return ".csv"; } }
        public string DefaultExportSuffix { get { return "sequence"; } }

        public async Task ExportToFile(BandEventBase eventBase, string filePath)
        {
            if (!(eventBase is SleepEvent))
            {
                throw new ArgumentException("eventBase must be of type SleepEvent to use the SleepToCSVExporter");
            }

            var sleepEvent = eventBase as SleepEvent;

            await Task.Run(() =>
            {
                var dataDump = new List<Dictionary<string, object>>();

                foreach (var sequence in sleepEvent.Sequences)
                {
                    var sleepSequence = sequence as SleepEventSequenceItem;
                    var sequenceData = new Dictionary<string, object>(BaseSequenceDumper.Dump(sequence));

                    sequenceData.Add("Sequence Type", sleepSequence.SequenceType);
                    sequenceData.Add("Sleep Time", sleepSequence.SleepTime);
                    sequenceData.Add("Day Id", sleepSequence.DayId);
                    sequenceData.Add("Sleep Type", sleepSequence.SleepType);

                    dataDump.Add(sequenceData);
                }

                // TODO: pass through convertDateTimeToLocal
                CSVExporter.ExportToFile(dataDump, filePath);
            });
        }
    }
}
