using Microsoft.Cargo.Client;
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

            Init();
        }

        private async void Init()
        {
            var strip = await _client.GetStartStripAsync();

            // move the StartStrip into a ObservableCollection so that it can be easily manipulated
            var bandStrip = strip.Select(i => new BandStrapp(this, i));
            
            Strip = new ObservableCollection<BandStrapp>(bandStrip);

            DefaultStrapps = (List<CargoStrapp>)(await _client.GetDefaultStrappsAsync());
            
            /*
            var bitmapSource = new BitmapImage();

            bitmapSource.BeginInit();
            bitmapSource.UriSource = new Uri(@"c:\temp\band_logos\test.png");

            // ensure that we resize to the correct dimensions
            bitmapSource.DecodePixelHeight = 46;
            bitmapSource.DecodePixelWidth = 46;

            bitmapSource.EndInit();

            var pbgra32Image = new FormatConvertedBitmap(bitmapSource, System.Windows.Media.PixelFormats.Pbgra32, null, 0);

            var bmp = new System.Windows.Media.Imaging.WriteableBitmap(pbgra32Image);

            var idx = Strip.Count - 2;

            var images = new List<System.Windows.Media.Imaging.WriteableBitmap>() { bmp, bmp };
            //System.Diagnostics.Debug.WriteLine("Switching " + Strip[idx].Name + " with: " + Strip[0].Name);
            Strip[idx].SetImageList(Strip[idx].StrappID, images, 0);

            _client.UpdateStrapp(Strip[idx]);

            var defaults = _client.GetDefaultStrappsNoImages();
             */
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
    }
}
