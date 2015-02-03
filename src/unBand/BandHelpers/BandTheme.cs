using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace unBand.BandHelpers
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PixelColor
    {
        // 32 bit BGRA 
        [FieldOffset(0)]
        public UInt32 ColorBGRA;
        // 8 bit components
        [FieldOffset(0)]
        public byte Blue;
        [FieldOffset(1)]
        public byte Green;
        [FieldOffset(2)]
        public byte Red;
        [FieldOffset(3)]
        public byte Alpha;
    }

    public class BandTheme : INotifyPropertyChanged
    {
        private CargoClient _client;
        private bool _inited;

        private WriteableBitmap _background;
        private StrappColorPalette _themeColor;

        private SolidColorBrush _baseColor;
        private SolidColorBrush _highlightColor;
        private SolidColorBrush _lowlightColor;
        private SolidColorBrush _mutedColor;
        private SolidColorBrush _secondaryTextColor;
        private SolidColorBrush _highContrastColor;

        // magic values for the background size (if you pass anything else in you get an exception)
        public static short BACKGROUND_WIDTH = 310;
        public static short BACKGROUND_HEIGHT = 102;

        public WriteableBitmap Background
        {
            get { return _background; }
            set
            {
                if (_background != value)
                {
                    _background = value;

                    var colorMuted = new PixelColor
                    {
                        Blue = 0,
                        Green = 0,
                        Red = 255,
                        Alpha = 0
                    };
                    
                    // Usually we would just call the below, but that has a bug that switches channels so
                    // all images come out blue tinted, so call our implementation instead
                    //_client.SetMeTileImageAsync(value);
                    SetMeTileImageAsync(value);

                    NotifyPropertyChanged();
                }
            }
        }

        public SolidColorBrush BaseColor
        {
            get { return _baseColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_baseColor == null || _baseColor.Color != value.Color)
                {
                    _baseColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public SolidColorBrush HighlightColor
        {
            get { return _highlightColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_highlightColor == null || _highlightColor.Color != value.Color)
                {
                    _highlightColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public SolidColorBrush LowlightColor
        {
            get { return _lowlightColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_lowlightColor == null || _lowlightColor.Color != value.Color)
                {
                    _lowlightColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public SolidColorBrush MutedColor
        {
            get { return _mutedColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_mutedColor == null || _mutedColor.Color != value.Color)
                {
                    _mutedColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public SolidColorBrush SecondaryTextColor
        {
            get { return _secondaryTextColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_secondaryTextColor == null || _secondaryTextColor.Color != value.Color)
                {
                    _secondaryTextColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public SolidColorBrush HighContrastColor
        {
            get { return _highContrastColor; }
            set
            {
                // for some reason .Equals is not overridden, so check equality manually
                if (_highContrastColor == null || _highContrastColor.Color != value.Color)
                {
                    _highContrastColor = value;

                    NotifyPropertyChanged();

                    UpdateColors();
                }
            }
        }

        public BandTheme(CargoClient client)
        {
            _client = client;
        }

        public async Task InitAsync()
        {
            // check if there is a MeTile to grab (otherwise you get an exception
            // that will also kill the BT connection for daring to read to far beyond the Stream)
            var tileId = _client.GetMeTileId();

            if (tileId > 0) 
            {
                _background = _client.GetMeTileBmp();
            }
            
            _themeColor = await _client.GetDeviceThemeAsync();

            SetColorProperties();

            // Notify that all properties have changed
            NotifyPropertyChanged(null);

            _inited = true;
        }

        /// <summary>
        /// Break out all of the colors from _themeColor and set the relevant properties
        /// </summary>
        private void SetColorProperties()
        {
            BaseColor          = new SolidColorBrush(GetColorFromBGRA(_themeColor.Base));
            HighlightColor     = new SolidColorBrush(GetColorFromBGRA(_themeColor.Highlight));
            LowlightColor      = new SolidColorBrush(GetColorFromBGRA(_themeColor.Lowlight));
            MutedColor         = new SolidColorBrush(GetColorFromBGRA(_themeColor.Muted));
            SecondaryTextColor = new SolidColorBrush(GetColorFromBGRA(_themeColor.SecondaryText));
            HighContrastColor  = new SolidColorBrush(GetColorFromBGRA(_themeColor.HighContrast));
        }

        private Color GetColorFromBGRA(uint bgra)
        {
            var pixel = new PixelColor() { ColorBGRA = bgra };

            return Color.FromArgb(255, pixel.Red, pixel.Green, pixel.Blue);
        }

        /// Known Colors:
        /// Base          : Base tile color
        /// Highlight     : accent color for various headings
        /// Lowlight      : button press highlight
        /// Muted         : unused?
        /// SecondaryText : controls some (literally) random items such as toggles that are off in settings
        ///                 meeting locations, date received for emails etc.
        /// High Contrast : Tile background when tile has an unread count (such as email, sms, phone etc.)
        public void UpdateColors()
        {
            if (!_inited) return;

            //TODO: all of these properties should be of type Color and the XAML should use a Converter
            _themeColor.Base          = GetBGRA(BaseColor);
            _themeColor.Highlight     = GetBGRA(HighlightColor);
            _themeColor.Lowlight      = GetBGRA(LowlightColor);
            _themeColor.Muted         = GetBGRA(MutedColor);
            _themeColor.SecondaryText = GetBGRA(SecondaryTextColor);
            _themeColor.HighContrast  = GetBGRA(HighContrastColor);
 
            _client.SetDeviceThemeAsync(_themeColor);
        }

        private uint GetBGRA(SolidColorBrush color)
        {
            var colorStruct = new PixelColor()
            {
                Blue = color.Color.B,
                Green = color.Color.G,
                Red = color.Color.R,
                Alpha = color.Color.A
            };

            return colorStruct.ColorBGRA;
        }

        public async Task ResetThemeAsync()
        {
            await _client.ResetThemeColorsAsync();
        }

        public void SetBackground(string file)
        {
            if (File.Exists(file))
            {
                var bitmapSource = new BitmapImage();

                bitmapSource.BeginInit();
                bitmapSource.UriSource = new Uri(file);

                // ensure that we resize to the correct dimensions
                bitmapSource.DecodePixelHeight = 102;
                bitmapSource.DecodePixelWidth = 310;

                bitmapSource.EndInit();

                // the Band expects a Pbgra32 image, so convert it now
                var pbgra32Image = new FormatConvertedBitmap(bitmapSource, PixelFormats.Pbgra32, null, 0);

                WriteableBitmap b = new WriteableBitmap(pbgra32Image);
                Background = b;
            }
        }

        private async Task SetMeTileImageAsync(WriteableBitmap wb)
        {
            byte[] myBgr565 = ConvertToBGR565(wb);

            var method = typeof(CargoClient).GetMethod("InstalledAppListStartStripSyncStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Task.Run(() => { method.Invoke(_client, null); });

            method = typeof(CargoClient).GetMethod("ProtocolWrite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Task.Run(() => { method.Invoke(_client, new object[] { (ushort)49937, new byte[] { 255, 255, 255, 255 }, myBgr565, 30000, false, 2 }); });

            method = typeof(CargoClient).GetMethod("InstalledAppListStartStripSyncEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Task.Run(() => { method.Invoke(_client, null); });
        }

        // Using the built in WB conversion fails, but doing it manually seems to work:
        private byte[] ConvertToBGR565(WriteableBitmap wb)
        {
            int width = wb.PixelWidth;
            int height = wb.PixelHeight;

            int[] pixels = new int[width * height];

            short[] shortArray = new short[pixels.Length];
            byte[] byteArray = new byte[pixels.Length * 2];

            wb.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i++)
            {
                byte[] colors = BitConverter.GetBytes(pixels[i]);

                // extract the RGB component of the pixel, bit shifted to the correct number of
                // bits for BGR565
                // PS: I believe this is the step where the bug in the actual Cargo library exists
                //     since r and b seem to be transposed
                byte r = (byte)(colors[2] >> 3);
                byte g = (byte)(colors[1] >> 2);
                byte b = (byte)(colors[0] >> 3);

                // place the components into their correct 565 locations
                shortArray[i] = (short)(r << 11 | g << 5 | b);
            }

            // Band commands expect an array of bytes so convert the Int16 (short) 
            // to bytes and return that
            Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
        }

        #endregion
    }
}
