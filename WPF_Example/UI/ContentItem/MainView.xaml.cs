
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Sequence;
using FinalVisionProject.Setting;
using FinalVisionProject.Utility;

namespace FinalVisionProject.UI {

    public enum MainViewMode {
        All,
        Original,
        Result,
    }

    public class MainViewModel : INotifyPropertyChanged {
        private string _SelectedSeqName;
        public string SelectedSeqName {
            get {
                return _SelectedSeqName;
            }
            set {
                _SelectedSeqName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSeqName"));
            }
        }

        private int _SelectedSeqIndex;
        public int SelectedSeqIndex {
            get {
                return _SelectedSeqIndex;
            }

            set {
                _SelectedSeqIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedSeqIndex"));
            }
        }

        private string _SelectedViewMode;
        public string SelectedViewMode {
            get {
                return _SelectedViewMode;
            }
            set {
                _SelectedViewMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedViewMode"));
            }
        }

        private int _SelectedViewIndex;
        public int SelectedViewIndex {
            get {
                return _SelectedViewIndex;
            }

            set {
                _SelectedViewIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedViewIndex"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// MainView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : UserControl {
        private const string COMBOBOX_SEQUENCE_ALL = "All";
        MainWindow mParentWindow;
        DeviceHandler pDev;
        SequenceHandler pSeq;
        LightHandler pLight;
        
        private MemoryStream BackgroundImageStream = null;
        private object mDrawInterlock = new object();
        private Dictionary<string, SequenceContext> ContextList;
        private System.Windows.Point? lastMousePositionOnTarget;
        private System.Windows.Point? lastCenterPositionOnTarget;

        public int BackgroundWidth { get; private set; } = DeviceHandler.INSPECTION_CAMERA_WIDTH;    //260326 hbk
        public int BackgroundHeight { get; private set; } = DeviceHandler.INSPECTION_CAMERA_HEIGHT;  //260326 hbk

        private ScaleTransform ScaleTransform = new ScaleTransform();

        private Task GrabTask = null;
       
        private bool dragStarted = false;

        private MainViewModel Model;

        // Shot 뷰어 관련 필드   //260326 hbk
        private static readonly string[] SHOT_ACTION_NAMES = {   //260326 hbk
            SequenceHandler.ACT_BOLT_ONE,    //260326 hbk // 인덱스 0
            SequenceHandler.ACT_BOLT_TWO,    //260326 hbk // 인덱스 1
            SequenceHandler.ACT_BOLT_THREE,  //260326 hbk // 인덱스 2
            SequenceHandler.ACT_ASSY_ONE,    //260326 hbk // 인덱스 3
            SequenceHandler.ACT_ASSY_TWO,    //260326 hbk // 인덱스 4
        };

        private static readonly string[] SHOT_DISPLAY_NAMES = {   //260326 hbk
            "Shot 1 (Bolt One)",       //260326 hbk
            "Shot 2 (Bolt Two)",       //260326 hbk
            "Shot 3 (Bolt Three)",     //260326 hbk
            "Shot 4 (Assy Rail One)",  //260326 hbk
            "Shot 5 (Assy Rail Two)",  //260326 hbk
        };

        private bool _suppressShotComboEvent = false;   //260326 hbk // 코드에서 SelectedIndex 변경 시 이벤트 억제

        private List<IMainView> CustomViewList = new List<IMainView>();

        public bool IsEditable {
            get { return canvas_main.IsEditable; }
            set { canvas_main.IsEditable = value; }    //260327 hbk 그리기 — IsEditable 활성화 (ROI 마우스 편집)
        }

        public MainView() {
            InitializeComponent();

            canvas_main.RenderTransform = ScaleTransform;
            canvas_main._ScaleTransform = ScaleTransform;
            //image_foreground.RenderTransform = ScaleTransform;
            canvas_main.ParentScrollViewer = scrollViewer;
            Model = new MainViewModel();
            this.DataContext = Model;
        }
        
        public double DrawScale {
            get { return ScaleTransform.ScaleX; }
            set {
                dragStarted = true;
                
                ScaleTransform.ScaleX = value;
                ScaleTransform.ScaleY = value;
                
                canvas_main.Width = (BackgroundWidth * ScaleTransform.ScaleX);
                canvas_main.Height = (BackgroundHeight * ScaleTransform.ScaleY);
                
                slider_scale.Value = ScaleTransform.ScaleX * 100;
                
                //PrevPoint = pt;

                dragStarted = false;
            }
        }
        

        private void MainView_Loaded(object sender, RoutedEventArgs e) {
            mParentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            pDev = SystemHandler.Handle.Devices;
            pSeq = SystemHandler.Handle.Sequences;
            pLight = SystemHandler.Handle.Lights;

            ContextList = pSeq.GetContextDictionary();
            //set contextlist
            foreach (IMainView customView in CustomViewList) {
                customView.ContextList = ContextList;
            }

            //view mode
            comboBox_viewMode.Items.Clear();
            string[] names = Enum.GetNames(typeof(MainViewMode));
            for (int i = 0; i < names.Length; i++) {
                comboBox_viewMode.Items.Add(names[i].Replace("_", " "));
            }
            if (comboBox_viewMode.Items.Count > 0) comboBox_viewMode.SelectedIndex = 0;
            
            //sequence 
            comboBox_sequence.Items.Clear();
            comboBox_sequence.Items.Add(COMBOBOX_SEQUENCE_ALL);
            for(int i = 0; i < pSeq.Count; i++) {
                comboBox_sequence.Items.Add(pSeq[i].Name);
            }
            if (comboBox_sequence.Items.Count > 0) comboBox_sequence.SelectedIndex = 0;

            // Shot ComboBox 초기화   //260326 hbk
            comboBox_shot.Items.Clear();   //260326 hbk
            foreach (string name in SHOT_DISPLAY_NAMES)   //260326 hbk
                comboBox_shot.Items.Add(name);   //260326 hbk
            if (comboBox_shot.Items.Count > 0)    //260326 hbk
                comboBox_shot.SelectedIndex = 0;  //260326 hbk

            this.DrawScale = pDev.Config.DrawScale;
        }

        public void AddCustomControl(string name, UserControl control) {
            TabItem newItem = new TabItem();
            newItem.Header = name;
            newItem.Visibility = Visibility.Visible;
            newItem.Height = 42;
            newItem.Content = control;
            tabControl_view.Items.Add(newItem);
            if((control is IMainView) == false) {
                throw new Exception("Custom control is not IMainView type.");
            }
            IMainView mainView = control as IMainView;
            CustomViewList.Add(mainView);
        }

        public void ChangeTabPage(int index) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { 
                tabControl_view.SelectedIndex = index;
            }));
        }

