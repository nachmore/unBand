using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unBand.Cloud;

namespace unBand.CloudHelpers
{
    public class BandCloudManager
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

        internal async void ExportEvents(string fileName, int? count)
        {
            // TODO: Still need to find a better way to load events incrementally
            if (count == null)
                count = 10000000; // suitably large number to encompass "everything".

            if (Events.Count < count)
            {
                LoadEvents((int)count);
            }

            // we have now downloaded the correct number of events, export them
            await ExportEvents(Events.Take(Events.Count > (int)count ? (int)count : Events.Count));
        }

        private async Task ExportEvents(IEnumerable<BandEventViewModel> bandEvents)
        {
            // TODO: set more logical initial capacity?
            var csv = new StringBuilder(500000);

            foreach (var bandEvent in bandEvents)
            {
                await bandEvent.Load();
                bandEvent.Event.ToCSV();
            }
        }
    }
}
