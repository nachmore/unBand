using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using unBand.Cloud;

namespace unBand.CloudHelpers
{
    public class BandEventViewModel :INotifyPropertyChanged
    {
        public BandEventBase Event { get; private set; }

        private bool _hasGPSPoints = false;
        public bool HasGPSPoints
        {
            get { return _hasGPSPoints; }
            set
            {
                if (_hasGPSPoints != value)
                {
                    _hasGPSPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BandCloudClient _cloud;
        private bool _loaded = false;

        public bool Loaded
        {
            get { return _loaded; }
            set
            {
                if (_loaded != value)
                {
                    _loaded = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public BandEventViewModel(BandCloudClient cloud, BandEventBase cloudEvent)
        {
            _cloud = cloud;

            Event = cloudEvent;

            if (Event is UserActivity)
            {
                // this event type is considered "Loaded" already since we get all of the information
                // from the initial API call
                Loaded = true;
            }
        }

        public async Task LoadFull()
        {
            if (!Loaded)
            {
                Event.InitFullEventData(await _cloud.GetFullEventData(Event.EventID, Event.Expanders));

                Loaded = true;

                HasGPSPoints = (Event is RunEvent) && ((RunEvent)Event).HasGPSPoints;
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
