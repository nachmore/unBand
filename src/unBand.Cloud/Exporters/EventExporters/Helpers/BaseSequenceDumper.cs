using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud.Exporters.EventExporters.Helpers
{
    internal static class BaseSequenceDumper
    {
        internal static Dictionary<string, object> Dump(EventBaseSequenceItem item)
        {
            return new Dictionary<string, object>()
            {
                {"Sequence Id", item.SequenceId},
                {"Start Time", item.StartTime},
                {"Calories Burned", item.CaloriesBurned},
                {"Order", item.Order},
                {"Duration", item.Duration},
                {"Average Heart Rate", item.HeartRate.Average},
                {"Lowest Heart Rate", item.HeartRate.Lowest},
                {"Peak Heart Rate", item.HeartRate.Peak},
            };
        }

    }
}
