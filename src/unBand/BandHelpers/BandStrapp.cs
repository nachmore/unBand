using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.BandHelpers
{
    class BandStrapp
    {
        // Starbucks have a magic template all to themselves (at least, I haven't found a way to apply it
        // to others / edit it), so let's detect that
        private static readonly Guid STARBUCKS_GUID = new Guid("{64a29f65-70bb-4f32-99a2-0f250a05d427}");

        public CargoStrapp Strapp { get; private set; }

        public bool IsDefault
        {
            get
            {
                return _tiles.DefaultStrapps.Any(i => i.StrappID == Strapp.StrappID);
            }
        }

        public bool IsStarbucks
        {
            get { return Strapp.StrappID == STARBUCKS_GUID; }
        }

        private BandTiles _tiles;

        public BandStrapp(BandTiles tiles, CargoStrapp strapp)
        {
            Strapp = strapp;
            _tiles = tiles;
        }

    }
}
