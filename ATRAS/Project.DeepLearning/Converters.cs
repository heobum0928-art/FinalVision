using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Project.DeepLearning.UI
{
    public class ClassTextColor : IValueConverter
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;

            return ClassColors.GetTextBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region constructors

        #endregion

    }
    
    public class ClassTextBackgroundColor : IValueConverter
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;

            return ClassColors.GetTextBackgroundBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region constructors

        #endregion

    }

    public class ConfidenceBarWidth : IValueConverter
    {
        #region methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size = 200.0;

            double confidence = (double)value / 100.0;

            int width = (int)(size * confidence );


            return width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class ConfidenceText : IValueConverter
    {
        #region methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double confidence = (double)value;

            return string.Format("{0:0.0}%", confidence);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
