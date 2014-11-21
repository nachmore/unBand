using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace unBand.BandHelpers
{
    public class BandLogger : TraceListenerBase
    {

#region Singleton

        private static BandLogger _theOne;

        public static BandLogger Instance
        {
            get
            {
                if (_theOne == null)
                {
                    _theOne = new BandLogger();
                }

                return _theOne;
            }
        }


#endregion

        public ObservableCollection<string> _logOutput = new ObservableCollection<string>();

        public ObservableCollection<string> LogOutput { get { return _logOutput; } }

        private BandLogger() { }

        public override void Log(LogLevel level, string message, params object[] args)
        {
            string log = DateTime.Now + ": " + level.ToString() + ": " + String.Format(message, args);

            //System.Diagnostics.Debug.WriteLine(level.ToString() + ": " + String.Format(message, args));

            // TODO: add option to collapse log if the last log message is identical to the current one

            // Current will be null when shutting down
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lock (log)
                    {
                        _logOutput.Insert(0, log);
                        if (_logOutput.Count > 100000)
                        {
                            _logOutput.RemoveAt(_logOutput.Count - 1);
                        }
                    }
                }));
            }
        }

        
    }
}
