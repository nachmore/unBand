using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public abstract class BandExerciseEventBase : BandEventBase
    {
        public override string PrimaryMetric { get { return CaloriesBurned + "cals"; } }

        public string AnalysisStatus { get; set; }
        public int CaloriesFromCarbs { get; set; }
        public int CaloriesFromFat { get; set; }
        public double StressBalance { get; set; }
        public double MaximalVO2 { get; set; }
        public double TrainingEffect { get; set; }
        public int RecoveryTime { get; set; }
        public int PausedTime { get; set; }

        public BandExerciseEventBase(JObject json) : base(json)
        {
            dynamic eventSummary = (dynamic)json;

            AnalysisStatus    = eventSummary.AnalysisStatus;
            CaloriesFromCarbs = eventSummary.CaloriesFromCarbs;
            CaloriesFromFat   = eventSummary.CaloriesFromFat;
            StressBalance     = eventSummary.StressBalance;
            MaximalVO2        = eventSummary.MaximalVO2;
            TrainingEffect    = eventSummary.TrainingEffect;
            RecoveryTime      = eventSummary.RecoveryTime;
            PausedTime        = eventSummary.PausedTime;

            HeartRate.AtFinish                 = eventSummary.FinishHeartRate;
            HeartRate.RecoveryHeartRate1Minute = eventSummary.RecoveryHeartRate1Minute;
            HeartRate.RecoveryHeartRate2Minute = eventSummary.RecoveryHeartRate2Minute;

            var zones = (dynamic)(eventSummary.HeartRateZones);

            HeartRate.Zones.Under        = zones.Under;
            HeartRate.Zones.Aerobic      = zones.Aerobic;
            HeartRate.Zones.Anaerobic    = zones.Anaerobic;
            HeartRate.Zones.FitnessZone  = zones.FitnessZone;
            HeartRate.Zones.HealthyHeart = zones.HealthyHeart;
            HeartRate.Zones.RedLine      = zones.RedLine;
            HeartRate.Zones.Over         = zones.Over;
        }

        public override Dictionary<string, object> DumpBasicEventData()
        {
            var rv = new Dictionary<string, object>(base.DumpBasicEventData());

            rv.Add("Analysis Status", AnalysisStatus.ToString());
            rv.Add("Calories From Carbs", CaloriesFromCarbs.ToString());
            rv.Add("Calories From Fat", CaloriesFromFat.ToString());
            rv.Add("StressBalance", StressBalance.ToString());
            rv.Add("Maximal VO2", MaximalVO2.ToString());
            rv.Add("Training Effect", TrainingEffect.ToString());
            rv.Add("Recovery Time (sec)", RecoveryTime.ToString());
            rv.Add("Paused Time", PausedTime.ToString());

            rv.Add("Heart Rate at Finish", HeartRate.AtFinish);
            rv.Add("Recovery Heart Rate after 1 Minute", HeartRate.RecoveryHeartRate1Minute);
            rv.Add("Recovery Heart Rate after 2 Minutes", HeartRate.RecoveryHeartRate1Minute);

            rv.Add("Heart Rate Zone - Under", HeartRate.Zones.Under);
            rv.Add("Heart Rate Zone - Aerobic", HeartRate.Zones.Aerobic);
            rv.Add("Heart Rate Zone - Anaerobic", HeartRate.Zones.Anaerobic);
            rv.Add("Heart Rate Zone - FitnessZone", HeartRate.Zones.FitnessZone);
            rv.Add("Heart Rate Zone - RedLine", HeartRate.Zones.RedLine);
            rv.Add("Heart Rate Zone - Over", HeartRate.Zones.Over);

            return rv;
        }

        public override Dictionary<string, string> DumpFullEventData()
        {
            return null;
        }

    }
}
