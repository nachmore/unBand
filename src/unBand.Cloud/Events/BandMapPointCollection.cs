using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace unBand.Cloud.Events
{
    internal class BandMapPointCollection : IEnumerable<BandMapPoint>
    {
        private readonly List<BandMapPoint> _mapPoints = new List<BandMapPoint>();

        internal void Add(BandMapPoint mapPoint)
        {
            _mapPoints.Add(mapPoint);
        }

        public IEnumerator<BandMapPoint> GetEnumerator()
        {
            return _mapPoints.FilterInvalidCoordinates().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal static class BandMapPointFilterExtensions
    {
        public static IEnumerable<BandMapPoint> FilterInvalidCoordinates(this IEnumerable<BandMapPoint> self)
        {
            return self.Where(IsValid);
        }

        private static bool IsValid(BandMapPoint point)
        {
            return !((int) point.Latitude == 0 &&
                     (int) point.Longitude == 0 &&
                     (int) point.Altitude == 0);
        }
    }
}