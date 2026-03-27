using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Project.UI.Common
{
    public class DeviceStatusButton : ButtonBase
    {
        #region fields
        protected bool _bkColor = false;
        protected DispatcherTimer timer = null;
        protected FormattedText formtxt;
        protected bool isMouseReallyOver;

        public static readonly DependencyProperty IsInterlockedProperty;
        public static readonly DependencyProperty IsActivedProperty;
        public static readonly DependencyProperty TextProperty;
        //public static readonly RoutedEvent ClickEvent;
        //public static readonly RoutedEvent PreviewClickEvent;
        #endregion

        #region propertise

        public Brush TickColor { get; set; }


        public bool IsInterlocked
        {
            set { SetValue(IsInterlockedProperty, value); }

            get { return (bool)GetValue(IsInterlockedProperty); }
        }

        public bool IsActived
        {
            set 
            { 
                SetValue(IsActivedProperty, value); 
            }

            get { return (bool)GetValue(IsActivedProperty); }
        }
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        //public event RoutedEventHandler PreviewClick
        //{
        //    add { AddHandler(PreviewClickEvent, value); }
        //    remove { RemoveHandler(PreviewClickEvent, value); }
        //}

        public string Text
        {
            set { SetValue(TextProperty, value == null ? " " : value); }

            get { return (string)GetValue(TextProperty); }
        }
        #endregion

        #region methods
        protected override Size MeasureOverride(Size constraint)
        {
            formtxt = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Foreground);


            Size sizeDesired = new Size(Math.Max(0, formtxt.Width) + 4, formtxt.Height + 4);

            sizeDesired.Width += Padding.Left + Padding.Right;
            sizeDesired.Height += Padding.Top + Padding.Bottom;

            sizeDesired.Width = Math.Min(sizeDesired.Width, constraint.Width);
            sizeDesired.Height = Math.Min(sizeDesired.Height, constraint.Height);

            return sizeDesired;
        }

        protected override void OnRender(DrawingContext dc)
        {
            // 배경설정.
            Brush brushBackground = Background;

            //if (isMouseReallyOver && IsMouseCaptured)
            //    brushBackground = SystemColors.ControlDarkBrush;

            //if (IsActived == true && IsMouseCaptured == false)
            //    brushBackground = Background;
            //else if (IsActived == false && IsMouseCaptured == false)
            //    brushBackground = SystemColors.ControlDarkBrush;

            if (_bkColor == true)
                brushBackground = Background;
            else
                brushBackground = SystemColors.ControlDarkBrush;


            // 펜의 두께 결정.
            Pen pen = new Pen(Foreground, IsMouseOver ? 2 : 1);


            // 둥근 모서리의 사각형을 그림.

            //dc.DrawRoundedRectangle(brushBackground, pen, new Rect(new Point(0, 0), RenderSize), 4, 4);
            dc.DrawRectangle(brushBackground, pen, new Rect(new Point(0, 0), RenderSize));

            // Interlock Rectangle
            dc.DrawRectangle(IsInterlocked ? Brushes.Red : Brushes.LimeGreen, new Pen(Brushes.Black, 2), new Rect(new Point(5, 5), new Size(10, 10)));


            formtxt.SetForegroundBrush(IsEnabled ? Foreground : SystemColors.ControlDarkBrush);


            Point ptText = new Point(2, 2);

            switch (HorizontalContentAlignment)
            {
                case HorizontalAlignment.Left:
                    ptText.X += Padding.Left;
                    break;

                case HorizontalAlignment.Right:

                    ptText.X += RenderSize.Width - formtxt.Width - Padding.Right;
                    break;

                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:

                    ptText.X += (RenderSize.Width - formtxt.Width - Padding.Left - Padding.Right) / 2;
                    break;
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    ptText.Y += Padding.Top;
                    break;

                case VerticalAlignment.Bottom:
                    ptText.Y += RenderSize.Height - formtxt.Height - Padding.Bottom;
                    break;

                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    ptText.Y += (RenderSize.Height - formtxt.Height - Padding.Top) / 2;

                    break;
            }

            dc.DrawText(formtxt, ptText);
        }
        private static void OnActiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            DeviceStatusButton d = o as DeviceStatusButton;

            if(d.IsActived == true)
            {
                if(d.timer == null)
                {
                    d.timer = new DispatcherTimer();
                    d.timer.Tick += d.TimerOnTick;
                    d.timer.Interval = TimeSpan.FromSeconds(0.5);
                    d.timer.Start();
                }                
            }
            else
            {
                if(d != null)
                {
                    d.timer.Stop();
                    d._bkColor = false;
                    d = null;
                }
            }
        }
        protected void TimerOnTick(object sender, EventArgs args)
        {
            _bkColor = _bkColor == true ? false : true;
            InvalidateVisual();
        }


        #endregion

        #region constructors
        public DeviceStatusButton()
        {

        }
        static DeviceStatusButton()
        {
            TextProperty = 
                DependencyProperty.Register("Text", typeof(string), 
                typeof(DeviceStatusButton), new FrameworkPropertyMetadata(" ", 
                FrameworkPropertyMetadataOptions.AffectsMeasure));

            IsInterlockedProperty =
                DependencyProperty.Register("IsInterlocked", typeof(bool),
                typeof(DeviceStatusButton), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsRender));

            IsActivedProperty =
                DependencyProperty.Register("IsActived", typeof(bool),
                typeof(DeviceStatusButton), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnActiveChanged)));

            //ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DeviceStatusButton));

            //PreviewClickEvent = EventManager.RegisterRoutedEvent("PreviewClick", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(DeviceStatusButton));
        }
        #endregion
    }
}
