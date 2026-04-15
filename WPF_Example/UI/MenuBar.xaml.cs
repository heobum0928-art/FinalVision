
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation; //260413 hbk — Storyboard
using System.Windows.Media.Imaging;
using FinalVisionProject.Network; //260413 hbk — AlarmEventArgs
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FinalVisionProject.UI {
    /// <summary>
    /// MenuBar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MenuBar : UserControl {
        MainWindow mParentWindow;

        private SolidColorBrush _aliveBrush;              //260413 hbk — alive_Ellipse Fill SolidColorBrush 캐시
        private Storyboard _flashStoryboard;              //260413 hbk — AliveFlashStoryboard 리소스 캐시
        private volatile bool _aliveTimeoutLatched;       //260413 hbk — 빨강 래치 (OnConnected 수신 시 clear)
        private volatile bool _aliveActive;               //260415 hbk — PLC ALIVE 수신 중 여부 (수신 시 set, 타임아웃/재접속 시 clear)
        private static readonly Color AliveGray      = Color.FromArgb(0xFF, 0x9E, 0x9E, 0x9E);  //260413 hbk
        private static readonly Color AliveBaseGreen = Color.FromArgb(0xFF, 0x7E, 0xE0, 0x8B);  //260413 hbk
        private static readonly Color AliveRed       = Color.FromArgb(0xFF, 0xE5, 0x39, 0x35);  //260413 hbk

        public MenuBar() {
            InitializeComponent();

            label_title.Text = SystemHandler.ProjectName;
            label_Version.Text = string.Format("Platform : {0}", SystemHandler.Handle.Recipes.GetVersion());
        }

        private bool _IsEditable = false;
        public bool IsEditable {
            get {
                return _IsEditable;
            }

            set {
                ButtonSetting.IsEnabled = value;
                Button_Camera.IsEnabled = value;
                Button_Light.IsEnabled = value;
                Button_Connect.IsEnabled = value;
                
                _IsEditable = value;
            }
        }

        private void MenuBar_Loaded(object sender, RoutedEventArgs e) {
            mParentWindow = (MainWindow)Window.GetWindow(this);
            UpdateLoginID(SystemHandler.Handle.Login.LoginID);

            //260413 hbk — Phase 16 ALIVE 인디케이터 초기화
            _aliveBrush = (SolidColorBrush)alive_Ellipse.Fill;
            _flashStoryboard = (Storyboard)this.Resources["AliveFlashStoryboard"];

            SystemHandler.Handle.AliveHeartbeatReceived += OnAliveHeartbeat;  //260413 hbk
            SystemHandler.Handle.AliveTimeout += OnAliveTimeoutEvent;          //260413 hbk
            if (SystemHandler.Handle.Server != null) {                          //260413 hbk — NRE 방어 (Pitfall #4)
                SystemHandler.Handle.Server.OnAlarm += OnServerAlarm;           //260413 hbk
            }
            this.Unloaded += MenuBar_Unloaded;                                  //260413 hbk — 해제
        }

        public void UpdateState() {
            this.label_DateTime.Text = DateTime.Now.ToString();
            label_status.Content = SystemHandler.Handle.Sequences.StateAll;
            label_seqName.Content = SystemHandler.Handle.Sequences.StateSequenceName;

            //260415 hbk — Phase 16 ALIVE 3-state 폴링 (PLC ALIVE 수신 중일 때만 녹색)
            if (_aliveBrush == null) return;           //260413 hbk — Loaded 이전 가드
            if (_aliveTimeoutLatched) {                //260413 hbk — 빨강 래치 유지
                _aliveBrush.Color = AliveRed;          //260413 hbk
                return;                                //260413 hbk
            }
            //260415 hbk — TCP 끊겼으면 ALIVE 활성도 즉시 해제 (끊긴 직후 회색 복귀)
            bool connected = SystemHandler.Handle.Server?.IsConnected() ?? false;
            if (!connected) _aliveActive = false;
            _aliveBrush.Color = _aliveActive ? AliveBaseGreen : AliveGray;          //260415 hbk — ALIVE 패킷 수신 중일 때만 녹색
        }

        public void UpdateLoginID(string id) {
            this.Dispatcher.Invoke(() => { 
                textBlock_login.Text = id.ToUpper();
            });
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e) {
            //mParentWindow.DisplayView(PageType.Setting);
            mParentWindow.PopupView(EPageType.Setting);
        }

        //private void ButtonDeepLearning_Click(object sender, RoutedEventArgs e)
        //{
        //    //mParentWindow.DisplayView(PageType.Setting);
        //    mParentWindow.PopupView(EPageType.DeepLearning);
        //}

        //private void ButtonOCR_Click(object sender, RoutedEventArgs e)
        //{
        //    //mParentWindow.DisplayView(PageType.Setting);
        //    mParentWindow.PopupView(EPageType.OCRManager);
        //}

        private void Button_Log_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Log);
        }

        private void Button_Recipe_Click(object sender, RoutedEventArgs e) {
            //mParentWindow.DisplayView(PageType.Recipe);
            mParentWindow.PopupView(EPageType.Recipe);
        }

        private void Button_Camera_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Camera);
        }

        private void Button_Light_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Light);
        }

        private void Button_Connect_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Connect);
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.Login);
        }

        private void Button_CI_Click(object sender, RoutedEventArgs e) {
            
        }

        private void Label_status_Click(object sender, RoutedEventArgs e) {
            mParentWindow.PopupView(EPageType.ProcessMonitor);
        }

        //260415 hbk — Phase 16 ALIVE 응답 수신 → 녹색 활성 + flash 1회
        private void OnAliveHeartbeat() {
            Dispatcher.BeginInvoke(new Action(() => {
                _aliveActive = true;               //260415 hbk — PLC ALIVE 수신 → 녹색
                if (_aliveTimeoutLatched) return;  //260413 hbk — 빨강 중에는 flash 금지 (Pitfall #2)
                if (_flashStoryboard == null) return;
                _flashStoryboard.Begin(this, true);  //260413 hbk — isControllable=true, 재진입 안전
            }));
        }

        //260415 hbk — Phase 16 ALIVE 타임아웃 → 빨강 래치 + 녹색 해제
        private void OnAliveTimeoutEvent() {
            Dispatcher.BeginInvoke(new Action(() => {
                _aliveActive = false;                            //260415 hbk
                _aliveTimeoutLatched = true;                     //260413 hbk
                if (_flashStoryboard != null) _flashStoryboard.Stop(this);  //260413 hbk
                if (_aliveBrush != null) _aliveBrush.Color = AliveRed;      //260413 hbk
            }));
        }

        //260415 hbk — Phase 16 Client 재접속 감지 → 빨강 래치 해제 (다음 ALIVE 패킷 전까지 회색)
        private void OnServerAlarm(object sender, AlarmEventArgs e) {
            if (e.AlarmType != AlarmEventArgs.AlarmEventType.OnConnected) return;  //260413 hbk
            Dispatcher.BeginInvoke(new Action(() => {
                _aliveTimeoutLatched = false;  //260413 hbk
                _aliveActive = false;          //260415 hbk — 재접속 직후엔 회색, 첫 ALIVE 수신 시 녹색 전환
            }));
        }

        //260413 hbk — Phase 16 Unload 시 이벤트 구독 해제 (메모리 누수 방지)
        private void MenuBar_Unloaded(object sender, RoutedEventArgs e) {
            SystemHandler.Handle.AliveHeartbeatReceived -= OnAliveHeartbeat;  //260413 hbk
            SystemHandler.Handle.AliveTimeout -= OnAliveTimeoutEvent;          //260413 hbk
            if (SystemHandler.Handle.Server != null) {
                SystemHandler.Handle.Server.OnAlarm -= OnServerAlarm;          //260413 hbk
            }
        }
    }
}
