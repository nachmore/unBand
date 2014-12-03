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

        public abstract void ExportToFile(List<Dictionary<string, object>> data, string filename);
        
        /// <summary>
        /// Returns a list of the unique fields across all items in the dictionary.
        /// 
        /// This is useful when dumping different event types as there will be a common set of fields
        /// but also some unique fields per event type that only need to be filled out for that event
        /// (so we need to add blanks in the correct spots)
        /// </summary>
        /// <param name="data"></param>
        internal List<string> GetUnifiedFields(List<Dictionary<string, object>> data)
        {
            // and this, my friends, is why I'm not the biggest linq fan. Essentially this:
            // 1. selects all keys
            // 2. flattens all of them
            // 3. Gets all of the unique items
            // 4. grabs all of the keys from the unique grouping
            return data.Select(d => d.Keys).SelectMany(d => d).GroupBy(d => d).Select(d => d.Key).ToList();
        }
    }
}
