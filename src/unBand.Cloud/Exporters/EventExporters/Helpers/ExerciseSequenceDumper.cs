using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud.Exporters.EventExporters.Helpers
{
    internal static class ExerciseSequenceDumper
    {
        internal static Dictionary<string, object> Dump(ExerciseEventSequenceItem item)
        {
            return new Dictionary<string, object>(BaseSequenceDumper.Dump(item))
            {
                {"Sequence Type", item.SequenceType},
                {"Calories from Carbs", item.CaloriesFromCarbs},
                {"Calories from Fat", item.CaloriesFromFat},
                {"Stress Balance", item.StressBalance},
                {"Maximal V02", item.MaximalV02},
                {"Training Effect", item.TrainingEffect},
                {"Heart Rate Zone - Aerobic", item.HeartRate.Zones.Aerobic},
                {"Heart Rate Zone - Anaerobic", item.HeartRate.Zones.Anaerobic},
                {"Heart Rate Zone - Fitness Zone", item.HeartRate.Zones.FitnessZone},
                {"Heart Rate Zone - Healthy Heart", item.HeartRate.Zones.HealthyHeart},
                {"Heart Rate Zone - Over", item.HeartRate.Zones.Over},
                {"Heart Rate Zone - Red Line", item.HeartRate.Zones.RedLine},
                {"Heart Rate Zone - Under", item.HeartRate.Zones.Under},
            };
        }

    }
}