        public void SetParam(ESequence seqID, ParamBase param) {
            if (pSeq[seqID] == null) return;

            string selectedSeq = pSeq[seqID].Name;
            Model.SelectedSeqIndex = comboBox_sequence.Items.IndexOf(COMBOBOX_SEQUENCE_ALL);
            if (Model.SelectedSeqName == COMBOBOX_SEQUENCE_ALL) {
                SequenceContext context = ContextList[selectedSeq];
                DisplayParam(context, param);
            }
        }

        private bool DisplayToBackground(Mat img) {
            try {
                //background
                if (img != null && (img.Empty() == false)) {
                    using (BackgroundImageStream = new MemoryStream()) {
                        img.WriteToStream(BackgroundImageStream, ".bmp");
                        BackgroundImageStream.Seek(0, SeekOrigin.Begin);
                        BitmapFrame frame = BitmapFrame.Create(BackgroundImageStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        canvas_main.Background = new ImageBrush(frame);
                        
                        BackgroundWidth = (int)frame.Width;
                        BackgroundHeight = (int)frame.Height;

                        canvas_main.Width = (BackgroundWidth * DrawScale);
                        canvas_main.Height = (BackgroundHeight * DrawScale);
                    }
                }
                else {
                    canvas_main.Background = Brushes.Black;
                    return false;
                }
            }
            catch(Exception e) {
                Logging.PrintErrLog((int)ELogType.Error, e.Message);
                return false;
            }
            return true;
        }
        
        public async void GrabAndDisplay(ICameraParam param, Action onComplete = null) {   //260326 hbk // 완료 콜백 추가
            if (param == null) return;
            if (pSeq.IsIdle == false) {
                return;
            }
            
            if (GrabTask != null) {
                return;
            }

            GrabTask = Task.Run(() => {
                lock (mDrawInterlock) {
                    //grab 수행
                    //pDev.ApplyProperty(param);
                    bool res = pLight.ApplyLight(param);
                    Mat grabbedImage = pDev.GrabImage(param);
                    param.PutImage(grabbedImage);
                    if (param is InspectionParam ip) {
                        ip.SetOriginalImage(grabbedImage);   //260326 hbk // Grab 시 Shot 뷰어용 원본 이미지 저장
#if SIMUL_MODE
                        // SIMUL B방식: Grab 직후 BlobDetect+FinishAction → Context.Result 갱신 → Strip 색상 반영   //260327 hbk
                        int _shotIdx = Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName);   //260327 hbk
                        SequenceBase _seq = pSeq?[ESequence.Inspection];                     //260327 hbk
                        if (_seq != null && _shotIdx >= 0 && _seq[_shotIdx] is Action_Inspection _act)
                            _act.RunBlobOnLastGrab();   //260327 hbk
#endif
                    }

                    this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                        var camera = pDev[param.DeviceName];   //260326 hbk — null 안전 참조 (NullReferenceException 방지)
                        string stateMsg;
                        Brush br = new SolidColorBrush(Colors.Red);
                        canvas_main.SetParam(param);
                        if (camera == null) {
                            stateMsg = "Device Not Opened";
                            label_message.Foreground = br;
                        }
                        else if (DisplayToBackground(grabbedImage)) {
                            //update state
                            stateMsg = "Grab Success";
                            if (camera.IsGrabFromFile) stateMsg = "Grab From File";   //260326 hbk
                            br = new SolidColorBrush(Colors.Lime);
                            label_message.Foreground = br;
                        }
                        else {
                            stateMsg = "Grab Fail";
                            label_message.Foreground = br;
                        }

                        double elapsedSec = camera != null ? camera.ElapsedTime.TotalMilliseconds / 1000.0 : 0.0;   //260326 hbk
                        string resultStr = string.Format("{0}\n{1} ({2:0.00}s)", param.DeviceName, stateMsg, elapsedSec);
                        label_message.Content = resultStr;

                        // Shot ComboBox 자동 선택   //260326 hbk
                        int shotIdx = Array.IndexOf(SHOT_ACTION_NAMES, param.ActionName);   //260326 hbk
                        if (shotIdx >= 0)   //260326 hbk
                        {
                            _suppressShotComboEvent = true;        //260326 hbk
                            comboBox_shot.SelectedIndex = shotIdx; //260326 hbk
                            _suppressShotComboEvent = false;       //260326 hbk
                        }
                        UpdateShotStrip();   //260326 hbk // 결과 스트립 갱신

                        foreach (IMainView customView in CustomViewList) {
                            customView.Display(param.SequenceName, grabbedImage, resultStr, br, param.ActionName);
                        }

                        canvas_main.InvalidateVisual();
                        onComplete?.Invoke();   //260326 hbk // null 안전 호출 — UpdateShotStrip() 뒤, InvalidateVisual() 뒤
                    }));
                }
            });
            await GrabTask;

            GrabTask.Dispose();
            GrabTask = null;
        }
        
        public void DisplayParam(SequenceContext context, ParamBase param) {
            lock (mDrawInterlock) {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                    //background — ResultImage가 null이면 캔버스 유지 (Grab 미실행 Shot 선택 시 화면 지움 방지)
                    if (context.ResultImage != null)
                        DisplayToBackground(context.ResultImage);

                    canvas_main.SetParam(param);

                    //update state
                    string resultStr = string.Format("{0}\n{1} ({2:0.00}s)", param.ToString(), context.ResultString, ((double)context.Timer.Elapsed.TotalMilliseconds / 1000.0f));
                    label_message.Content = resultStr;
                    switch (context.Result) {
                        case EContextResult.None:
                            label_message.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                        case EContextResult.Pass:
                            label_message.Foreground = new SolidColorBrush(Colors.Lime);
                            break;
                        case EContextResult.Fail:
                            label_message.Foreground = new SolidColorBrush(Colors.Red);
                            break;
                        case EContextResult.Error:
                            label_message.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                    }

                    foreach (IMainView customView in CustomViewList) {
                        customView.Display(param.Parent.Name, context.ResultImage, resultStr, label_message.Foreground, param.OwnerName);
                    }

                    canvas_main.InvalidateVisual();
                }));
            }
        }
        
        //select comboBox
        public void DisplaySequenceContext(SequenceContext context) {
            //comboBox
            string selectedItem = Model.SelectedSeqName;
            if ((selectedItem != COMBOBOX_SEQUENCE_ALL) && (selectedItem != context.Source.Name)) {
                return;
            }

            lock (mDrawInterlock) {
                this.Dispatcher.BeginInvoke( System.Windows.Threading.DispatcherPriority.Normal,new Action(() => {
                    //background
                    DisplayToBackground(context.ResultImage);

                    canvas_main.SetContext(context);
                    string name = context.Source.Name;
                    if (context.ActionParam != null) name = context.ActionParam.ToString();
                    //update state
                    string resultStr = string.Format("{0}\n{1} ({2:0.00}s)", name, context.ResultString, ((double)context.Timer.Elapsed.TotalMilliseconds / 1000.0f));
                    label_message.Content = resultStr;
                    switch (context.Result) {
                        case EContextResult.None:
                            label_message.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                        case EContextResult.Pass:
                            label_message.Foreground = new SolidColorBrush(Colors.Lime);
                            break;
                        case EContextResult.Fail:
                            label_message.Foreground = new SolidColorBrush(Colors.Red);
                            break;
                        case EContextResult.Error:
                            label_message.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                    }

                    foreach (IMainView customView in CustomViewList) {
                        //customView.Display(context.Source.Name, context.ResultImage, resultStr, label_message.Foreground, context.ActionParam.OwnerName); // Origin Code
                        // 2024.01.04 수정 start
                        if (context.ActionParam != null)
                            customView.Display(context.Source.Name, context.ResultImage, resultStr, label_message.Foreground, context.ActionParam.OwnerName);
                        else
                            customView.Display(context.Source.Name, context.ResultImage, resultStr, label_message.Foreground);
                        // 2024.01.04 수정 End
                    }

                    canvas_main.InvalidateVisual();
                }));
            }
        }
        
        private void ComboBox_sequence_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Model.SelectedSeqIndex < 0) return;
            
            string selectedName = Model.SelectedSeqName;
            if (selectedName == COMBOBOX_SEQUENCE_ALL) return;

            SequenceContext context = ContextList[selectedName];
            DisplaySequenceContext(context);
        }

        private void ComboBox_viewMode_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        // Shot ComboBox 선택 변경 → 해당 Shot 이미지 표시   //260326 hbk
        private void ComboBox_shot_SelectionChanged(object sender, SelectionChangedEventArgs e)   //260326 hbk
        {
            if (_suppressShotComboEvent) return;   //260326 hbk
            int idx = comboBox_shot.SelectedIndex;   //260326 hbk
            if (idx < 0 || idx >= SHOT_ACTION_NAMES.Length) return;   //260326 hbk
            RefreshShotViewer(idx);   //260326 hbk
        }

        // 원본/측정 RadioButton 전환   //260326 hbk
        private void RadioButton_imageMode_Checked(object sender, RoutedEventArgs e)   //260326 hbk
        {
            int idx = comboBox_shot.SelectedIndex;   //260326 hbk
            if (idx < 0) return;   //260326 hbk
            RefreshShotViewer(idx);   //260326 hbk
        }

        // 결과 스트립 Border 클릭 → ComboBox 연동   //260326 hbk
        private void StripBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)   //260326 hbk
        {
            if (sender is Border border && border.Tag is string tagStr)   //260326 hbk
            {
                if (int.TryParse(tagStr, out int shotIdx))   //260326 hbk
                {
                    _suppressShotComboEvent = true;   //260326 hbk
                    comboBox_shot.SelectedIndex = shotIdx;   //260326 hbk
                    _suppressShotComboEvent = false;   //260326 hbk
                    RefreshShotViewer(shotIdx);        //260326 hbk
                }
            }
        }

        // Shot 인덱스에 해당하는 InspectionParam 조회   //260326 hbk
        private InspectionParam GetInspectionParam(int shotIndex)   //260326 hbk
        {
            if (pSeq == null) return null;   //260326 hbk
            SequenceBase seq = pSeq[ESequence.Inspection];   //260326 hbk
            if (seq == null || shotIndex < 0 || shotIndex >= seq.ActionCount) return null;   //260326 hbk
            return seq[shotIndex].Param as InspectionParam;   //260326 hbk
        }

        // Shot 뷰어 갱신 — 선택된 Shot의 원본 or 측정 이미지 표시   //260326 hbk
        private void RefreshShotViewer(int shotIndex)   //260326 hbk
        {
            InspectionParam param = GetInspectionParam(shotIndex);   //260326 hbk
            if (param == null) return;   //260326 hbk

            bool showOriginal = radioButton_original.IsChecked == true;   //260326 hbk
            Mat imgToShow = showOriginal
                ? param.LastOriginalImage
                : (param.LastAnnotatedImage ?? param.GetAnnotatedImageTemp());   //260327 hbk — SIMUL에서 Temp fallback

            // 이미지가 없으면 캔버스 검정   //260326 hbk
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {   //260326 hbk
                DisplayToBackground(imgToShow);   //260326 hbk // 기존 메서드 재사용
            }));   //260326 hbk
        }

        // 결과 스트립 색상 업데이트 (외부에서 호출 가능)   //260326 hbk
        public void UpdateShotStrip()   //260326 hbk
        {
            if (pSeq == null) return;   //260326 hbk
            SequenceBase seq = pSeq[ESequence.Inspection];   //260326 hbk
            if (seq == null) return;   //260326 hbk

            Border[] stripBorders = {   //260326 hbk
                stripBorder_Shot1, stripBorder_Shot2, stripBorder_Shot3,   //260326 hbk
                stripBorder_Shot4, stripBorder_Shot5   //260326 hbk
            };

            for (int i = 0; i < stripBorders.Length && i < seq.ActionCount; i++)   //260326 hbk
            {
                EContextResult result = seq[i].Context.Result;   //260326 hbk
                SolidColorBrush bg;   //260326 hbk
                switch (result)   //260326 hbk
                {
                    case EContextResult.Pass:
                        bg = new SolidColorBrush(Colors.Lime);   //260326 hbk // OK = 초록
                        break;
                    case EContextResult.Fail:
                        bg = new SolidColorBrush(Colors.Red);    //260326 hbk // NG = 빨강
                        break;
                    default:
                        bg = new SolidColorBrush(Colors.Gray);   //260326 hbk // 미실행 = 회색
                        break;
                }
                stripBorders[i].Background = bg;   //260326 hbk
            }
        }
        
        private void Canvas_main_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {

            lastMousePositionOnTarget = Mouse.GetPosition(canvas_main);

            double zoom = e.Delta > 0 ? .05 : -.05;
            DrawScale += zoom;
            
            if (DrawScale <= 0.2) DrawScale = 0.2;
            else if (DrawScale > 2.0) DrawScale = 2.0;

            
            e.Handled = true;
        }
       

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e) {
            DrawScale = slider_scale.Value / 100;
            this.dragStarted = false;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e) {
            this.dragStarted = true;
        }

        private void Slider_scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (this.IsLoaded == false) return;

            var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, canvas_main);

            
            if (!dragStarted) {
                DrawScale = slider_scale.Value / 100;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0) {
                System.Windows.Point? targetBefore = null;
                System.Windows.Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue) {
                    if (lastCenterPositionOnTarget.HasValue) {
                        var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        System.Windows.Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, canvas_main);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(canvas_main);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue) {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / canvas_main.Width;
                    double multiplicatorY = e.ExtentHeight / canvas_main.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY)) {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
    }
}
