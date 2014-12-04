using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.CloudHelpers
{
    public class CSVExporter : CloudDataExporter
    {
        public override string DefaultExt { get { return ".csv"; } }

        public override void ExportToFile(List<Dictionary<string, object>> data, string filename)
        {
            var sb = new StringBuilder(500000);
            var fields = GetUnifiedFields(data);

            sb.Append(string.Join(",", fields));
            sb.Append("\n");

            foreach (var item in data)
            {
                // dump all of the values in the correct order
                foreach (var field in fields)
                {
                    if (item.Keys.Contains(field))
                    {
                        sb.Append(PrepareValue(item[field]));
                    }

                    sb.Append(",");
                }
                sb.Append("\n");
            }

            File.WriteAllText(filename, sb.ToString());
        }

        private string PrepareValue(object value)
        {
            if (value is DateTime && Settings.ConvertDateTimeToLocal) 
            {
                var dt = (DateTime)value;
                return dt.ToLocalTime().ToString();
            } 

            return (value == null ? "" : value.ToString());
        }
    }
}
