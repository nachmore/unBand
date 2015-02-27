using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.Cloud
{
    public class BandMapPoint
    {
        public string Type { get; set; }
        public int SecondsSinceStart { get; set; }
        public int Ordinal { get; set; }
        public int Distance { get; set; }
        public int HeartRate { get; set; }
        public double Pace { get; set; }

        /// <summary>
        /// 0 to 100. Useful for graphs
        /// </summary>
        public int ScaledPace { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        /// <summary>
        /// Expected Horizontal Position Error
        /// </summary>
        public double EHPE { get; set; }

        /// <summary>
        /// Expected Vertical Position Error
        /// </summary>
        public double EVPE { get; set; }
        public bool IsPaused { get; set; }
        public bool IsResume { get; set; }
    }
}
