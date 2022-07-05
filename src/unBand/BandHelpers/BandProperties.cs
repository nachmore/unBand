using Microsoft.Band.Admin;
using Microsoft.Band.Admin.Streaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace unBand.BandHelpers
{
    /// <summary>
    /// Used to extract a bunch of static(ish) properties from a device so that we can bind against the values
    /// </summary>
    class BandProperties : INotifyPropertyChanged
    {
        private ICargoClient _client;

        private string _deviceName;
        private byte _percentCharge;
        private System.Timers.Timer _timeUpdater;
        private DateTime _deviceTime;

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte BatteryPercentageCharge
        {
            get { return _percentCharge; }
            set
            {
                if (_percentCharge != value)
                {
                    _percentCharge = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime DeviceTime
        {
            get { return _deviceTime; }
            set
            {
                // we always notify of changes to DeviceTime since we often just update
                // properties of _deviceTime

                _deviceTime = value;
                
                NotifyPropertyChanged();
            }
        }
        
        #region Non-Changing Properties

        public string PermanentSerialNumber { get; private set; }
        public string ProductSerialNumber { get; private set; }
        public bool IsValidFirmware { get; private set; }
        public FirmwareVersions FirmwareVersions { get; private set; }
        public EphemerisCoverageDates EphemerisCoverageDates { get; private set; }
        public UInt16 LogVersion { get; private set; }
        public UInt32 MaxStrappCount { get; private set; } // TODO: can this be set? There is no function to do this, but there are generic property dictionaries
        public long PendingDeviceDataBytes { get; private set; }
        public string RunningAppType { get; private set; }
        public UInt32 TimeZonesDataVersion { get; private set; }

        public Guid DeviceId { get; private set; }

        #endregion

        internal BandProperties(ICargoClient client)
        {
            _client = client;
        }

        public async Task InitAsync()
        {
            PermanentSerialNumber = await _client.GetPermanentSerialNumberAsync();
            ProductSerialNumber   = await _client.GetProductSerialNumberAsync();

            IsValidFirmware  = await _client.GetFirmwareBinariesValidationStatusAsync();
            FirmwareVersions = await _client.GetFirmwareVersionsAsync();
            EphemerisCoverageDates = await _client.GetGpsEphemerisCoverageDatesFromDeviceAsync();
            LogVersion = await _client.GetLogVersionAsync();
            MaxStrappCount = await _client.GetMaxTileCountAsync();
            
            PendingDeviceDataBytes = await _client.GetPendingDeviceDataBytesAsync();
            // var j = await _client.GetPendingLocalDataBytesAsync(); NullException
            RunningAppType = (await _client.GetRunningAppAsync()).ToString();
            TimeZonesDataVersion = await _client.GetTimeZonesDataVersionFromDeviceAsync();

            await _client.SensorSubscribeAsync(SensorType.BatteryGauge); 
            _client.BatteryGaugeUpdated += _client_BatteryGaugeUpdated;

            _client.BatteryUpdated += _client_BatteryUpdated;

            var userProfile = await _client.GetUserProfileFromDeviceAsync();
            
            DeviceName = userProfile.DeviceSettings.DeviceName;
            DeviceId = userProfile.DeviceSettings.DeviceId;

            await InitDeviceTime();
        }

        private async Task InitDeviceTime()
        {
            // not sure why the band doesn't report timezone in a standard way, but the time returned by
            // the band is in UTC (as per the function name) while the timeZone is a collection of properties
            // with the actual time zone represented as a raw string...
            var timeZone = await _client.GetDeviceTimeZoneAsync();
            var time     = await _client.GetDeviceUtcTimeAsync();

            // ... luckily we have the "ZoneOffsetMinutes" property which can be used to offset the time to the correct time that is displayed on the band.
            time = time.AddMinutes(timeZone.ZoneOffsetMinutes);

            DeviceTime = time;

            // start a timer to update the current time (we cheat by using a local timer
            // instead of polling the device)
            _timeUpdater = new System.Timers.Timer();
            _timeUpdater.Elapsed += _timeUpdater_Elapsed;
            _timeUpdater.Interval = (60 - time.Second) * 1000;
            _timeUpdater.Start();
        }

        void _timeUpdater_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // first trigger
            if (!_timeUpdater.AutoReset)
            {
                _timeUpdater.Interval = 60000;
                _timeUpdater.AutoReset = true;
                _timeUpdater.Start();
            }
            
            DeviceTime = _deviceTime.AddMinutes(1);
        }

        ~BandProperties()
        {
            // TODO: would be great to manage state for this at a global level so that we could tell on disconnect
            //       that some sensors were still subscribed to (as opposed to just saying "we're subscribed")
            try
            {
                _client.SensorUnsubscribe(SensorType.BatteryGauge);
            }
            catch { } // this will throw if the user disconnected their band from the machine before exiting
        }

        void _client_BatteryUpdated(object sender, BatteryUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _client_BatteryGaugeUpdated(object sender, BatteryGaugeUpdatedEventArgs e)
        {
            BatteryPercentageCharge = e.PercentCharge;
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
