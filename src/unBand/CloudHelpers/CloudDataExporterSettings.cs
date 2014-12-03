using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace unBand.CloudHelpers
{
    /// <summary>
    /// For now none of the properties call NotifyPropertyChanged as they are driven by the UI
    /// </summary>
    public class CloudDataExporterSettings : INotifyPropertyChanged
    {
        public bool ExportSteps { get; set; }

        public bool ExportWorkouts { get; set; }
        public bool ExportSleep { get; set; }
        public bool ExportRuns { get; set; }

        /// <summary>
        /// If true, overrides ExportCount
        /// </summary>
        public bool ExportAll { get; set; }
        public int ExportCount { get; set; }
        
        public bool ConvertDateTimeToLocal { get; set; }
        
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
