using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace unBand.BandHelpers
{
    class BandManager : INotifyPropertyChanged
    {

        #region Singleton

        /// <summary>
        /// Call BandManager.Start() to kick things off htere
        /// TODO: Consider an exception if Start() is not called
        /// </summary>
        public static BandManager Instance { get; private set; }

        #endregion

        private bool _isConnected = false;
        private DeviceInfo _deviceInfo;
        private CargoClient _cargoClient;
        private BandProperties _properties;
        private BandTheme _theme;
        private BandSensors _sensors;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public CargoClient CargoClient
        { 
            get { return _cargoClient; }
            set
            {
                if (_cargoClient != value)
                {
                    _cargoClient = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandProperties Properties
        {
            get { return _properties; }
            set
            {
                if (_properties != value)
                {
                    _properties = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandTheme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandSensors Sensors
        {
            get { return _sensors; }
            set
            {
                if (_sensors != value)
                {
                    _sensors = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BandManager()
        {
        }

        public static void Create()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Instance = new BandManager();
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Microsoft.Cargo.Client.Desktop8"))
            {
                // there are dependencies (primarily Newtonsoft.Json) but we don't use those yet.
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                return Assembly.LoadFrom(@"C:\Program Files (x86)\Microsoft Band Sync\Microsoft.Cargo.Client.Desktop8.dll"); 
            }

            return null;
        }

        public static void Start() 
        {
            if (Instance == null)
                Create();

            // While I don't love the whole Timer(1) thing, it simply means that we trigger the first run immediately
            // TODO: Since this could potentially cause race conditions if someone really wanted to garauntee order
            //       of execution we could split out object creation from Start().
            Timer timer = new Timer(1);

            timer.Elapsed += async (sender, e) =>
            {
                if (!Instance.IsConnected)
                {
                    await Instance.DiscoverDevices();

                }
                else
                {
                    // make sure we still have devices
                    var devices = await CargoClient.GetConnectedDevicesAsync();

                    if (!(devices.Count() > 0 && devices[0].Id == Instance._deviceInfo.Id))
                    {
                        Instance.IsConnected = false;
                    }
                }

                // only reset once we finished processing
                timer.Interval = 1000;
                timer.Start();
            };

            timer.AutoReset = false;
            timer.Start();
        }

        private async Task DiscoverDevices()
        {
            var devices = await CargoClient.GetConnectedDevicesAsync();

            if (devices.Count() > 0)
            {
                // must create on the UI thread for various Binding reasons
                _deviceInfo = devices[0];

                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                // TODO: support more than one device?
                    CargoClient = await CargoClient.CreateAsync(_deviceInfo);
                
                    IsConnected = true;

                    //TODO: call an "OnConnected" function
                    Properties = new BandProperties(CargoClient);
                    Theme = new BandTheme(CargoClient);
                    Sensors = new BandSensors(CargoClient);
                }));
            }
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
