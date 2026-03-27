using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Project.UI.Common
{
    /// <summary>
    /// LoaddingWindow.xaml에 대한 상호 작용 논리
    /// </summary>

    public partial class LoadingWindow : Window, INotifyPropertyChanged
    {
        #region fields
        public event PropertyChangedEventHandler PropertyChanged;

        protected DispatcherTimer _timer = null;

        protected int close_sec = 60;

        protected int current_sec = 0;

        protected string _LoadingMessage = "Training...";
        #endregion

        #region propertise
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
        #endregion

        #region methods
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string LoadingMessage
        {
            get
            {
                return _LoadingMessage;
            }

            set
            {
                _LoadingMessage = value;
            }
        }

        #endregion

        #region constructors

        #endregion
        protected LoadingWindow()
        {
            InitializeComponent();

            DataContext = this;

            StartTimer();







        }

        public LoadingWindow(int close_sec)
        {
            InitializeComponent();

            DataContext = this;

            this.close_sec = close_sec;

            _LoadingMessage = string.Empty;


            //{
            //    mTimer = new DispatcherTimer();
            //    mTimer.Interval = TimeSpan.FromSeconds(1000);
            //    mTimer.Tick += OnTimerTick;
            //    mTimer.Start();
            //}


            StartTimer();


        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            current_sec++;
            LoadingMessage = string.Format("Training({0})...", current_sec);

            if (close_sec <= current_sec)
            {
                _timer.Stop();
                _timer.Tick -= OnTimerTick;

                //DialogResult = false;
                Result = MessageBoxResult.Cancel;


                this.Close();
            }

        }

        private void StartTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)  // 1초마다 Tick 발생
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            int k = 0;
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            int k = 0;
        }
    }
}
