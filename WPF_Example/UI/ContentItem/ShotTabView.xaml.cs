using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenCvSharp;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Sequence;
using FinalVisionProject.Utility;

namespace FinalVisionProject.UI
{
    // Shot 탭 UserControl — Grab + ROI 티칭 + 결과 확인   //260327 hbk Shot탭
    public partial class ShotTabView : UserControl
    {
        private int _shotIndex = -1;
        private DeviceHandler  _pDev;
        private SequenceHandler _pSeq;
        private LightHandler   _pLight;
        private Task _grabTask;
        private readonly ScaleTransform _scale = new ScaleTransform(1, 1);
        private int _bgW, _bgH;
        private MemoryStream _bgStream;

        private bool _dragStarted = false;
        private System.Windows.Point? _lastMousePos;
        private System.Windows.Point? _lastCenterPos;
        private bool _editPermitted = false;   //260330 hbk — 로그인 등 전역 Edit 권한

        public bool IsEditable   //260330 hbk — 전역 Edit 권한 (로그인 상태 연동)
        {
            get { return _editPermitted; }
            set
            {
                _editPermitted = value;
                btn_editRoi.IsEnabled = value;           //260330 hbk — 권한 없으면 버튼 비활성
                if (!value)
                {
                    btn_editRoi.IsChecked  = false;      //260330 hbk — 권한 해제 시 Edit 모드도 해제
                    canvas_shot.IsEditable = false;
                    canvas_shot.InvalidateVisual();
                }
            }
        }

        public double DrawScale
        {
            get { return _scale.ScaleX; }
            set
            {
                double s = Math.Max(0.2, Math.Min(2.0, value));
                _scale.ScaleX      = s;
                _scale.ScaleY      = s;
                canvas_shot.Width  = _bgW * s;
                canvas_shot.Height = _bgH * s;
                _dragStarted = true;
                slider_scale.Value = s * 100;
                _dragStarted = false;
            }
        }

        public ShotTabView()
        {
            InitializeComponent();
            // RenderTransform 제거 — Width 조정 + dc.PushTransform(_ScaleTransform)으로 충분   //260330 hbk
            canvas_shot._ScaleTransform    = _scale;
            canvas_shot.ParentScrollViewer = sv_shot;
            canvas_shot.IsEditable         = false;
        }

        // MainView_Loaded에서 호출 — shot 인덱스 0~4   //260327 hbk Shot탭
        public void Initialize(int shotIndex)
        {
            _shotIndex = shotIndex;
            _pDev   = SystemHandler.Handle.Devices;
            _pSeq   = SystemHandler.Handle.Sequences;
            _pLight = SystemHandler.Handle.Lights;
        }

        // 해당 Shot의 InspectionParam 반환
        private InspectionParam GetParam()
        {
            if (_pSeq == null || _shotIndex < 0) return null;
            var seq = _pSeq[ESequence.Inspection];
            if (seq == null || _shotIndex >= seq.ActionCount) return null;
            return seq[_shotIndex].Param as InspectionParam;
        }

        // Edit ROI 토글 버튼 — 활성 시 캔버스 ROI 드래그 허용   //260330 hbk
        private void BtnEditRoi_Click(object sender, RoutedEventArgs e)
        {
            canvas_shot.IsEditable = btn_editRoi.IsChecked == true;
            canvas_shot.InvalidateVisual();
        }

