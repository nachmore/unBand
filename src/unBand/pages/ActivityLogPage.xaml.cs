﻿using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Cargo.Client;
using Microsoft.Live;
using Microsoft.Live.Desktop;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using unBand.BandHelpers;
using unBand.Cloud;
using unBand.Cloud.Exporters.EventExporters;
using unBand.CloudHelpers;
using unBand.MapHelpers;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for MyBandPage.xaml
    /// </summary>
    public partial class ActivityLogPage : UserControl, INotifyPropertyChanged
    {

        private BandManager _band;
        ProgressDialogController _summaryExportProgressDialog;
        ProgressDialogController _fullExportProgressDialog;

        public Dictionary<string, CloudDataExporter> Exporters { get { return _exporters; } }

        private Dictionary<string, CloudDataExporter> _exporters = new Dictionary<string, CloudDataExporter>()
        {
            {"CSV", null},
            {"Excel", null}
        };

        private KeyValuePair<string, CloudDataExporter> _exporter;
        public KeyValuePair<string, CloudDataExporter> Exporter
        {
            get { return _exporter; }
            set
            {
                if (!value.Equals(_exporter))
                {
                    _exporter = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private CloudDataExporterSettings _exportSettings;
        public CloudDataExporterSettings ExportSettings
        {
            get { return _exportSettings; }
            set
            {
                if (_exportSettings != value)
                {
                    _exportSettings = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ActivityLogPage()
        {
            InitializeComponent();

            // set the default exporter
            // TODO: Restore last used
            Exporter = Exporters.First();

            LoadExportSettings();

            _band = BandManager.Instance;

            this.DataContext = BandCloudManager.Instance;

            BandCloudManager.Instance.AuthenticationCompleted += Instance_AuthenticationCompleted;
        }

        void Instance_AuthenticationCompleted()
        {
            // BUG #19: When loading events has less impact on the UI / a smooth loading indicator 
            //          reenable this code
            //BandCloudManager.Instance.LoadEvents(10);
        }

        private void LoadExportSettings()
        {
            ExportSettings = (Settings.Current.ExportSettings ?? new CloudDataExporterSettings());
        }

        private void SaveExportSettings()
        {
            Settings.Current.ExportSettings = ExportSettings;
            Settings.Current.Save();
        }

        private async void ExportEventSummaryToCSV(int? count = null)
        {
            Telemetry.TrackEvent(TelemetryCategory.Export, Telemetry.TelemetryEvent.Export.Summary);

            var saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.FileName = "band_export.csv";
            saveDialog.DefaultExt = ".csv";

            var result = saveDialog.ShowDialog();

            if (result == true)
            {
                _summaryExportProgressDialog = await ((MetroWindow)(Window.GetWindow(this))).ShowProgressAsync("Exporting Data", "...");
                _summaryExportProgressDialog.SetCancelable(true); // TODO: this needs to be implemented. No event?
                _summaryExportProgressDialog.SetProgress(0);

                var progressIndicator = new Progress<BandCloudExportProgress>(ReportSummaryExportProgress);

                await BandCloudManager.Instance.ExportEventsSummaryToCSV(count, ExportSettings, saveDialog.FileName, progressIndicator);

                _summaryExportProgressDialog.CloseAsync();

                if (ExportSettings.OpenFileAfterExport)
                {
                    Process.Start(saveDialog.FileName);                    
                }

                SaveExportSettings();
            }
        }

        void ReportSummaryExportProgress(BandCloudExportProgress value)
        {
            // TODO: handle 0 events to export

            if (value.TotalEventsToExport <= 0)
            {
                _summaryExportProgressDialog.SetIndeterminate();
            }
            else
            {
                _summaryExportProgressDialog.SetProgress(((double)(value.ExportedEventsCount) / value.TotalEventsToExport));
            }

            if (!string.IsNullOrEmpty(value.StatusMessage))
                _summaryExportProgressDialog.SetMessage(value.StatusMessage);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportEventSummaryToCSV(ExportSettings.ExportAll ? (int?)null : 100);
        }

        private async void btnLoadEvents_Click(object sender, RoutedEventArgs e)
        {
            SaveExportSettings();

            await LoadEvents();
        }

        private async Task LoadEvents() 
        {
            await BandCloudManager.Instance.LoadEvents(ExportSettings.ExportAll ? 1000000 : 100);
        }

        private async void lstEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // when an Event is selected in the ListBox we need to load the full Event

            var item = ((ListBox)sender).SelectedItem as BandEventViewModel;

            if (item != null)
            {
                await item.LoadFull();
            }

            if(item.Event.EventType == BandEventType.Running && item.HasGPSPoints)
                UpdateMap(item.Event as RunEvent);
        }
        private void btnExportToGPX_Click(object sender, RoutedEventArgs e)
        {
            var runEvent = ((BandEventViewModel)lstEvents.SelectedItem).Event as RunEvent;

            if (runEvent != null)
            {
                var exporter = GPXExporter.Instance;

                var saveDialog = new SaveFileDialog();
                saveDialog.AddExtension = true;
                saveDialog.FileName = "run_" + runEvent.EventID + ".gpx"; // TODO: better auto-generated name?
                saveDialog.DefaultExt = exporter.DefaultExtension;

                var result = saveDialog.ShowDialog();

                if (result == true)
                {
                    exporter.ExportToFile(runEvent, saveDialog.FileName);
                }
            }
        }
        
        private async void UpdateMap(RunEvent inputIvent)
        {
            MapControl.Markers.Clear();
            MapControl.MapProvider = GMapProviders.GoogleMap;
            MapControl.Manager.Mode = GMap.NET.AccessMode.ServerOnly;

            var points = inputIvent.MapPoints.Select(x => new PointLatLng(Convert.ToDouble(x.Latitude), Convert.ToDouble(x.Longitude)));

            foreach (var pnt in points)
            {
                var mrk = new GMapMarker(pnt);

                if (points.First() == pnt)
                    mrk.Shape = MapShapes.GetStartShape("Start!");
                else if (points.Last() == pnt)
                    mrk.Shape = MapShapes.GetEndShape("End!");
                else
                    mrk.Shape = MapShapes.GetDataPointShape();

                mrk.ZIndex = 1000;
                MapControl.Markers.Add(mrk);
            }

            MapControl.IgnoreMarkerOnMouseWheel = true;
            MapControl.DragButton = MouseButton.Left;
            MapControl.ZoomAndCenterMarkers(null);
        }

        private void RdbMap_Click(object sender, RoutedEventArgs e)
        {
            var SrcBtn = sender as RadioButton;

            switch (SrcBtn.Name)
            {
                case "RdbMapHybrid":
                    MapControl.MapProvider = GMapProviders.GoogleHybridMap;
                    break;
                case "RdbMapSatellite":
                    MapControl.MapProvider = GMapProviders.GoogleSatelliteMap;
                    break;
                case "RdbMapTerrain":
                default:
                    MapControl.MapProvider = GMapProviders.GoogleTerrainMap;
                    break;
            }
        }

        private async void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new CommonOpenFileDialog();
            folderDialog.Title = "Where should I save your data?";
            folderDialog.IsFolderPicker = true;
            folderDialog.EnsureFileExists = true;
            folderDialog.EnsurePathExists = true;
            folderDialog.EnsureValidNames = true;
            folderDialog.EnsureReadOnly = false;
            folderDialog.Multiselect = false;
            folderDialog.ShowPlacesList = true;

            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Telemetry.TrackEvent(TelemetryCategory.Export, Telemetry.TelemetryEvent.Export.Full);

                var folder = folderDialog.FileName;

                BandCloudManager.Instance.Events.Clear();

                _fullExportProgressDialog = await ((MetroWindow)(Window.GetWindow(this))).ShowProgressAsync("Exporting Full Activity Data", "Loading Activities list...");
                _fullExportProgressDialog.SetCancelable(true); // TODO: this needs to be implemented. No event?
                _fullExportProgressDialog.SetIndeterminate();

                // HACK HACK HACK HACK
                // TODO: add a Cancelled Event into the MahApps.Metro library

                // polling method to cancel the export if the user requests that it be cancelled
                Task.Run(async () =>
                {
                    while (_fullExportProgressDialog != null && _fullExportProgressDialog.IsOpen)
                    {
                        if (_fullExportProgressDialog.IsCanceled)
                        {
                            BandCloudManager.Instance.CancelFullExport = true;

                            Telemetry.TrackEvent(TelemetryCategory.Export, Telemetry.TelemetryEvent.Export.FullCancelled);

                            // we'd exit from the while loop anyway, but only when the progress dialog finally exits
                            // which can take up to 10 seconds, so might as well shut this down asap
                            return; 
                        }

                        await Task.Delay(500);
                    }
                });

                await LoadEvents();

                var progressIndicator = new Progress<BandCloudExportProgress>(ReportFullExportProgress);
                
                // TODO: progress reporter
                await BandCloudManager.Instance.ExportFullEventData(folder, ExportSettings, progressIndicator);

                _fullExportProgressDialog.CloseAsync();

                SaveExportSettings();
            }
        }

        void ReportFullExportProgress(BandCloudExportProgress value)
        {
            // TODO: handle 0 events to export

            if (value.TotalEventsToExport <= 0)
            {
                _fullExportProgressDialog.SetIndeterminate();
            }
            else
            {
                _fullExportProgressDialog.SetProgress(((double)(value.ExportedEventsCount) / value.TotalEventsToExport));
            }

            if (!string.IsNullOrEmpty(value.StatusMessage))
                _fullExportProgressDialog.SetMessage(value.StatusMessage);
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
