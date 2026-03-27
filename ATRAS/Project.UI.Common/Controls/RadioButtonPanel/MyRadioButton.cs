using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project.UI.Common
{
    public class MyRadioButton : Control
    {
        #region fields
        public static readonly DependencyProperty TextProperty;

        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty IsHighlightedProperty;
        public static readonly DependencyProperty BrushProperty;

        public static readonly DependencyProperty LengthProperty;

        FormattedText formtxt;
        static Size sizeCell = new Size(20, 20);
        Border bord = null;

        protected int _ButtonIndex = 0;
        #endregion

        #region propertise
        public int ButtonIndex
        {
            get { return _ButtonIndex; }
        }
        public bool IsSelected
        {
            set { SetValue(IsSelectedProperty, value); }
            get { return (bool)GetValue(IsSelectedProperty); }
        }
        public bool IsHighlighted
        {
            set { SetValue(IsHighlightedProperty, value); }
            get { return (bool)GetValue(IsHighlightedProperty); }
        }

        public Brush Brush
        {
            set { SetValue(BrushProperty, value); }
            get { return (Brush)GetValue(BrushProperty); }
        }

        public int Length
        {
            set { SetValue(LengthProperty, value); }
            get { return (int)GetValue(LengthProperty); }
        }
        public string Text
        {
            set { SetValue(TextProperty, value == null ? " " : value); }
            get { return (string)GetValue(TextProperty); }
        }
        #endregion

        #region medthods
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index > 0)
                throw new ArgumentOutOfRangeException("index");

            return bord;
        }

        //protected override Size MeasureOverride(Size sizeAvailable)
        //{
        //    formtxt = new FormattedText(
        //        Text, CultureInfo.CurrentCulture, FlowDirection,
        //        new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
        //        FontSize, Foreground);

        //    double width = Math.Max(formtxt.Width + 4, Length);
        //    double height = formtxt.Height + 4 + Length;
        //    //double height = Math.Max(formtxt.Height + 4, Length);

        //    Size sizseDesired = new Size(width, height);

        //    sizeCell = new Size(Length, Length);

        //    sizseDesired.Width += Padding.Left + Padding.Right;
        //    sizseDesired.Height += Padding.Top + Padding.Bottom;

        //    return sizseDesired;
        //}
        //protected override void OnRender(DrawingContext dc)
        //{
        //    Brush brushBackground = SystemColors.ControlBrush;

        //    Point ptText = new Point(0, 0);

        //    ptText.X += (RenderSize.Width - formtxt.Width -
        //            Padding.Left - Padding.Right) / 2;

        //    ptText.Y += Padding.Top;

        //    dc.DrawText(formtxt, ptText);

        //    double x = (RenderSize.Width - Length -
        //        Padding.Left - Padding.Right) / 2;

        //    Rect rect = new Rect(new Point(x, ptText.Y + formtxt.Height + 2), sizeCell);
        //    rect.Inflate(-1, -1);
        //    Pen pen = new Pen(SystemColors.HighlightBrush, 1);

        //    if (IsHighlighted)
        //        dc.DrawRectangle(SystemColors.ControlDarkBrush, pen, rect);
        //    else if (IsSelected)
        //        dc.DrawRectangle(SystemColors.ControlLightBrush, pen, rect);
        //    else
        //        dc.DrawRectangle(Brushes.Transparent, null, rect);

        //    rect = rect = new Rect(new Point(x, ptText.Y + formtxt.Height + 2), sizeCell);
        //    rect.Inflate(-4, -4);
        //    pen = new Pen(SystemColors.ControlTextBrush, 1);
        //    dc.DrawRectangle(Brush, pen, rect);






        //}

        protected override Size MeasureOverride(Size sizeAvailable)
        {
            formtxt = new FormattedText(
                Text, CultureInfo.CurrentCulture, FlowDirection,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize, Foreground);

            double width = Math.Max(formtxt.Width + 4, Length);
            //double height = formtxt.Height + 4 + Length;
            double height = Math.Max(formtxt.Height + 4, Length);

            Size sizseDesired = new Size(width, height);

            sizeCell = new Size(Length, Length);

            sizseDesired.Width += Padding.Left + Padding.Right;
            sizseDesired.Height += Padding.Top + Padding.Bottom;

            return sizseDesired;
        }
        protected override void OnRender(DrawingContext dc)
        {
            Brush brushBackground = SystemColors.ControlBrush;

            double x = (RenderSize.Width - Length -
                Padding.Left - Padding.Right) / 2;

            double y = (RenderSize.Height - Length - Padding.Top - Padding.Bottom) / 2;

            Rect rect = new Rect(new Point(x, y), sizeCell);
            rect.Inflate(-1, -1);
            Pen pen = new Pen(SystemColors.HighlightBrush, 1);

            if (IsHighlighted)
                dc.DrawRectangle(SystemColors.ControlDarkBrush, pen, rect);
            else if (IsSelected)
                dc.DrawRectangle(SystemColors.ControlLightBrush, pen, rect);
            else
                dc.DrawRectangle(Brushes.Transparent, null, rect);

            rect = rect = new Rect(new Point(x, y), sizeCell);
            rect.Inflate(-4, -4);
            pen = new Pen(SystemColors.ControlTextBrush, 1);
            dc.DrawRectangle(Brush, pen, rect);


            Point ptText = new Point(0, 0);

            ptText.X += (RenderSize.Width - formtxt.Width -
                    Padding.Left - Padding.Right) / 2;

            ptText.Y += (RenderSize.Height - formtxt.Height -
                    Padding.Top - Padding.Bottom) / 2;

            dc.DrawText(formtxt, ptText);

        }
        private static void OnBrushChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnLengthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region constructor
        public MyRadioButton(int index)
        {
            bord = new Border();

            // AddVisualChild is necessary for event routing!
            AddVisualChild(bord);
            AddLogicalChild(bord);

            _ButtonIndex = index;
        }
        protected MyRadioButton()
        {
            //bord = new Border();

            //// AddVisualChild is necessary for event routing!
            //AddVisualChild(bord);
            //AddLogicalChild(bord);
        }

        static MyRadioButton()
        {
            TextProperty =
                DependencyProperty.Register("Text", typeof(string),
                                            typeof(MyRadioButton),
                    new FrameworkPropertyMetadata(" ",
                            FrameworkPropertyMetadataOptions.AffectsMeasure));

            IsSelectedProperty =
                DependencyProperty.Register("IsSelected", typeof(bool),
                        typeof(MyRadioButton), new FrameworkPropertyMetadata(false,
                                FrameworkPropertyMetadataOptions.AffectsRender));

            IsHighlightedProperty =
                DependencyProperty.Register("IsHighlighted", typeof(bool),
                        typeof(MyRadioButton), new FrameworkPropertyMetadata(false,
                                FrameworkPropertyMetadataOptions.AffectsRender));

            BrushProperty =
                DependencyProperty.Register("Brush", typeof(Brush),
                        typeof(MyRadioButton), new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsRender,
                                new PropertyChangedCallback(OnBrushChanged)));

            LengthProperty =
                DependencyProperty.Register("Length", typeof(int),
                        typeof(MyRadioButton), new FrameworkPropertyMetadata(20,
                        FrameworkPropertyMetadataOptions.AffectsMeasure,
                                new PropertyChangedCallback(OnLengthChanged)));
        }
        #endregion
    }
}
