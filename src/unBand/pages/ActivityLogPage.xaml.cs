﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Cargo.Client;
using Microsoft.Live;
using Microsoft.Live.Desktop;
using Microsoft.Win32;
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
using unBand.CloudHelpers;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for MyBandPage.xaml
    /// </summary>
    public partial class ActivityLogPage : UserControl, INotifyPropertyChanged
    {

        private BandManager _band;
        ProgressDialogController _progressDialog;

        public List<BandEventBase> Events { get; set; }

        public Dictionary<string, CloudDataExporter> Exporters { get { return _exporters; } }

        private Dictionary<string, CloudDataExporter> _exporters = new Dictionary<string, CloudDataExporter>()
        {
            {"CSV", new CSVExporter()},
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
        }

        private void LoadExportSettings()
        {
            ExportSettings = new CloudDataExporterSettings();

            ExportSettings = (Settings.Current.ExportSettings != null ? 
                                Settings.Current.ExportSettings :
                                new CloudDataExporterSettings());
        }

        private void SaveExportSettings()
        {
            Settings.Current.ExportSettings = ExportSettings;
            Settings.Current.Save();
        }

        private void btnExportLast100_Click(object sender, RoutedEventArgs e)
        {
            ExportEvents(100);
        }

        private void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            ExportEvents();            
        }

        private async void ExportEvents(int? count = null)
        {
            if (_exporter.Value == null)
            {
                MessageBox.Show("Coming soon...");
                return;
            }

            var saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.FileName = "band_export.csv";
            saveDialog.DefaultExt = _exporter.Value.DefaultExt;

            var result = saveDialog.ShowDialog();

            if (result == true)
            {
                var gpxPath = "";

                if (ExportSettings.ExportGPXRunEvents)
                {
                    var dialog = new System.Windows.Forms.FolderBrowserDialog();
                    dialog.Description = "Select GPX folder";

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        gpxPath = dialog.SelectedPath;
                    }
                }

                _progressDialog = await ((MetroWindow)(Window.GetWindow(this))).ShowProgressAsync("Exporting Data", "...");
                _progressDialog.SetCancelable(true); // TODO: this needs to be implemented. No event?
                _progressDialog.SetProgress(0);

                var progressIndicator = new Progress<BandCloudExportProgress>(ReportProgress);

                _exporter.Value.Settings = ExportSettings;

                await BandCloudManager.Instance.ExportEventsSummary(count, _exporter.Value, saveDialog.FileName, gpxPath, progressIndicator);

                _progressDialog.CloseAsync();

                if (ExportSettings.OpenFileAfterExport)
                {
                    Process.Start(saveDialog.FileName);
                    if (gpxPath != "")
                    {
                        Process.Start("explorer.exe", gpxPath);
                    }
                }

                SaveExportSettings();
            }
        }

        void ReportProgress(BandCloudExportProgress value)
        {
            // TODO: handle 0 events to export

            if (value.TotalEventsToExport <= 0)
            {
                _progressDialog.SetIndeterminate();
            }
            else
            {
                _progressDialog.SetProgress(((double)(value.ExportedEventsCount) / value.TotalEventsToExport));
            }

            if (!string.IsNullOrEmpty(value.StatusMessage))
                _progressDialog.SetMessage(value.StatusMessage);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportEvents();
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
