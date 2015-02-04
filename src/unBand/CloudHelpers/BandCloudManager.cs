using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using unBand.Cloud;
using unBand.Cloud.Exporters.EventExporters.Helpers;

namespace unBand.CloudHelpers
{
    public class BandCloudExportProgress
    {
        public string StatusMessage { get; set; }
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

        private const int MAX_TOP_REQUEST = 1500;

        private BandCloudClient _cloud;

        // TODO: this event needs to convey success or failure
        public event BandCloudAuthComplete AuthenticationCompleted;

        /// <summary>
        /// Signal that any ongoing full export should be cancelled.
        /// This will be flipped back to false when the request has been acknowledged
        /// </summary>
        public bool CancelFullExport { get; set; }

        public ObservableCollection<BandEventViewModel> Events { get; set; }

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private BandCloudManager()
        {
            Events = new ObservableCollection<BandEventViewModel>();

            _cloud = new BandCloudClient();
            _cloud.AuthenticationCompleted += _cloud_AuthenticationCompleted;

            Login();
        }

        public void Login() 
        {
            if (!IsLoggedIn)
                _cloud.Login();
        }

        /// <summary>
        /// Perform initial init after authentication completes
        /// </summary>
        void _cloud_AuthenticationCompleted()
        {
            IsLoggedIn = true;

            if (AuthenticationCompleted != null)
                AuthenticationCompleted();
        }

        /// <summary>
        /// Proxy for BandCloudClient.GetEvents which wraps loaded Events into a wrapper which is a little more
        /// friendly for Binding against objects which are often only partially loaded.
        /// </summary>
        /// <param name="topCount">Limited for some requests (in particular /UserActivities) to 1500</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>Nothing. DataBind against Events.</returns>
        public async Task LoadEvents(int? topCount = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            List<BandEventBase> userDailyActivities = null;

            // using topCount for dailyEvents makes no sense, since it seems to return (topCount)hours since the Band started
            // reporting data, which is somewhat useless. So if the dates weren't provided, let's just load up the max range (7 days)
            // Note: there seems to be a bug in the Health cloud service that lets you pull of the data in one swoop, but I don't
            //       want to be naughty (and at some point that will be a ton of data for one request)
            if (startDate == null && endDate == null)
            {
                var end = DateTime.UtcNow;
                var start = end.AddDays(-7);
                userDailyActivities = await _cloud.GetUserActivity(0, start, end);
            }

            if (userDailyActivities != null)
            {
                // userDailyActivities will be a sorted list from earliest to last, but the other activities (below)
                // are retrieved in reverse order, so reverse the list.
                userDailyActivities.Reverse();
            }

            // for now, don't limit the request limit, since we're hitting requests that don't care
            // topCount = (topCount > MAX_TOP_REQUEST ? MAX_TOP_REQUEST : topCount);

            var events = await _cloud.GetEvents(topCount, startDate, endDate);

            var curDailyActivity = (userDailyActivities == null ? -1 : 0);

            foreach (var cloudEvent in events)
            {
                // make sure to intersperse daily activities in the proper location with regular activities
                // essentially if we hit a curDailyActivity that is more recent (or the same) as the current activity
                // then dump it to the list. This guarantees that the daily counts are topmost in the list for a given date
                // and that if you have a bunch of Daily Activity logs with nothing in between (no sleep, exercise etc) then
                // they will be dumped in one after another
                while (curDailyActivity > -1 && curDailyActivity < userDailyActivities.Count && userDailyActivities[curDailyActivity].StartTime.Date >= cloudEvent.StartTime.Date)
                {
                    Events.Add(new BandEventViewModel(_cloud, userDailyActivities[curDailyActivity]));

                    curDailyActivity++;
                }

                Events.Add(new BandEventViewModel(_cloud, cloudEvent));
            }

            // if we have left over daily activities, dump them now
            for (; curDailyActivity < userDailyActivities.Count; curDailyActivity++)
            {
                Events.Add(new BandEventViewModel(_cloud, userDailyActivities[curDailyActivity]));
            }

        }

