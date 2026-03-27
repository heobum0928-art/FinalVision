using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class WaferExistFlagColor : IValueConverter
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SlotStates ss = (SlotStates)value;

            if(ss == SlotStates.Empty)
            {
                return Brushes.Transparent;
            }
            else if(ss == SlotStates.ProcessDone)
            {
                return Brushes.Blue;
            }
            else if(ss == SlotStates.Correct)
            {
                return Brushes.White;
            }
            else if(ss == SlotStates.CrossSlotted || ss == SlotStates.DoubleSlotted)
            {
                return Brushes.OrangeRed;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region constructors

        #endregion

    }

    public class BoolGreenRedColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;

            if (bb == true)
                return Brushes.LightGreen;

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolRedGreenColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bb = (bool)value;

            if (bb == true)
                return Brushes.LightGreen;

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IOStateColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IOStates ios = (IOStates)value;

            if (ios == IOStates.On)
                return Brushes.LightGreen;
            else if (ios == IOStates.Off)
                return Brushes.Red;
            
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
