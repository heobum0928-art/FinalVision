using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using FinalVisionProject.Setting;
using FinalVisionProject.Define;
using System.Windows.Threading;
using FinalVisionProject.Utility;
using FinalVisionProject.Sequence;
using FinalVisionProject.UI;
using FinalVisionProject.Device;
using FinalVisionProject.Login;
using System.Windows.Controls;
using FinalVisionProject.Properties;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using System.Diagnostics;
using Project.BaseLib.Utils;

namespace FinalVisionProject {    

    public enum EPageType {
        Ready,
        Recipe,
        Log,
        Setting,
        //DeepLearning,
        //OCRManager,
        Camera,
        Light,
        Connect,
        Login,
        ProcessMonitor,
    }
    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window {

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        private Point startPos;
        //private PageType CurrentPage = PageType.Ready;

        private SystemHandler mSystemHandler;
        

        private bool _IsEditable;
        public bool IsEditable {
            get {
                return _IsEditable;
            }

            set {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                    inspectionList.IsEditable = value;
                    menuBar.IsEditable = value;
                    mainView.SetShotViewsEditable(value);   //260330 hbk — Shot 탭 ROI 편집도 Edit 모드 연동
                    _IsEditable = value;
                }));
            }
        }

        //UI
        private Window mModalWindow;
        private ProcessMonitorWindow mProcMonitorWindow;
        private LogViewWindow mLogViewWindow;

        private DispatcherTimer mTimer = new DispatcherTimer();

        

        public MainWindow() {
            //initialize
            mSystemHandler = SystemHandler.Handle;
            mSystemHandler.Initialize();
            
            //update ui
            InitializeComponent();
            //ComboLanguage.ItemsSource = Enum.GetValues(typeof(ELanguageType)).Cast<ELanguageType>();
            
            //register
            mainView.canvas_main.OnSelectionItemChanged += SelectedDrawingItemChanged;
            mSystemHandler.Sequences.OnRecipeChanged += this.OnLoadRecipe;

            mSystemHandler.Login.OnLoginStateChanged += this.OnLoginChanged;

            for (int i = 0; i < mSystemHandler.Sequences.Count; i++) {
                mSystemHandler.Sequences[i].OnStart += OnSequenceStart;
                mSystemHandler.Sequences[i].OnStop += OnSequenceStop;
                mSystemHandler.Sequences[i].OnError += OnSequenceError;
                mSystemHandler.Sequences[i].OnFinish += OnSequenceFinish;
            }
            mTimer.Dispatcher.Thread.Priority = System.Threading.ThreadPriority.AboveNormal;
            mTimer.Interval = TimeSpan.FromMilliseconds(constantInterval);
            mTimer.Tick += TimerTick;
        }
        const int constantInterval = 100;
        private int timerCallCount = 0;
        const int statusUpdateInterval = 1000;
        private void TimerTick(object sender, EventArgs args) {

            var now = DateTime.Now;
            //var nowMilliseconds = (int)now.TimeOfDay.TotalMilliseconds;
            //var timerInterval = constantInterval - nowMilliseconds % constantInterval + 5;//5: sometimes the tick comes few millisecs early
            //mTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);

            if (this.IsVisible) {
                menuBar.UpdateState();
                if ((timerCallCount * constantInterval) >= statusUpdateInterval) {
                    statusBar.Model.UpdateResourceInfo();
                    timerCallCount = 0;
                }
                if(mProcMonitorWindow != null) {
                    if (mProcMonitorWindow.IsVisible) {
                        mProcMonitorWindow.Model.UpdateState();
                    }
                }
            }
            else {
                mTimer.Stop();
            }
            timerCallCount++;
        }

        //언어 설정이 변경될 때 호출합니다.
        private void LanguageChanged(object sender, string e) {
            
        }

        public void SelectedDrawingItemChanged(object sender, SelectionChangedCallbackArg arg) {
            //mParentWindow.SelectParam()
        }

        private void OnSequenceStart(SequenceContext context) {
        }

        private void OnSequenceStop(SequenceContext context) {
            Logging.PrintLog((int)ELogType.Result, context.ToString());
            statusBar.Model.SetText(string.Format("{0} Stop.({1},{2}ms)", context.Source.Name, context.ResultString, context.Timer.ElapsedMilliseconds.ToString()));
        }

        private void OnSequenceError(SequenceContext context) {
            mainView.DisplaySequenceContext(context);
            Logging.PrintLog((int)ELogType.Result, context.ToString());
            statusBar.Model.SetText(string.Format("{0} Error.({1},{2}ms)", context.Source.Name, context.ResultString, context.Timer.ElapsedMilliseconds.ToString()));
        }

        private void OnSequenceFinish(SequenceContext context) {
            //context.InputImage
            mainView.DisplaySequenceContext(context);

            Logging.PrintLog((int)ELogType.Result, context.ToString());

            statusBar.Model.SetText(string.Format("{0} Finished.({1},{2}ms)", context.Source.Name, context.ResultString, context.Timer.ElapsedMilliseconds.ToString()));
        }

        private void Title_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                if (e.ClickCount >= 2) {
                    this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
                }
                else {
                    startPos = e.GetPosition(null);
                }
            }
            else if (e.ChangedButton == MouseButton.Right) {
                var pos = PointToScreen(e.GetPosition(this));
                IntPtr hWnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                IntPtr hMenu = GetSystemMenu(hWnd, false);
                int cmd = TrackPopupMenu(hMenu, 0x100, (int)pos.X, (int)pos.Y, 0, hWnd, IntPtr.Zero);
                if (cmd > 0) SendMessage(hWnd, 0x112, (IntPtr)cmd, IntPtr.Zero);
            }
        }

        private void Title_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (this.WindowState == WindowState.Maximized && Math.Abs(startPos.Y - e.GetPosition(null).Y) > 2) {
                    var point = PointToScreen(e.GetPosition(null));

                    this.WindowState = WindowState.Normal;

                    this.Left = point.X - this.ActualWidth / 2;
                    this.Top = point.Y - label_title.ActualHeight / 2;
                }
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        
        public void StartSequence(ESequence seqID, EAction actID= EAction.Unknown) {
            mSystemHandler.Sequences.Start(seqID, actID);
        }

        public void StartSequence(string seqName, int actNo = 0) {

        }

        public void OnLoginChanged(object sender, LoginEventArgs args) {
            this.IsEditable = args.IsLogined;
            menuBar.UpdateLoginID(mSystemHandler.Login.LoginID);
        }

        public void OnLoadRecipe(object sender, RecipeChangedEventArgs args) {
            //args.RecipeName;
            inspectionList.OnLoadRecipe(args.RecipeName);
        }

        public void SaveRecipe(string name=null) {
            if(mSystemHandler.Sequences.StateAll == EContextState.Running) {
                CustomMessageBox.Show("Error", SystemHandler.Handle.Localize["System Is Running"], MessageBoxImage.Error);
                return;
            }
            if(!mSystemHandler.Sequences.SaveRecipe(name)) {   //260403 hbk — RecipeSavePath 루트 기준 저장
                CustomMessageBox.Show("Error", SystemHandler.Handle.Localize["fail to save recipe"], MessageBoxImage.Error);
                return;
            }
            CustomMessageBox.Show(SystemHandler.Handle.Localize["Save Recipe Success"], string.Format("Recipe : {0} Saved!", name), MessageBoxImage.Information, false);
        }
        
        //main view 위에 표시한다. 
        public void PopupView(EPageType page) {
            if(mModalWindow != null) {
                //closing window or else
            }
            switch (page) {
                case EPageType.Login:
                    mModalWindow = new LoginWindow();
                    mModalWindow.Owner = this;
                    if(mModalWindow.ShowDialog() == true) {
                        
                    }
                    break;
                case EPageType.Camera:
                    if(mSystemHandler.Sequences.IsIdle == false) {
                        CustomMessageBox.Show(SystemHandler.Handle.Localize["Sequence is Running"], SystemHandler.Handle.Localize["window cannot be displayed.\nWait until the sequence is completed."], MessageBoxImage.Error);
                        return;
                    }
                    string selectedDevName = null;
                    if(inspectionList.SelectedParam != null) {
                        ICameraParam param = inspectionList.SelectedParam as ICameraParam;
                        if(param != null) {
                            selectedDevName = param.DeviceName;
                        }
                    }
                    mModalWindow = new DeviceSelector(selectedDevName);
                    mModalWindow.Owner = this;
                    if(mModalWindow.ShowDialog() == true) {
                        DeviceSelector devWindow = mModalWindow as DeviceSelector;
                        if (mainView.DrawScale != devWindow.SelectedDisplayConfig.DrawScale) {
                            mainView.DrawScale = devWindow.SelectedDisplayConfig.DrawScale;
                        }
                        
                    }
                    break;
                case EPageType.Connect:
                    mModalWindow = new TcpServerWindow();
                    mModalWindow.Owner = this;
                    if(mModalWindow.ShowDialog() == true){

                    }
                    break;
                case EPageType.Light:
                    mModalWindow = new LightHandlerWindow();
                    mModalWindow.Owner = this;
                    if(mModalWindow.ShowDialog() == true) {

                    }
                    break;
                case EPageType.Log:
                    /*
                    if(mLogViewWindow != null) {
                        if (mLogViewWindow.IsLoaded) {
                            mLogViewWindow.Show();
                            return;
                        }
                    }
                    mLogViewWindow = new LogViewWindow();
                    mLogViewWindow.Owner = this;
                    mLogViewWindow.Show();
                    */                
                    break;
                case EPageType.Ready:
                    break;
                case EPageType.Recipe:
                    mModalWindow = new OpenRecipeWindow();
                    mModalWindow.Owner = this;
                    if (mModalWindow.ShowDialog() == true) {
                        string selectedName = (mModalWindow as OpenRecipeWindow).SelectedRecipeName;
                        if (!mSystemHandler.LoadRecipe(selectedName)) {   //260403 hbk — RecipeSavePath 루트 기준 로드
                            CustomMessageBox.Show("Error", SystemHandler.Handle.Localize["fail to load recipe"], MessageBoxImage.Error);
                        }
                    }
                    break;
                case EPageType.Setting:
                    mModalWindow = new SettingWindow();
                    mModalWindow.Owner = this;
                    mModalWindow.ShowDialog();
                    break;

                //case EPageType.DeepLearning:
                //    mModalWindow = new DeepLearningWindow();
                //    mModalWindow.Owner = this;
                //    mModalWindow.ShowDialog();
                //    break;


                //case EPageType.OCRManager:
                //    mModalWindow = new OCRWindow();
                //    mModalWindow.Owner = this;
                //    mModalWindow.ShowDialog();
                //    break;

                case EPageType.ProcessMonitor:
                    if(mProcMonitorWindow != null) {
                        if (mProcMonitorWindow.IsLoaded) {
                            mProcMonitorWindow.Show();
                            return;
                        }
                    }
                    mProcMonitorWindow = new ProcessMonitorWindow();
                    mProcMonitorWindow.Owner = this;
                    mProcMonitorWindow.Show();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            CustomMessageBox.Parent = this;
            mTimer.Start();

            //register custom ui
            RegisterCustomUI();

            //load recipe

            if (mSystemHandler.Setting.CurrentRecipeName != null)
            {
                mSystemHandler.LoadRecipe(mSystemHandler.Setting.CurrentRecipeName);   //260403 hbk — RecipeSavePath 루트 기준 로드
            }
            
            IsEditable = false;







            //ByteImage test_image = new ByteImage("D:\\2.bmp");


            //var bin_image = test_image.Binarization(1, 130, BinTypes.InSide);

            //bin_image.Save("D:\\bin_image.bmp");

            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();
            //bin_image = bin_image.DilationHorizontal();


            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();
            //bin_image = bin_image.ErosionHorizontal();



            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();
            //bin_image = bin_image.DilationVertical();


            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();
            //bin_image = bin_image.ErosionVertical();



            //bin_image.Save("D:\\Erosion.bmp");



        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            MessageBoxResult result = CustomMessageBox.ShowConfirmation("Confirm", SystemHandler.Handle.Localize["Are you sure you want to exit program?"], MessageBoxButton.YesNo);
            if(result == MessageBoxResult.No) {
                e.Cancel = true;
                return;
            }

            //register
            mTimer.Stop(); //260402 hbk DispatcherTimer 정리 (메모리 누수 방지)
            mTimer.Tick -= TimerTick; //260402 hbk

            mainView.Cleanup(); //260402 hbk MainView OnFinish 이벤트 해제
            mainView.shotView_1.Cleanup(); //260402 hbk ShotTabView OnFinish 이벤트 해제
            mainView.shotView_2.Cleanup(); //260402 hbk
            mainView.shotView_3.Cleanup(); //260402 hbk
            mainView.shotView_4.Cleanup(); //260402 hbk
            mainView.shotView_5.Cleanup(); //260402 hbk

            mainView.canvas_main.OnSelectionItemChanged -= SelectedDrawingItemChanged; //260402 hbk 이벤트 해제 누락 수정
            mSystemHandler.Sequences.OnRecipeChanged -= this.OnLoadRecipe; //260402 hbk 이벤트 해제 누락 수정
            mSystemHandler.Login.OnLoginStateChanged -= this.OnLoginChanged; //260402 hbk 이벤트 해제 누락 수정

            for (int i = 0; i < mSystemHandler.Sequences.Count; i++) {
                mSystemHandler.Sequences[i].OnStart -= OnSequenceStart;
                mSystemHandler.Sequences[i].OnStop -= OnSequenceStop;
                mSystemHandler.Sequences[i].OnError -= OnSequenceError;
                mSystemHandler.Sequences[i].OnFinish -= OnSequenceFinish;
            }
            mSystemHandler.Release();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        public void AddCustomMainView(string name, UserControl control) {
            mainView.AddCustomControl(name, control);
        }
    }
}
