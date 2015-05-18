using System.Collections.Generic;
using unBand.Cloud.Events;

namespace unBand.Cloud
{
    public interface IBandEventWithMapPoints
    {
        bool HasGPSPoints { get; set; }
        IEnumerable<BandMapPoint> MapPoints { get; }
    }
}