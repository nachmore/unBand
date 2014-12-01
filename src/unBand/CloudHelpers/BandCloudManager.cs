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
            LoadMoreEvents();
        }

        private async void LoadMoreEvents(int top = 100)
        {
            var events = await _cloud.GetEvents(top);

            foreach (var cloudEvent in events) 
            {
                Events.Add(new BandEventViewModel(cloudEvent));
            }
        }
    }
}
