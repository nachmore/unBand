using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public enum ExerciseEventSequenceType
    {
        Workout,
        GuidedWorkout,
        Running,
    }

    public class ExerciseEventSequenceItem : EventBaseSequenceItem
    {
        public ExerciseEventSequenceType SequenceType { get; internal set; }
        public int CaloriesFromCarbs { get; internal set; }
        public int CaloriesFromFat { get; internal set; }
        public int StressBalance { get; internal set; }
        public int MaximalV02 { get; internal set; }

        public double TrainingEffect { get; internal set; }

        public ExerciseEventSequenceItem(JObject json) : base(json)
        {
            dynamic rawSequence = (dynamic)json;

            SequenceType = rawSequence.SequenceType;
            CaloriesFromCarbs = rawSequence.CaloriesFromCarbs;
            CaloriesFromFat = rawSequence.CaloriesFromFat;
            StressBalance = rawSequence.StressBalance;
            MaximalV02 = rawSequence.MaximalV02;
            TrainingEffect = rawSequence.TrainingEffect;

            HeartRate.Zones = new HeartRateZones();

            dynamic zones = rawSequence.HeartRateZones;

            HeartRate.Zones.Aerobic = zones.Aerobic;
            HeartRate.Zones.Anaerobic = zones.Anaerobic;
            HeartRate.Zones.FitnessZone = zones.FitnessZone;
            HeartRate.Zones.HealthyHeart = zones.HealthyHeart;
            HeartRate.Zones.Over = zones.Over;
            HeartRate.Zones.RedLine = zones.RedLine;
            HeartRate.Zones.Under = zones.Under;
        }
    }
}
