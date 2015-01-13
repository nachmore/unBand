using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.CloudHelpers
{
    public abstract class CloudDataExporter
    {
        public abstract string DefaultExt { get; }
        public CloudDataExporterSettings Settings { get; set; }

        public abstract void ExportToFile(List<Dictionary<string, object>> data, string filename);
    }
}