        // Grab 버튼   //260327 hbk Shot탭
        private async void Btn_Grab_Click(object sender, RoutedEventArgs e)
        {
            if (_grabTask != null) return;
            var param = GetParam();
            if (!(param is ICameraParam camParam)) return;
            if (!_pSeq.IsIdle) return;

            btn_grab.IsEnabled = false;
            _grabTask = Task.Run(() =>
            {
                _pLight.ApplyLight(camParam);
                Mat grabbed = _pDev.GrabImage(camParam);
                camParam.PutImage(grabbed);
                param.SetOriginalImage(grabbed);

#if SIMUL_MODE
                var seq = _pSeq[ESequence.Inspection];
                if (seq != null && seq[_shotIndex] is Action_Inspection act)
                    act.RunBlobOnLastGrab();
#endif

                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    RefreshImage();
                    UpdateResultLabel();
                    btn_grab.IsEnabled = true;
                }));
            });
            await _grabTask;
            _grabTask.Dispose();
            _grabTask = null;
        }

        // 원본/측정 RadioButton 전환
        private void Rb_ImageMode_Checked(object sender, RoutedEventArgs e)
        {
            RefreshImage();
        }

        // 이미지 갱신 — 원본 or 측정 선택에 따라   //260327 hbk Shot탭
        public void RefreshImage()
        {
            var param = GetParam();
            if (param == null) return;

            bool showOriginal = rb_original.IsChecked == true;
            Mat img = showOriginal
                ? param.LastOriginalImage
                : (param.LastAnnotatedImage ?? param.GetAnnotatedImageTemp() ?? param.LastOriginalImage);

            DisplayToBackground(img);
            canvas_shot.SetParam((ParamBase)param);
        }

        // 결과 레이블 갱신 — OK/NG/---   //260327 hbk Shot탭
        public void UpdateResultLabel()
        {
            var seq = _pSeq?[ESequence.Inspection];
            if (seq == null || _shotIndex >= seq.ActionCount) return;

            var result = seq[_shotIndex].Context.Result;
            switch (result)
            {
                case EContextResult.Pass:
                    label_result.Content    = "OK";
                    label_result.Foreground = new SolidColorBrush(Colors.Lime);
                    break;
                case EContextResult.Fail:
                    label_result.Content    = "NG";
                    label_result.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    label_result.Content    = "---";
                    label_result.Foreground = new SolidColorBrush(Colors.Gray);
                    break;
            }
        }

        // 캔버스 배경에 이미지 표시   //260327 hbk Shot탭
        private void DisplayToBackground(Mat img)
        {
            try
            {
                if (img != null && !img.Empty())
                {
                    _bgStream?.Dispose();
                    _bgStream = new MemoryStream();
                    img.WriteToStream(_bgStream, ".bmp");
                    var frame = BitmapFrame.Create(_bgStream,
                        BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    canvas_shot.Background = new ImageBrush(frame);
                    _bgW = (int)frame.Width;
                    _bgH = (int)frame.Height;
                    canvas_shot.Width  = _bgW * _scale.ScaleX;
                    canvas_shot.Height = _bgH * _scale.ScaleY;
                }
                else
                {
                    canvas_shot.Background = Brushes.Black;
                }
            }
            catch { }
        }

        // 마우스 휠 줌 — 마우스 위치 기준으로 확대/축소
        private void Canvas_shot_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _lastMousePos = Mouse.GetPosition(canvas_shot);
            DrawScale += e.Delta > 0 ? 0.05 : -0.05;
            e.Handled = true;
        }

        // 슬라이더
        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _dragStarted = true;
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DrawScale = slider_scale.Value / 100;
            _dragStarted = false;
        }

        private void Slider_scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded) return;
            var center = new System.Windows.Point(sv_shot.ViewportWidth / 2, sv_shot.ViewportHeight / 2);
            _lastCenterPos = sv_shot.TranslatePoint(center, canvas_shot);
            if (!_dragStarted)
                DrawScale = slider_scale.Value / 100;
        }

        // 줌 후 스크롤 위치 보정 — MainView와 동일한 로직
        private void Sv_shot_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0 && e.ExtentWidthChange == 0) return;

            System.Windows.Point? before = null;
            System.Windows.Point? after  = null;

            if (!_lastMousePos.HasValue)
            {
                if (_lastCenterPos.HasValue)
                {
                    var center = new System.Windows.Point(sv_shot.ViewportWidth / 2, sv_shot.ViewportHeight / 2);
                    before = _lastCenterPos;
                    after  = sv_shot.TranslatePoint(center, canvas_shot);
                }
            }
            else
            {
                before        = _lastMousePos;
                after         = Mouse.GetPosition(canvas_shot);
                _lastMousePos = null;
            }

            if (!before.HasValue) return;

            double dx  = (after.Value.X - before.Value.X) * (e.ExtentWidth  / canvas_shot.Width);
            double dy  = (after.Value.Y - before.Value.Y) * (e.ExtentHeight / canvas_shot.Height);
            double newX = sv_shot.HorizontalOffset - dx;
            double newY = sv_shot.VerticalOffset   - dy;

            if (double.IsNaN(newX) || double.IsNaN(newY)) return;
            sv_shot.ScrollToHorizontalOffset(newX);
            sv_shot.ScrollToVerticalOffset(newY);
        }
    }
}
