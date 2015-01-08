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
            string rv;

            if (value is DateTime && Settings.ConvertDateTimeToLocal) 
            {
                var dt = (DateTime)value;
                rv = dt.ToLocalTime().ToString();
            }
            else
            {
                rv = (value == null ? "" : value.ToString());
            }

            rv = CSVEscape(rv);

            return rv;
        }

        #region Handle Commas

        // From: http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

        private string CSVEscape(string field)
        {
            if (field.Contains(QUOTE))
                field = field.Replace(QUOTE, ESCAPED_QUOTE);

            if (field.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                field = QUOTE + field + QUOTE;

            return field;
        }
        
        #endregion

    }
}
