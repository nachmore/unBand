using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using unBand.Cloud;

namespace unBand
{
    class EventTypeToImageConverter : IValueConverter
    {
        private static BitmapImage _imageWorkout = new BitmapImage(new Uri("/assets/icons/activity_workout.png", UriKind.RelativeOrAbsolute));
        private static BitmapImage _imageGuidedWorkout = new BitmapImage(new Uri("/assets/icons/activity_guided_workout.png", UriKind.RelativeOrAbsolute));
        private static BitmapImage _imageSleeping = new BitmapImage(new Uri("/assets/icons/activity_sleep.png", UriKind.RelativeOrAbsolute));
        private static BitmapImage _imageRunning = new BitmapImage(new Uri("/assets/icons/activity_run.png", UriKind.RelativeOrAbsolute));
        private static BitmapImage _imageDailyActivity = new BitmapImage(new Uri("/assets/icons/user_activity.png", UriKind.RelativeOrAbsolute));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            var eventType = value as BandEventType?;

            if (eventType == null) 
                return null;

            switch (eventType)
            {
                case BandEventType.GuidedWorkout:
                    return _imageGuidedWorkout;
                case BandEventType.Workout:
                    return _imageWorkout;
                case BandEventType.Sleeping:
                    return _imageSleeping;
                case BandEventType.Running:
                    return _imageRunning;
                case BandEventType.UserDailyActivity:
                    return _imageDailyActivity;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
