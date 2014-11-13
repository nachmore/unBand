using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace unBand.BandHelpers.Sensors 
{
    public class BandPedometer: INotifyPropertyChanged
    {

        private CargoClient _client;
        private uint _totalSteps;
        private uint _totalMovements;

        public uint TotalSteps
        {
            get { return _totalSteps; }
            set
            {
                if (_totalSteps != value)
                {
                    _totalSteps = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public uint TotalMovements
        {
            get { return _totalMovements; }
            set
            {
                if (_totalMovements != value)
                {
                    _totalMovements = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandPedometer(CargoClient client)
        {
            _client = client;

            Init();
        }

        private void Init()
        {
            OneTimePedometerReading();
        }

        /// <summary>
        /// To start us off we get an initial, one-off, Pedometer reading.
        /// To get consistent updates use TODO: StartPedometer();
        /// </summary>
        private async void OneTimePedometerReading()
        {
            _client.PedometerUpdated += _client_OneTimePedometerUpdated;
            await _client.SensorSubscribeAsync(SensorType.Pedometer);
        }

        void _client_OneTimePedometerUpdated(object sender, PedometerUpdatedEventArgs e)
        {
            _client.SensorUnsubscribe(SensorType.Pedometer);
            
            TotalSteps = e.TotalSteps;
            TotalMovements = e.TotalMovements;

            _client.PedometerUpdated -= _client_OneTimePedometerUpdated;
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
