using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public bool CollapseRepeatedMessages { get; set; }

        private string _lastFormattedMessage;

        private BandLogger() 
        {
            CollapseRepeatedMessages = true;
        }

        public override void Log(LogLevel level, string message, params object[] args)
        {
            string newMessage = String.Format(message, args);

            if (CollapseRepeatedMessages && newMessage == _lastFormattedMessage)
            {
                // Current will be null when shutting down
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lock (_logOutput)
                        {

                            // essentially, pop the current message, extract the count from the text
                            // and stick it back on with the new count. We could save the count, but there's
                            // no benefit.

                            string repeatedLine = _logOutput[0];
                            _logOutput.RemoveAt(0);

                            var re = new Regex(@" \((\d+)\)$");
                            var match = re.Match(repeatedLine);
                            var count = 1;

                            if (match.Success)
                            {
                                count = int.Parse(match.Groups[1].Value);

                                count++;

                                repeatedLine = re.Replace(repeatedLine, "");
                            }

                            repeatedLine += " (" + count + ")";

                            _logOutput.Insert(0, repeatedLine);
                        }
                    }));
                }

                return;
            }

            _lastFormattedMessage = newMessage;

            string log = DateTime.Now + ": " + level.ToString() + ": " + newMessage;

            // Current will be null when shutting down
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lock (_logOutput)
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
