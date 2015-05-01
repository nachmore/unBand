using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace unBand.Cloud
{
    public interface IBandEventWithMapPoints
    {
        bool HasGPSPoints { get; set; }

        List<BandMapPoint> MapPoints { get; }
    }
}
