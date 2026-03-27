using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Project.UI.Common
{
    public class BlinkButton : Button
    {
        #region fields
        public static readonly DependencyProperty IsActivedProperty;

        protected DispatcherTimer timer = null;

        protected bool _bkColor = false;

        //protected bool _IsActived = false;

        protected Brush _tempColor = null;
        #endregion

        #region propertise
        public Brush BlinkColor { set; private get; }

        public bool IsActived
        {

            set
            {



                SetValue(IsActivedProperty, value);


            }

            get { return (bool)GetValue(IsActivedProperty); }
            //set
            //{
            //    _IsActived = value;

            //    if(_IsActived == true)
            //    {
            //        if(timer == null)
            //        {
            //            _tempColor = Background;
            //            timer = new DispatcherTimer();
            //            timer.Tick += TimerOnTick;
            //            timer.Interval = TimeSpan.FromSeconds(1);
            //            timer.Start();
            //        }
            //    }
            //    else
            //    {
            //        if(timer != null)
            //        {
            //            Background = _tempColor;
            //            timer.Stop();
            //            _bkColor = false;
            //            timer = null;
            //        }
            //    }
            //}

            //get 
            //{

            //    return _IsActived;
            //}
        }
        #endregion

        #region methods
        private static void OnActiveChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BlinkButton d = o as BlinkButton;

            if (d.IsActived == true)
            {
                if (d.timer == null)
                {
                    d._tempColor = d.Background;
                    d.timer = new DispatcherTimer();
                    d.timer.Tick += d.TimerOnTick;
                    d.timer.Interval = TimeSpan.FromSeconds(1);
                    d.timer.Start();
                }
            }
            else
            {
                if (d.timer != null)
                {
                    d.Background = d._tempColor;
                    d.timer.Stop();
                    d._bkColor = false;
                    d.timer = null;
                }
            }
        }

        protected void TimerOnTick(object sender, EventArgs args)
        {
            if(_bkColor == false)
            {
                _bkColor = true;
                Background = BlinkColor;
            }
            else
            {
                _bkColor = false;

                Background = _tempColor;
            }
        }
        #endregion

        #region constructors
        public BlinkButton()
        {
        }

        static BlinkButton()
        {
            IsActivedProperty =
                DependencyProperty.Register("IsActived", typeof(bool),
                typeof(BlinkButton), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnActiveChanged)));

        }

        #endregion


    }
}
