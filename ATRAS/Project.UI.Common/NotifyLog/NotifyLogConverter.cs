using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class NotifyLogConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<LogItems> items = (ObservableCollection<LogItems>)value;

            foreach (var item in items)
            {
                var logItem = (item as LogItems);
                if (logItem != null)
                {
                    if (logItem.LogLevel == LogLevel.Error)
                        return Brushes.Red;

                }
            }
            return Brushes.DimGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