        public async Task ExportEventsSummaryToCSV(int? count, CloudDataExporterSettings settings, string fileName, IProgress<BandCloudExportProgress> progress)
        {
            // TODO: Still need to find a better way to load events incrementally
            if (count == null)
                count = 10000000; // suitably large number to encompass "everything".

            if (Events.Count < count)
            {
                // clear out our existing events, since they're going to be replaced
                Events.Clear();

                progress.Report(new BandCloudExportProgress() { TotalEventsToExport = 0, StatusMessage = "Downloading Events..." });

                await LoadEvents((int)count);
            }

            // we have now downloaded the correct number of events, export them
            // Note: Take will take min(Events.Count, count)
            await ExportEventsSummary(
                Events.Where(e =>
                        {
                            return (settings.IncludeRuns && e.Event.EventType == BandEventType.Running) ||
                                   (settings.IncludeSleep && e.Event.EventType == BandEventType.Sleeping) ||
                                   (settings.IncludeWorkouts && (e.Event.EventType == BandEventType.GuidedWorkout || e.Event.EventType == BandEventType.Workout));
                        }
                    )
                    .Take((int)count), 
                settings, 
                fileName, 
                progress
            );
        }

        private async Task ExportEventsSummary(IEnumerable<BandEventViewModel> bandEvents, CloudDataExporterSettings settings, string filePath, IProgress<BandCloudExportProgress> progress)
        {
            // TODO: set more logical initial capacity?
            var csv = new StringBuilder(500000);
            var dataToDump = new List<Dictionary<string, object>>(100);

            var progressReport = new BandCloudExportProgress() { TotalEventsToExport = bandEvents.Count(), StatusMessage = "Exporting Events..." };

            progress.Report(progressReport);
            await Task.Yield();

            foreach (var bandEvent in bandEvents)
            {
                // TODO: I hate this pattern. I should be able to just tell the CloudClient to download all of the data for my event,
                //       or tell the event to download the data itself
                // TODO: This fits ExportsEventsFull, not Summary
                //var data = await _cloud.GetFullEventData(bandEvent.Event.EventID, bandEvent.Event.Expanders);
                //bandEvent.Event.InitFullEventData(data);

                dataToDump.Add(bandEvent.Event.DumpBasicEventData());

                progressReport.ExportedEventsCount++;
                progress.Report(progressReport);

                await Task.Yield(); // since we need to update progress, make sure to yield for a bit
            }

            CSVExporter.ExportToFile(dataToDump, filePath);
        }

        /// <summary>
        /// Make sure to call LoadEvents() first, this will then dump everything in Events
        /// </summary>
        public async Task ExportFullEventData(string folder, CloudDataExporterSettings settings, IProgress<BandCloudExportProgress> progress) 
        {
            // TODO: there is a limit to how quickly we can hit the service. This limit should be enforced at 
            //       a global level
            var progressReport = new BandCloudExportProgress() { TotalEventsToExport = Events.Count };

            foreach (var eventViewModel in Events)
            {
                if (CancelFullExport)
                {
                    CancelFullExport = false;

                    return;
                }

                ++progressReport.ExportedEventsCount;
                progressReport.StatusMessage = "Loading data for activity " + progressReport.ExportedEventsCount + "/" + Events.Count + " (" + eventViewModel.Event.FriendlyEventType + ")\n\nNote: This can take a while, we are restricted to loading full data for one Activity per 10 seconds.";
                progress.Report(progressReport);

                System.Diagnostics.Debug.WriteLine("loading " + progressReport.ExportedEventsCount);

                await eventViewModel.LoadFull();

                var bandEvent = eventViewModel.Event;

                foreach (var exporter in bandEvent.Exporters)
                {
                    var dateTime = (settings.ConvertDateTimeToLocal ? bandEvent.StartTime.ToLocalTime() : bandEvent.StartTime);
                    var filePath = System.IO.Path.Combine(folder, bandEvent.FriendlyEventType + "_" + dateTime.ToString("yyyyMMdd_hhmm") + "_" + exporter.DefaultExportSuffix + exporter.DefaultExtension);

                    // export in the background, no await
                    exporter.ExportToFile(bandEvent, filePath);
                }

                // we are restricted to loading an event / 10 seconds (approximately). See https://github.com/nachmore/unBand/issues/20
                // for more details
                await Task.Delay(10000);
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
