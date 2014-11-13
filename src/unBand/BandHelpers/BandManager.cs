using Microsoft.Cargo.Client;
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
    class BandManager : INotifyPropertyChanged
    {

        #region Singleton

        private static BandManager _theOne;

        public static BandManager Instance
        {
            get
            {
                if (_theOne == null)
                    _theOne = new BandManager();

                return _theOne;
            }
        }

        #endregion

        private bool _isConnected = false;
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
            Init();
        }

        private void Init()
        {
            // TODO: change this from a one off discover call to a continuous monitor
            DiscoverDevices();
        }

        private async void DiscoverDevices()
        {
            
            var devices = await CargoClient.GetConnectedDevicesAsync();

            if (devices.Count() > 0)
            {
                // TODO: support more than one device?
                CargoClient = await CargoClient.CreateAsync(devices[0]);
                
                IsConnected = true;

                //TODO: call an "OnConnected" function
                Properties = new BandProperties(CargoClient);
                Theme = new BandTheme(CargoClient);
                Sensors = new BandSensors(CargoClient);
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
