using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud.Exporters.EventExporters
{
    /// <summary>
    /// Note: exporters should be Singletons (we're forcing them to be so that we don't
    /// need to create instances for each export), so they also need to implement some
    /// kind of static mechanism to retrieve the Instance
    /// </summary>
    public interface IEventExporter
    {
        string DefaultExtension { get; }
        string DefaultExportSuffix { get; }

        Task ExportToFile(BandEventBase eventBase, string filePath);
    }
}
