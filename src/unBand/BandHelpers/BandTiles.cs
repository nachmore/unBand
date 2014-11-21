using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unBand.BandHelpers
{
    class BandTiles
    {
        private CargoClient _client;

        public ObservableCollection<CargoStrapp> Strip { get; private set; }

        public BandTiles(CargoClient client)
        {
            _client = client;

            Init();
        }

        private async void Init()
        {
            var strip = await _client.GetStartStripAsync();

            // move the StartStrip into a ObservableCollection so that it can be easily manipulated
            Strip = new ObservableCollection<CargoStrapp>(strip);
        }

        public Task Save()
        {
            var strip = new StartStrip(Strip.ToList<CargoStrapp>());

            return _client.SetStartStripAsync(strip);
        }

    }
}
