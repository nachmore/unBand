using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using unBand.Cloud;

namespace unBand.CloudHelpers
{
    public class BandCloudExportProgress
    {
        public int TotalEventsToExport { get; set; }
        public int ExportedEventsCount { get; set; }
    }

    public class BandCloudManager : INotifyPropertyChanged
    {

        #region Singleton

        private static BandCloudManager _instance;

        public static BandCloudManager Instance
        {
            get 
            {
                if (_instance == null)
                {
                    _instance = new BandCloudManager();
                }

                return _instance; 
            }
            set
            {
                _instance = value;
            }
        }

        #endregion

        private BandCloudClient _cloud;

        public ObservableCollection<BandEventViewModel> Events { get; set; }

        private BandCloudManager()
        {
            Events = new ObservableCollection<BandEventViewModel>();

            _cloud = new BandCloudClient();
            _cloud.AuthenticationCompleted += _cloud_AuthenticationCompleted;
            _cloud.Login();
        }

        /// <summary>
        /// Perform initial init after authentication completes
        /// </summary>
        void _cloud_AuthenticationCompleted()
        {
            LoadEvents();
        }

        /// <summary>
        /// TODO: Find a way to load events within a range instead of just from the top
        /// </summary>
        /// <param name="top"></param>
        private async void LoadEvents(int top = 100)
        {
            var events = await _cloud.GetEvents(top);

            foreach (var cloudEvent in events) 
            {
                Events.Add(new BandEventViewModel(cloudEvent));
            }
        }

        internal async Task ExportEvents(string fileName, int? count, IProgress<BandCloudExportProgress> progress)
        {
            // TODO: Still need to find a better way to load events incrementally
            if (count == null)
                count = 10000000; // suitably large number to encompass "everything".

            if (Events.Count < count)
            {
                LoadEvents((int)count);
            }

            // we have now downloaded the correct number of events, export them
            await ExportEvents(Events.Take(Events.Count > (int)count ? (int)count : Events.Count), progress);
        }

        private async Task ExportEvents(IEnumerable<BandEventViewModel> bandEvents, IProgress<BandCloudExportProgress> progress)
        {
            // TODO: set more logical initial capacity?
            var csv = new StringBuilder(500000);
            var dataToDump = new List<Dictionary<string, string>>(100);

            var progressReport = new BandCloudExportProgress() { TotalEventsToExport = bandEvents.Count()};

            progress.Report(progressReport);

            foreach (var bandEvent in bandEvents)
            {
                // TODO: I hate this pattern. I should be able to just tell the CloudClient to download all of the data for my event,
                //       or tell the event to download the data itself
                var data = await _cloud.GetFullEventData(bandEvent.Event.EventID, bandEvent.Event.Expanders);
                bandEvent.Event.InitFullEventData(data);

                dataToDump.Add(bandEvent.Event.GetRawSummary());

                progressReport.ExportedEventsCount++;
                progress.Report(progressReport);
            }

            //TODO: dump data
            System.Diagnostics.Debug.WriteLine(dataToDump.Count);
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
