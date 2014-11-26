using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace unBand.Cloud
{
    public class UserGuidedWorkoutInfoSegment
    {
    }

    public class UserGuidedWorkoutEventConverter : TypeConverter 
    {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is JObject) 
            {
                return new UserGuidedWorkoutEvent((JObject)value);
            }

            return null;
        }
    }

    [TypeConverter(typeof(UserGuidedWorkoutEventConverter))]
    public class UserGuidedWorkoutEvent : BandEventBase
    {
        public List<UserGuidedWorkoutInfoSegment> Segments { get; private set; }

        public UserGuidedWorkoutEvent(JObject json)
            : base(json)
        {
            Segments = new List<UserGuidedWorkoutInfoSegment>();
            InitFromDynamic((dynamic)json);
        }

        public override void DownloadAllData()
        {

        }

        /// <summary>
        /// Creates a SleepEvent object that is intialized from the summary JSON returned by GetEvents()
        ///  
        /// To fill in detailed information about this event a call to DownloadAllData() is required.
        /// </summary>
        /// <param name="rawEvent"></param>
        /// <returns></returns>
        private void InitFromDynamic(dynamic rawEvent)
        {

        }
    }
}
