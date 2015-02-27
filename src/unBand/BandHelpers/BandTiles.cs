using Microsoft.Band.Admin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace unBand.BandHelpers
{
    class BandTiles
    {
        private CargoClient _client;

        public ObservableCollection<BandStrapp> Strip { get; private set; }

        public List<CargoStrapp> DefaultStrapps { get; private set; }

        public BandTiles(CargoClient client)
        {
            _client = client;
        }

        public async Task InitAsync()
        {
            var strip = await _client.GetStartStripAsync();

            // move the StartStrip into a ObservableCollection so that it can be easily manipulated
            var bandStrip = strip.Select(i => new BandStrapp(this, i));
            
            Strip = new ObservableCollection<BandStrapp>(bandStrip);

            DefaultStrapps = (List<CargoStrapp>)(await _client.GetDefaultStrappsAsync());
        }

        public Task Save()
        {
            var strappStrip = Strip.Select(i => i.Strapp).ToList();
            var strip = new StartStrip(strappStrip);

            return _client.SetStartStripAsync(strip);
        }


        public async Task ClearAllCounts()
        {
            foreach (var bandStrapp in Strip)
            {
                await _client.ClearStrappAsync(bandStrapp.Strapp.StrappID);
            }
        }

        internal void UpdateStrapp(CargoStrapp strapp)
        {
            _client.UpdateStrapp(strapp);

            // here is where it gets a bit hairy. If we just call UpdateStrapp alone
            // it will shove the updated Tile at the end of the strip, which is undesirable
            // so let's resave the strip so that the tiles go back to the same location
            Save();
                
        }
    }
}
