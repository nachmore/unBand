using Microsoft.Cargo.Client;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private bool _isDesktopSyncAppRunning = false;
        private DeviceInfo _deviceInfo;
        private CargoClient _cargoClient;
        private BandProperties _properties;
        private BandTheme _theme;
        private BandSensors _sensors;
        private BandTiles _tiles;

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

        public bool IsDesktopSyncAppRunning
        {
            get { return _isDesktopSyncAppRunning; }
            set
            {
                if (_isDesktopSyncAppRunning != value)
                {
                    _isDesktopSyncAppRunning = value;
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

        public BandTiles Tiles
        {
            get { return _tiles; }
            set
            {
                if (_tiles != value)
                {
                    _tiles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandLogger Log { get { return BandLogger.Instance; } }

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
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                var dllName = "Microsoft.Cargo.Client.Desktop8.dll";

                // let's try and find the dll
                var dllLocations = new List<string>()
                {
                    GetDllLocationFromRegistry(),

                    // fallback path
                    Path.Combine(GetProgramFilesx86(), "Microsoft Band Sync", dllName)
                };

                foreach (string location in dllLocations)
                {
                    if (File.Exists(location))
                    {
                        return Assembly.LoadFrom(location);
                    }
                }

                throw new FileNotFoundException("Could not find Band Sync app on your machine. I looked in:\n\n" + string.Join("\n", dllLocations));
            }

            return null;
        }

        private static string GetDllLocationFromRegistry()
        {
            var sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.ToString();

            var regRoot = Microsoft.Win32.RegistryHive.LocalMachine;
            string regKeyName = @"SOFTWARE\MICROSOFT\Windows\CurrentVersion\Installer\UserData\" + sid + @"\Components\1C4501C98808F3A5CBF549E17D39406F";
            string regValueName = "D9E6F917E27BA454F9E448BCA68562DE";

            RegistryKey regKey;

            if (Environment.Is64BitOperatingSystem) 
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Registry64);
            } 
            else 
            {
                regKey = RegistryKey.OpenBaseKey(regRoot, RegistryView.Default);
            }

            regKey = regKey.OpenSubKey(regKeyName);

            if (regKey != null)
            {
                return regKey.GetValue(regValueName).ToString();
            }

            return "[not found] " + regKeyName + "\\" + regValueName;

        }

        private static string GetProgramFilesx86()
        {
            var envVar = (Environment.Is64BitProcess ? "ProgramFiles(x86)" : "ProgramFiles");

            return Environment.GetEnvironmentVariable(envVar);
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
            if (DesktopSyncAppIsRunning())
                return;

            var devices = await CargoClient.GetConnectedDevicesAsync();

            if (devices.Count() > 0)
            {
                // must create on the UI thread for various Binding reasons
                _deviceInfo = devices[0];

                Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    // TODO: support more than one device?
                    InitializeCargoLogging();
                    CargoClient = await CargoClient.CreateAsync(_deviceInfo);
                
                    IsConnected = true;

                    //TODO: call an "OnConnected" function
                    Properties = new BandProperties(CargoClient);
                    Theme = new BandTheme(CargoClient);
                    Sensors = new BandSensors(CargoClient);
                    Tiles = new BandTiles(CargoClient);
                }));
            }
        }

        private bool DesktopSyncAppIsRunning()
        {
            return IsDesktopSyncAppRunning = (System.Diagnostics.Process.GetProcessesByName("Microsoft Band Sync").Length > 0);
        }

        private void InitializeCargoLogging()
        {
            // get log instance
            var field = typeof(Logger).GetField("traceListenerInternal", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, BandLogger.Instance);
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
