using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    /// <summary>
    /// Event detail types. If you use the wrong combination (for example
    /// MapPoints on sleep) you'll get a nice response from the web 
    /// service that will tell you which one is wrong.
    /// </summary>
    public enum BandEventExpandType
    {
        Sequences,
        MapPoints,
        Info
    }

    public enum BandEventType
    {
        Sleeping,
        Running,
        GuidedWorkout,
        Workout,
        UserDailyActivity
    }

    public enum FeelingType
    {
        Unknown,
    }

    public class HeartRateZones
    {
        public int Under { get; set; }
        public int Aerobic { get; set; }
        public int Anaerobic { get; set; }
        public int FitnessZone { get; set; }
        public int HealthyHeart { get; set; }
        public int RedLine { get; set; }
        public int Over { get; set; }
    }

    public class HeartRateSummary
    {
        public int Average { get; set; }
        public int Lowest { get; set; }
        public int Peak { get; set; }
        public int Resting { get; set; }
        public int AtFinish { get; set; }
        public int RecoveryHeartRate1Minute { get; set; }
        public int RecoveryHeartRate2Minute { get; set; }
        public HeartRateZones Zones { get; set; }

        public HeartRateSummary(int average, int low, int peak)
        {
            Average = average;
            Lowest = low;
            Peak = peak;

            Zones = new HeartRateZones();
        }
    }
}
