using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud.Exporters.EventExporters.Helpers
{
    public static class CSVExporter
    {
        public static void ExportToFile(List<Dictionary<string, object>> data, string filePath, bool convertDateTimeToLocal = true)
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
                        sb.Append(PrepareValue(item[field], convertDateTimeToLocal));
                    }

                    sb.Append(",");
                }
                sb.Append("\n");
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        private static string PrepareValue(object value, bool convertDateTimeToLocal)
        {
            string rv;

            if (value is DateTime && convertDateTimeToLocal) 
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

        /// <summary>
        /// Returns a list of the unique fields across all items in the dictionary.
        /// 
        /// This is useful when dumping different event types as there will be a common set of fields
        /// but also some unique fields per event type that only need to be filled out for that event
        /// (so we need to add blanks in the correct spots)
        /// </summary>
        /// <param name="data"></param>
        private static List<string> GetUnifiedFields(List<Dictionary<string, object>> data)
        {
            // and this, my friends, is why I'm not the biggest linq fan. Essentially this:
            // 1. selects all keys
            // 2. flattens all of them
            // 3. Gets all of the unique items
            // 4. grabs all of the keys from the unique grouping
            return data.Select(d => d.Keys).SelectMany(d => d).GroupBy(d => d).Select(d => d.Key).ToList();
        }

        #region Handle Commas

        // From: http://stackoverflow.com/questions/769621/dealing-with-commas-in-a-csv-file

        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };

        private static string CSVEscape(string field)
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
