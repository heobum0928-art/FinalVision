using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
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
        private readonly ScaleTransform _scale = new ScaleTransform(1, 1);
        private int _bgW, _bgH;
        private MemoryStream _bgStream;
        private InspectionParam _subscribedParam = null;          //260330 hbk — ROIShapeChanged 구독 중인 param 추적
        private ERoiShape _lastAppliedROIShape = ERoiShape.Rectangle;  //260330 hbk — Edit 모드 아닐 때 되돌릴 기준 shape
        private bool _revertingROIShape = false;                        //260330 hbk — 되돌리기 중 재진입 방지

        private bool _dragStarted = false;
        private System.Windows.Point? _lastMousePos;
        private System.Windows.Point? _lastCenterPos;
        private bool _editPermitted = false;   //260330 hbk — 로그인 등 전역 Edit 권한
        private SequenceBase _subscribedSeq = null; //260402 hbk OnFinish 해제용 시퀀스 참조

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
            _pDev = SystemHandler.Handle.Devices;
            _pSeq = SystemHandler.Handle.Sequences;
            _pLight = SystemHandler.Handle.Lights;

            // TCP 검사 완료 시 UI 자동 갱신 — OnFinish 구독   //260330 hbk
            var seq = _pSeq[ESequence.Inspection];
            if (seq != null)
            {
                _subscribedSeq = seq; //260402 hbk 해제용 참조 저장
                seq.OnFinish += OnInspectionFinish; //260402 hbk 람다→named handler (해제 가능)
            }
        }


        //260402 hbk OnFinish named handler (람다 대체 — 이벤트 해제 가능)
        private void OnInspectionFinish(SequenceContext ctx)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                RefreshImage();
                UpdateResultLabel();
            }));
        }

        //260402 hbk 이벤트 정리 (MainWindow 종료 시 호출)
        public void Cleanup()
        {
            if (_subscribedSeq != null)
            {
                _subscribedSeq.OnFinish -= OnInspectionFinish;
                _subscribedSeq = null;
            }
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

        //260406 hbk -- D-01: ShotTabView Grab 버튼 제거 (InspectionListView button_grab으로 통합)

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

            // ROIShape 변경 이벤트 구독 — 이전 param 해제 후 새 param 구독   //260330 hbk

            if (_subscribedParam != param)   //260330 hbk
            {
                if (_subscribedParam != null)
                    _subscribedParam.ROIShapeChanged -= OnParamROIShapeChanged;   //260330 hbk
                param.ROIShapeChanged += OnParamROIShapeChanged;                  //260330 hbk
                _subscribedParam = param;                                          //260330 hbk
            }
            _lastAppliedROIShape = param.ROIShape;   //260330 hbk — 현재 param의 shape를 기준으로 초기화

            bool showOriginal = rb_original.IsChecked == true;
            Mat img;                                                                       //260401 hbk
            if (showOriginal)
            {
                img = param.LastOriginalImage;                                              //260401 hbk — 원본 보기 모드
            }
            else
            {
                if (param.LastAnnotatedImage != null)
                    img = param.LastAnnotatedImage;                                         //260401 hbk — 1순위: 실검사 결과 이미지
                else if (param.GetAnnotatedImageTemp() != null)
                    img = param.GetAnnotatedImageTemp();                                    //260401 hbk — 2순위: 시뮬 재검사 임시 이미지
                else
                    img = param.LastOriginalImage;                                          //260401 hbk — 3순위: 둘 다 없으면 원본이라도
            }

            DisplayToBackground(img);
            canvas_shot.SetParam((ParamBase)param);

            // 경로 표시 갱신   //260331 hbk
            if (!string.IsNullOrEmpty(param.SimulImagePath))
            {
                tb_imagePath.Text       = Path.GetFileName(param.SimulImagePath);   //260331 hbk
                tb_imagePath.Foreground = System.Windows.Media.Brushes.White;       //260331 hbk
                tb_imagePath.ToolTip    = param.SimulImagePath;                      //260331 hbk
            }
            else
            {
                tb_imagePath.Text       = "(없음)";                                   //260331 hbk
                tb_imagePath.Foreground = System.Windows.Media.Brushes.Gray;        //260331 hbk
                tb_imagePath.ToolTip    = null;                                      //260331 hbk
            }
        }

        // ROIShape 변경 시 DrawableList 즉시 갱신 — Edit 모드일 때만 허용, 아니면 되돌리기   //260330 hbk
        private void OnParamROIShapeChanged(object sender, EventArgs e)   //260330 hbk
        {
            if (_revertingROIShape) return;   //260330 hbk — 되돌리기 중 재진입 차단

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                var param = GetParam();
                if (param == null) return;

                if (!canvas_shot.IsEditable)   //260330 hbk Edit ROI 버튼 비활성 상태면 변경 되돌리기
                {
                    _revertingROIShape = true;
                    param.ROIShape = _lastAppliedROIShape;   //260330 hbk — 이전 shape로 복원
                    _revertingROIShape = false;
                    return;
                }

                _lastAppliedROIShape = param.ROIShape;   //260330 hbk — 성공적으로 적용된 shape 기록
                canvas_shot.SetParam((ParamBase)param);
            }));
        }

        // Shot별 이미지 열기   //260331 hbk
        private void Btn_OpenImage_Click(object sender, RoutedEventArgs e)
        {
            var param = GetParam();
            if (param == null) return;

            var dlg = new OpenFileDialog
            {
                Title  = "Shot 이미지 파일 선택",
                Filter = "이미지 파일|*.bmp;*.jpg;*.jpeg;*.png;*.tiff|모든 파일|*.*",
            };
            if (!string.IsNullOrEmpty(param.SimulImagePath))
                dlg.InitialDirectory = Path.GetDirectoryName(param.SimulImagePath);

            if (dlg.ShowDialog() != true) return;

            param.SimulImagePath = dlg.FileName;   //260331 hbk — 경로 저장 (레시피에 자동 저장)

            // 파일 즉시 로드하여 화면 표시   //260331 hbk
            var mat = OpenCvSharp.Cv2.ImRead(param.SimulImagePath, OpenCvSharp.ImreadModes.Color);
            param.SetOriginalImage(mat);
            mat?.Dispose();

            // 로드된 이미지로 Blob 검사 실행   //260401 hbk
            var seq = _pSeq[ESequence.Inspection];
            if (seq != null && seq[_shotIndex] is Action_Inspection act)
                act.RunBlobOnLastGrab();

            RefreshImage();
            UpdateResultLabel();   //260401 hbk — OK/NG 결과 즉시 반영
        }

        // Shot별 이미지 삭제   //260331 hbk
        private void Btn_DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            var param = GetParam();
            if (param == null) return;

            param.SimulImagePath = "";          //260331 hbk — 경로 초기화
            param.SetOriginalImage(null);       //260331 hbk — 이미지 버퍼 해제
            RefreshImage();
        }

        // 결과 레이블 갱신 — OK/NG/---   //260327 hbk Shot탭
        public void UpdateResultLabel()
        {
            var seq = _pSeq?[ESequence.Inspection];
            if (seq == null || _shotIndex >= seq.ActionCount) return;

            var result = seq[_shotIndex].Context.Result;
            var p = seq[_shotIndex].Param as InspectionParam;
            double blobArea = p?.LastBlobArea ?? 0;
            switch (result)
            {
                case EContextResult.Pass:
                    label_result.Content = $"OK  {blobArea:F0}";   //260331 hbk — 면적 표시
                    label_result.Foreground = new SolidColorBrush(Colors.Lime);
                    break;
                case EContextResult.Fail:
                    label_result.Content = $"NG  {blobArea:F0}";   //260331 hbk — NG시 0 또는 실제 면적
                    label_result.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    label_result.Content = "---";
                    label_result.Foreground = new SolidColorBrush(Colors.Gray);
                    break;
            }
        }

        ///캔버스 배경에 이미지 표시   //260327 hbk Shot탭
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
                    canvas_shot.SetDisplayMat(img);   //260331 hbk — RuntimeResizer.CurrentPosDisplay용 Mat 전달
                    // Grab 시 슬라이더 현재값 적용 (기본 52%) — _scale이 초기 1.0인 문제 방지   //260330 hbk
                    double s = Math.Max(0.2, Math.Min(2.0, slider_scale.Value / 100.0));   //260330 hbk
                    _scale.ScaleX = s;   //260330 hbk
                    _scale.ScaleY = s;   //260330 hbk
                    canvas_shot.Width = _bgW * s;
                    canvas_shot.Height = _bgH * s;
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
