using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace unBand
{
    /// <summary>
    /// Since regular WinForms / WPF settings do not play nicely with ClickOnce, let's roll our own
    /// </summary>
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        #region Singleton

        private static Settings _theOne;
        public static Settings Current { 
            get 
            {
                if (_theOne == null)
                {
                    _theOne = Load();
                }

                return _theOne;
            } 
        }

        #endregion

        #region Statics

        private static object _lock = new object();

        /// <summary>
        /// The assumption for this application is that settings are not that important, so if they
        /// do not exist or get corrupted we simply return the defaults
        /// </summary>
        /// <returns></returns>
        private static Settings Load()
        {
            lock (_lock)
            {
                // we're not double checking pre-lock since we're only ever called from Instance.get()
                // and we want that to wait until object init is done if someone actually happens to get 
                // stuck at the lock
                if (_theOne != null)
                    return _theOne;

                var settingsFile = GetSettingsFilePath();

                // if the file does *not* exist then we can just ignore the rest of load, as we'll just
                // continue on with the defaults and at some point the main program should call Save()
                if (!File.Exists(settingsFile))
                {
                    return new Settings();
                }

                using (var sr = new StreamReader(settingsFile))
                {
                    var xmlSerializer = new XmlSerializer(typeof(Settings));

                    Settings deserialized = null;

                    try
                    {
                        deserialized = xmlSerializer.Deserialize(sr) as Settings;
                    }
                    catch { } // generally == corrupted Settings file

                    if (deserialized == null)
                    {
                        return new Settings();
                    }

                    // no longer the first run if we're loading these Settings from disk
                    if (deserialized.FirstRun)
                        deserialized.FirstRun = false;

                    return deserialized;
                }
            }
        }

        /// <summary>
        /// Settings path is not user configurable (and even if it was, it would be in a regkey which
        /// we could check here), so return the relatively static string. Could be cached, if we save often
        /// enough and the cost of the call becomes significant.
        /// </summary>
        /// <returns></returns>
        private static string GetSettingsFilePath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "unBand");

            // TODO: creating a file here feels like a dirty side affect
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return Path.Combine(dir, "unBand.settings");
        }

        #endregion

        private bool _agreedToFirstRunWarning;
        public bool AgreedToFirstRunWarning
        {
            get { return _agreedToFirstRunWarning; }
            set
            {
                if (_agreedToFirstRunWarning != value)
                {
                    _agreedToFirstRunWarning = value;
                    
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _agreedToTelemetry;
        public bool AgreedToTelemetry
        {
            get { return _agreedToTelemetry; }
            set
            {
                if (_agreedToTelemetry != value)
                {
                    _agreedToTelemetry = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private CloudHelpers.CloudDataExporterSettings _exportSettings;
        public CloudHelpers.CloudDataExporterSettings ExportSettings
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

        private Guid _device;
        public Guid Device
        {
            get { return _device; }
            set
            {
                if (_device != value)
                {
                    _device = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private bool _firstRun;
        public bool FirstRun
        {
            get { return _firstRun; }
            set
            {
                if (_firstRun != value)
                {
                    _firstRun = value;

                    NotifyPropertyChanged();
                }
            }
        }

        private Settings() 
        {
            Default();
            Save(); // save the defaults since some of them may be randomly generated
        }
        
        public void Save()
        {
            using (var sw = new StreamWriter(GetSettingsFilePath(), false))
            {
                var xmlSerializer = new XmlSerializer(typeof(Settings));
                xmlSerializer.Serialize(sw, this);
            }
        }

        private void Default()
        {
            AgreedToFirstRunWarning = false;
            AgreedToTelemetry = false;
            Device = Guid.NewGuid();
            FirstRun = false;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null && Application.Current != null)
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
