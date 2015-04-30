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
using Microsoft.Band.Admin;
using Microsoft.Band;
using Microsoft.Band.Personalization;

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
        private Microsoft.Band.BandTheme _themeColor;

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
            // NEEDS BT VERIFICATION IF THIS STILL HAPPENS: check if there is a MeTile to grab (otherwise you get an exception
            // that will also kill the BT connection for daring to read to far beyond the Stream)
            // Need to find a workaround, since this is not exposed in the new lib: var tileId = _client.GetMeTileId();

            BandImage meTileImage = null;

            try
            {
                meTileImage = await _client.PersonalizationManager.GetMeTileImageAsync();
            }
            catch (InvalidOperationException)
            { } // no background image

            if (meTileImage != null)
                _background = meTileImage.ToWriteableBitmap();


            _themeColor = await _client.PersonalizationManager.GetThemeAsync();

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
            BaseColor          = new SolidColorBrush(_themeColor.Base.ToColor());
            HighlightColor     = new SolidColorBrush(_themeColor.Highlight.ToColor());
            LowlightColor      = new SolidColorBrush(_themeColor.Lowlight.ToColor());
            MutedColor         = new SolidColorBrush(_themeColor.Muted.ToColor());
            SecondaryTextColor = new SolidColorBrush(_themeColor.SecondaryText.ToColor());
            HighContrastColor  = new SolidColorBrush(_themeColor.HighContrast.ToColor());
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
            _themeColor.Base          = BaseColor.Color.ToBandColor();
            _themeColor.Highlight     = HighlightColor.Color.ToBandColor();
            _themeColor.Lowlight      = LowlightColor.Color.ToBandColor();
            _themeColor.Muted         = MutedColor.Color.ToBandColor();
            _themeColor.SecondaryText = SecondaryTextColor.Color.ToBandColor();
            _themeColor.HighContrast  = HighContrastColor.Color.ToBandColor();
 
            _client.SetDeviceThemeAsync(_themeColor);
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
            var bandImage = wb.ToBandImage();
            await _client.PersonalizationManager.SetMeTileImageAsync(bandImage);
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
