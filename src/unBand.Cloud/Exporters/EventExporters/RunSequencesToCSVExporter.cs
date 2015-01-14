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
    public class RunSequencesToCSVExporter : IEventExporter
    {
        #region Singleton

        private static IEventExporter _theOne;

        public static IEventExporter Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new RunSequencesToCSVExporter();
                }

                return _theOne;
            }
        }

        private RunSequencesToCSVExporter() { }

        #endregion
        
        public string DefaultExtension { get { return ".csv"; } }
        public string DefaultExportSuffix { get { return "sequence"; } }

        public async Task ExportToFile(BandEventBase eventBase, string filePath)
        {
            if (!(eventBase is RunEvent))
            {
                throw new ArgumentException("eventBase must be of type SleepEvent to use the RunToCSVExporter");
            }

            var runEvent = eventBase as RunEvent;

            await Task.Run(() =>
            {
                var dataDump = new List<Dictionary<string, object>>();

                foreach (var sequence in runEvent.Sequences)
                {
                    var runSequence = sequence as RunEventSequenceItem;
                    var sequenceData = new Dictionary<string, object>(BaseSequenceDumper.Dump(sequence))
                    {
                        {"Total Distance", runSequence.TotalDistance},
                        {"Split Distance", runSequence.SplitDistance},
                        {"Split Pace", runSequence.SplitPace},
                        {"Overall Pace", runSequence.OverallPace},
                        {"Actual Distance", runSequence.ActualDistance},
                        {"Paused Time", runSequence.PausedTime},
                    };

                    dataDump.Add(sequenceData);
                }

                // TODO: pass through convertDateTimeToLocal
                CSVExporter.ExportToFile(dataDump, filePath);
            });
        }
    }
}
