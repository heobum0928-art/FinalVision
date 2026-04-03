using System;
using System.Collections.Generic;
using PropertyTools.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;
using FinalVisionProject.Setting;
using OpenCvSharp;
using System.Windows;

//260326 hbk — Action_Inspection: 5-Shot 공통 검사 Action + InspectionParam + SimpleBlobDetector
namespace FinalVisionProject.Sequence
{
    public class InspectionActionContext : ActionContext   //260326 hbk — Action 결과 Context
    {
        #region constructors
        public InspectionActionContext(ActionBase source) : base(source)   //260326 hbk — 생성자, source=소유 Action
        {
        }
        #endregion
    }

    public enum ERoiShape { Rectangle, Circle }   //260327 hbk 그리기 — ROI 도형 선택 (Rectangle 또는 Circle)

    public class InspectionParam : CameraSlaveParam   //260326 hbk — Shot별 독립 파라미터 (ROI/Blob/Delay)
    {
        #region fields
        public readonly int ShotIndex;   //260326 hbk — Shot 번호 (0=Bolt_One ~ 4=Assy_Rail_Two)
        #endregion

        #region properties

        [Category("Common")]                   //260326 hbk
        [ReadOnly(true)]
        public string ProcessName { get; set; }   //260326 hbk — UI 표시용 Action 이름 (ReadOnly)

        private ERoiShape _roiShape = ERoiShape.Rectangle;   //260330 hbk — full property로 변경 (변경 이벤트 지원)
        [Category("ROI Setting")]                  //260326 hbk
        public ERoiShape ROIShape   //260330 hbk
        {
            get => _roiShape;
            set
            {
                if (_roiShape == value) return;
                _roiShape = value;
                ROIShapeChanged?.Invoke(this, EventArgs.Empty);   //260330 hbk — PropertyGrid 변경 즉시 캔버스 갱신 트리거
            }
        }
        public event EventHandler ROIShapeChanged;   //260330 hbk — ROIShape 변경 시 ShotTabView에서 SetParam 재호출

        [Category("ROI Setting")]                  //260326 hbk
        [Rectangle, Converter(typeof(UI.RectConverter))]
        public System.Windows.Rect ROI { get; set; }   //260326 hbk — 촬상 검사 영역 (Shot별 개별 설정)

        [Category("ROI Setting")]                  //260327 hbk 그리기
        [Circle, Converter(typeof(UI.CircleConverter))]   //260327 hbk 그리기
        public UI.Circle ROICircle { get; set; }          //260327 hbk 그리기 — 원형 ROI (ROIShape=Circle 시 사용)

        [Category("Blob")]                                        //260326 hbk
        public double BlobMinArea { get; set; } = 100000;         //260326 hbk Blob 최소 면적 (자재 없으면 미검출)
        public double BlobMaxArea { get; set; } = 9999999;       //260326 hbk Blob 최대 면적 (노이즈 필터)
        public int BlobThreshold { get; set; } = 100;            //260327 hbk 이진화 임계값 (0~255, ROI 내부 적용)

        [Category("General")]                       //260326 hbk
        public int DelayMs { get; set; } = 0;       //260326 hbk — 촬상 전 대기시간(ms), 자재 이동 안정화용

        [System.ComponentModel.Browsable(false)]                           //260331 hbk — PropertyGrid 표시 제외
        public string SimulImagePath { get; set; } = "";                   //260331 hbk — Shot별 개별 로드 이미지 파일 경로

        [System.ComponentModel.Browsable(false)]                           //260331 hbk
        public double LastBlobArea { get; set; } = 0;                      //260331 hbk — 최근 검사 Blob 면적 (검출 없으면 0)

        // Shot 이미지 버퍼   //260326 hbk // Shot별 원본/오버레이 이미지 보관
        [System.ComponentModel.Browsable(false)]   //260330 hbk — PropertyGrid 표시 제외 (내부 이미지 버퍼)
        public Mat LastOriginalImage { get; private set; }    //260326 hbk // Grab 시 저장 (항상 최신 원본)
        [System.ComponentModel.Browsable(false)]   //260330 hbk — PropertyGrid 표시 제외 (내부 이미지 버퍼)
        public Mat LastAnnotatedImage { get; private set; }   //260326 hbk // 실검사 완료 시 1회 저장, 이후 잠금

        // 잠금 플래그: 실운영(SIMUL_MODE 미정의) BlobDetect 완료 후 true   //260326 hbk
        private bool _AnnotatedImageLocked = false;           //260326 hbk

        private readonly object _imageLock = new object();   //260403 hbk — 이미지 접근 동기화

        public void SetOriginalImage(Mat img)   //260326 hbk // Grab 완료 후 호출
        {
            lock (_imageLock) {   //260403 hbk — TCP/UI 스레드 동시 접근 방지
                LastOriginalImage?.Dispose();        //260326 hbk // 이전 이미지 해제
                LastOriginalImage = img?.Clone();    //260326 hbk // 클론 저장 (원본 생명주기 독립)
            }
        }

        //260403 hbk — 시뮬모드: 로드된 이미지 Clone 반환 (원본 보호)
        public Mat GetOriginalImageClone() {
            lock (_imageLock) {
                return LastOriginalImage?.Clone();
            }
        }

        public void SetAnnotatedImage(Mat img)   //260326 hbk // 실검사(비SIMUL) BlobDetect 완료 후 1회만 호출
        {
            if (_AnnotatedImageLocked) return;   //260326 hbk // SIMUL 재검사 시 덮어쓰기 방지
            LastAnnotatedImage?.Dispose();       //260326 hbk
            LastAnnotatedImage = img?.Clone();   //260326 hbk
            _AnnotatedImageLocked = true;        //260326 hbk // 한 번 저장 후 잠금
        }

        // SIMUL 재검사용 임시 이미지 전달 — LastAnnotatedImage 변경 없음   //260326 hbk
        // 호출자(Action_Inspection)가 이 Mat을 직접 DisplayToBackground로 전달한다   //260326 hbk
        private Mat _AnnotatedImageTemp = null;   //260326 hbk

        public void SetAnnotatedImageTemp(Mat img)   //260326 hbk // SIMUL 재검사 시 캔버스용 임시 저장
        {
            _AnnotatedImageTemp?.Dispose();       //260326 hbk
            _AnnotatedImageTemp = img?.Clone();   //260326 hbk // LastAnnotatedImage 변경 없음
        }

        public Mat GetAnnotatedImageTemp()   //260326 hbk // 캔버스 표시 후 호출자가 가져감
        {
            return _AnnotatedImageTemp;   //260326 hbk
        }

        public void ResetAnnotatedImageLock()   //260326 hbk // 실운영 새 사이클 시작 시 잠금 해제
        {
            _AnnotatedImageLocked = false;       //260326 hbk
        }

        #endregion

        #region methods

        public override bool Load(IniFile loadFile, string groupName)   //260326 hbk
        {
            bool result = base.Load(loadFile, groupName);
            // 레시피에 구 카메라 이름이 저장되어 있어도 항상 올바른 카메라로 강제
            DeviceName = DeviceHandler.INSPECTION_CAMERA;
            return result;
        }

        public override bool Save(IniFile saveFile, string groupName)   //260326 hbk
        {
            return base.Save(saveFile, groupName);
        }

        public override bool CopyTo(ParamBase param)   //260330 hbk — Copy/Paste: InspectionParam 전용 필드도 복사
        {
            if (!(param is InspectionParam target)) return false;
            bool result = base.CopyTo(param);   //260330 hbk — 카메라/조명 설정 복사
            target.ROIShape      = this.ROIShape;       //260330 hbk
            target.ROI           = this.ROI;            //260330 hbk
            target.ROICircle     = this.ROICircle;      //260330 hbk
            target.BlobMinArea   = this.BlobMinArea;    //260330 hbk
            target.BlobMaxArea   = this.BlobMaxArea;    //260330 hbk
            target.BlobThreshold = this.BlobThreshold;  //260330 hbk
            target.DelayMs       = this.DelayMs;        //260330 hbk
            target.SimulImagePath = this.SimulImagePath; //260331 hbk
            return result;
        }

        #endregion

        #region constructors
        public InspectionParam(object owner, int shotIndex) : base(owner)   //260326 hbk
        {
            ShotIndex = shotIndex;
        }
        #endregion
    }

    public class Action_Inspection : ActionBase   //260326 hbk 5-Shot 공통 Action (Grab, BlobDetect, SaveImage, End)
    {
        #region fields
        private VirtualCamera _Camera;          //260326 hbk — HIK 카메라 (또는 SIMUL VirtualCamera)
        private InspectionParam _MyParam;       //260326 hbk — Shot별 독립 파라미터 참조
        private Mat _GrabbedImage;              //260326 hbk — Grab된 원본 이미지
        private bool _IsOK;                     //260326 hbk — 검사 판정 결과 (true=OK, false=NG)
        private string _FolderPath = "";        //260403 hbk -- current inspection time-folder path (D-09)

        public enum EStep                       //260326 hbk — Run() 상태머신 Step 정의
        {
            Grab       = 0,
            BlobDetect = 1,
            SaveImage  = 2,
            End        = 3,
        }
        #endregion

        #region methods

        //260403 hbk -- capture time-folder path from InspectionSequenceContext (D-09)
        public override void OnBegin(SequenceContext prevResult = null)
        {
            base.OnBegin(prevResult);   //260403 hbk -- Step 리셋, Timer 재시작, Context 초기화
            var inspContext = prevResult as InspectionSequenceContext;   //260403 hbk -- InspectionSequenceContext로 캐스트
            if (inspContext != null)
            {
                _FolderPath = inspContext.CurrentFolderPath;   //260403 hbk -- 검사 시작 시 생성된 time-folder 경로 캡처
            }
        }

        public override void OnLoad()   //260326 hbk
        {
            _MyParam.ProcessName = Param.OwnerName;

            _Camera = SystemHandler.Handle.Devices[_MyParam.DeviceName];   //260326 hbk
            if (_Camera != null)
            {
                if (_Camera.Properties == null)
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Not Open!", "Camera is not open. Please check your connection status.", System.Windows.MessageBoxImage.Error);
                    return;
                }
                if (!_Camera.Properties.ApplyFromParam(_MyParam))
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Property Set Fail!", "Check camera settings. or camera state.", System.Windows.MessageBoxImage.Error);
                }
                if (!_Camera.SetSoftwareTriggerMode())
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Software trigger mode Set Fail!", "Check camera settings. or camera state.", System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                CustomMessageBox.Show(_MyParam.DeviceName + " Camera Not Open!", "Camera is not open. Please check your connection status.", System.Windows.MessageBoxImage.Error);
                return;
            }

            base.OnLoad();
        }

        public override ActionContext Run()   //260326 hbk
        {
            switch ((EStep)Step)
            {
                case EStep.Grab:   //260326 hbk
                    // 딜레이 적용
                    if (_MyParam.DelayMs > 0)
                        System.Threading.Thread.Sleep(_MyParam.DelayMs);
#if SIMUL_MODE
                    //260403 hbk — 시뮬모드: 로드된 이미지 Clone으로 재사용, 없으면 VirtualCamera Grab
                    _GrabbedImage = _MyParam.GetOriginalImageClone() ?? _Camera.GrabImage();
                    //260403 hbk — 시뮬모드에서는 SetOriginalImage 호출 안 함 (원본 보존)
#else
                    //260403 hbk — 실제모드: 카메라에서 Grab
                    _GrabbedImage = _Camera.GrabImage();
                    _MyParam.SetOriginalImage(_GrabbedImage);   //260326 hbk // 원본 이미지 버퍼 저장
#endif
                    if (_GrabbedImage == null)
                    {
                        Logging.PrintLog((int)ELogType.Trace, "[ALGO] {0} Grab 실패 NG", Name);   //260330 hbk
                        _IsOK = false;
                        Step = (int)EStep.SaveImage;   //260326 hbk Grab 실패시 SaveImage로 이동
                        break;
                    }
                    Logging.PrintLog((int)ELogType.Trace, "[ALGO] {0} Grab OK", Name);   //260330 hbk
                    Step++;
                    break;

                case EStep.BlobDetect:   //260326 hbk
                    Mat annotated;
                    _IsOK = RunBlobDetection(_GrabbedImage, _MyParam, out annotated);      //260326 hbk
                    Logging.PrintLog((int)ELogType.Trace, "[ALGO] {0} BlobDetect Result:{1}", Name, _IsOK ? "OK" : "NG");   //260330 hbk
#if SIMUL_MODE
                    // SIMUL 재검사: LastAnnotatedImage 변경 없이 캔버스용 임시 저장   //260326 hbk
                    _MyParam.SetAnnotatedImageTemp(annotated);   //260326 hbk // LastAnnotatedImage 잠금 유지
                    // 캔버스 표시는 GrabAndDisplay(MainView)에서 처리 — DisplayToBackground(annotated) 호출
#else
                    // 실운영: 최초 1회만 LastAnnotatedImage 갱신 (이후 잠금)   //260326 hbk
                    _MyParam.SetAnnotatedImage(annotated);   //260326 hbk
#endif
                    annotated?.Dispose();   //260326 hbk // Clone됐으므로 로컬 해제
                    Step++;
                    break;

                case EStep.SaveImage:   //260326 hbk
                    SaveResultImage(_GrabbedImage, _IsOK);   //260326 hbk
                    Step++;
                    break;

                case EStep.End:   //260326 hbk
                    Logging.PrintLog((int)ELogType.Trace, "[ALGO] {0} End, Result:{1}", Name, _IsOK ? "Pass" : "Fail");   //260330 hbk
                    FinishAction(_IsOK ? EContextResult.Pass : EContextResult.Fail);   //260326 hbk
                    break;
            }
            return base.Run();
        }

        // SIMUL B방식 — Grab 완료 후 즉시 BlobDetect+FinishAction 수행 (Context.Result 갱신)   //260327 hbk
        public void RunBlobOnLastGrab()   //260327 hbk
        {
            Mat img = _MyParam.LastOriginalImage;   //260327 hbk
            if (img == null)
                return;               //260327 hbk
            Mat annotated;
            _IsOK = RunBlobDetection(img, _MyParam, out annotated);     //260327 hbk
            _MyParam.SetAnnotatedImageTemp(annotated);   //260327 hbk — SIMUL: Temp 갱신, LastAnnotatedImage 잠금 유지
            annotated?.Dispose();                        //260327 hbk
            SaveResultImage(img, _IsOK);                 //260327 hbk
            FinishAction(_IsOK ? EContextResult.Pass : EContextResult.Fail);   //260327 hbk — Context.Result 갱신
        }

        // Blob 검출 — SimpleBlobDetector, filterByArea만 + 오버레이 Mat 반환   //260326 hbk
        private bool RunBlobDetection(Mat image, InspectionParam param, out Mat annotated)   //260326 hbk
        {
            if (image == null)
            {
                annotated = null;
                return false;   //260326 hbk
            }

            Mat gray = null; //260402 hbk try 밖 선언 (finally에서 Dispose 보장)
            Mat roiMat = null; //260402 hbk try 밖 선언 (finally에서 Dispose 보장)
            bool needDisposeGray = false; //260402 hbk gray가 새로 생성된 경우만 Dispose

            try
            {
                // Gray 변환   //260326 hbk
                if (image.Channels() == 1)
                    gray = image;
                else
                {
                    gray = new Mat();
                    Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
                    needDisposeGray = true; //260402 hbk 새로 생성한 경우 표시
                }

                // ROI 결정: ROIShape에 따라 Rectangle 또는 Circle 적용   //260327 hbk 그리기
                int x, y, w, h;
                bool isCircleMode = (param.ROIShape == ERoiShape.Circle);   //260327 hbk 그리기
                OpenCvSharp.Point circleCenter = new OpenCvSharp.Point(0, 0);   //260327 hbk 그리기
                int circleRadius = 0;                                            //260327 hbk 그리기

                if (isCircleMode)   //260327 hbk 그리기
                {
                    var circ = param.ROICircle;
                    circleRadius = (int)circ.Radius;
                    if (circleRadius <= 0)
                    {
                        annotated = null;
                        return false;   //260327 hbk Circle 미설정이면 NG 반환
                    }
                    circleCenter = new OpenCvSharp.Point((int)circ.CenterX, (int)circ.CenterY);
                    x = Math.Max(0, (int)circ.CenterX - circleRadius);
                    y = Math.Max(0, (int)circ.CenterY - circleRadius);
                    int bx2 = Math.Min(gray.Width, (int)circ.CenterX + circleRadius);
                    int by2 = Math.Min(gray.Height, (int)circ.CenterY + circleRadius);
                    w = bx2 - x;
                    h = by2 - y;
                    if (w <= 0 || h <= 0)
                    {
                        annotated = null;
                        return false;   //260327 hbk 그리기
                    }
                    roiMat = new Mat(gray, new OpenCvSharp.Rect(x, y, w, h)).Clone();   //260327 hbk 그리기 — Clone으로 독립 메모리
                }
                else
                {
                    var roi = param.ROI;
                    x = Math.Max(0, (int)roi.X);
                    y = Math.Max(0, (int)roi.Y);
                    w = Math.Min((int)roi.Width, gray.Width - x);
                    h = Math.Min((int)roi.Height, gray.Height - y);
                    if (w <= 0 || h <= 0)   //260327 hbk — ROI 미설정(0,0,0,0)이면 NG 반환 (티칭 필수)
                    {
                        annotated = null;
                        return false;
                    }
                    roiMat = new Mat(gray, new OpenCvSharp.Rect(x, y, w, h)).Clone(); //260402 hbk Clone 추가 (gray 독립 + finally에서 안전 Dispose)
                }

                // 스무딩 — 노이즈 제거 후 이진화   //260327 hbk
                Mat smoothed = new Mat();
                Cv2.GaussianBlur(roiMat, smoothed, new OpenCvSharp.Size(5, 5), 0);   //260327 hbk — 5x5 Gaussian, 노이즈 제거

                // ROI 내 이진화 (BlobThreshold 적용)   //260327 hbk
                Mat threshed = new Mat();
                Cv2.Threshold(smoothed, threshed, param.BlobThreshold, 255, ThresholdTypes.Binary);   //260327 hbk 밝은 자재 흰색(255), 배경 검정(0)
                smoothed.Dispose();

                // Circle 모드: 원 외부 마스킹 (바운딩 박스에서 원 영역만 남김)   //260327 hbk 그리기
                roiMat.Dispose(); //260402 hbk roiMat 즉시 해제 (Rectangle/Circle 공통)
                roiMat = null; //260402 hbk finally 중복 Dispose 방지

                if (isCircleMode)
                {
                    Mat circleMask = Mat.Zeros(threshed.Size(), MatType.CV_8UC1);
                    OpenCvSharp.Point localCenter = new OpenCvSharp.Point(
                        circleCenter.X - x, circleCenter.Y - y);   //260327 hbk 그리기 — 바운딩 박스 기준 좌표
                    Cv2.Circle(circleMask, localCenter, circleRadius, new Scalar(255), -1);   //260327 hbk 그리기
                    Mat maskedThreshed = new Mat();
                    threshed.CopyTo(maskedThreshed, circleMask);   //260327 hbk 그리기
                    circleMask.Dispose();
                    threshed.Dispose();
                    threshed = maskedThreshed;   //260327 hbk 그리기
                }

                // connection — FindContours (Halcon connection 대응)   //260327 hbk
                OpenCvSharp.Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(threshed, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                // select_shape(area, MinArea, MaxArea) — 면적 필터   //260327 hbk
                var selected = new System.Collections.Generic.List<(OpenCvSharp.Point center, double area)>();
                foreach (var contour in contours)
                {
                    double area = Cv2.ContourArea(contour);
                    if (area < param.BlobMinArea || area > param.BlobMaxArea) continue;

                    // 중심 계산 (Moments)   //260327 hbk
                    var M = Cv2.Moments(contour);
                    if (M.M00 == 0) continue;
                    int cx = (int)(M.M10 / M.M00) + x;   // ROI 오프셋 적용하여 전체 이미지 좌표로 변환
                    int cy = (int)(M.M01 / M.M00) + y;
                    selected.Add((new OpenCvSharp.Point(cx, cy), area));
                }

                // 오버레이용 컬러 Mat 생성   //260326 hbk
                annotated = new Mat();
                if (image.Channels() == 1)
                    Cv2.CvtColor(image, annotated, ColorConversionCodes.GRAY2BGR);
                else
                    annotated = image.Clone();

                bool isOk = (selected.Count >= 1);   //260327 hbk — 자재 있으면 OK
                param.LastBlobArea = selected.Count > 0 ? selected.Max(s => s.area) : 0;   //260331 hbk — 최대 면적 저장
                Scalar blobColor = isOk ? new Scalar(0, 255, 0) : new Scalar(0, 0, 255);

                foreach (var (center, area) in selected)
                {
                    int blobRadius = (int)Math.Sqrt(area / Math.PI);   //260327 hbk — 면적 기반 반지름
                    blobRadius = Math.Max(10, blobRadius);
                    Cv2.Circle(annotated, center, blobRadius, blobColor, 2);   //260327 hbk — blob 외곽 원
                    Cv2.Circle(annotated, center, 3, blobColor, -1);           //260327 hbk — 중심점
                    Cv2.PutText(annotated, $"Area:{area:F0}",
                        new OpenCvSharp.Point(center.X + blobRadius + 3, center.Y),
                        HersheyFonts.HersheySimplex, 0.5, blobColor, 1);   //260327 hbk — 면적 표시
                }

                // ROI 경계 표시   //260326 hbk
                if (isCircleMode)
                    Cv2.Circle(annotated, circleCenter, circleRadius, new Scalar(255, 255, 0), 1);
                else
                    Cv2.Rectangle(annotated,
                        new OpenCvSharp.Point(x, y),
                        new OpenCvSharp.Point(x + w, y + h),
                        new Scalar(255, 255, 0), 1);

                threshed.Dispose();
                return isOk;

            }
            catch (Exception ex)
            {
                Logging.PrintLog((int)ELogType.Error, string.Format("BlobDetect Error: {0}", ex.Message));   //260326 hbk
                annotated = null;
                return false;   //260326 hbk
            }
            finally
            {
                //260402 hbk 예외 발생 시에도 Mat 해제 보장 (메모리 누수 방지)
                if (needDisposeGray && gray != null) gray.Dispose();
                if (roiMat != null) roiMat.Dispose();
            }
        }

        //260403 hbk -- ImageFolderManager-based save: original + annotated pair (D-03, D-04, D-06, D-07)
        private void SaveResultImage(Mat image, bool isOK)
        {
            if (image == null) return;   //260403 hbk

            var setting = SystemSetting.Handle;
            if (isOK && !setting.SaveOkImage) return;    //260326 hbk
            if (!isOK && !setting.SaveNgImage) return;   //260326 hbk

            if (string.IsNullOrEmpty(_FolderPath)) return;   //260403 hbk -- guard: no folder path

            try
            {
                // original image -- async save with Clone (D-03)   //260403 hbk
                string origPath = ImageFolderManager.GetSavePath(_FolderPath, Name, isOK);   //260403 hbk
                Mat imgClone = image.Clone();   //260403 hbk -- Clone으로 비동기 저장 시 원본 생명주기 독립
                Task.Factory.StartNew((obj) =>
                {
                    var mat = obj as Mat;   //260403 hbk
                    try { mat.SaveImage(origPath); }   //260403 hbk
                    catch (Exception ex) { Logging.PrintLog((int)ELogType.Error, string.Format("SaveImage Error: {0}", ex.Message)); }   //260403 hbk
                    finally { mat.Dispose(); }   //260403 hbk -- Clone 해제
                }, imgClone);

                // annotated image -- async save with Clone (D-04, D-06)   //260403 hbk
                Mat annotated = _MyParam.LastAnnotatedImage;   //260403 hbk
                if (annotated != null && !annotated.IsDisposed)   //260403 hbk -- null guard for SIMUL mode
                {
                    string annotatedPath = ImageFolderManager.GetAnnotatedSavePath(_FolderPath, Name, isOK);   //260403 hbk
                    Mat annotatedClone = annotated.Clone();   //260403 hbk -- Clone으로 비동기 저장
                    Task.Factory.StartNew((obj) =>
                    {
                        var mat = obj as Mat;   //260403 hbk
                        try { mat.SaveImage(annotatedPath); }   //260403 hbk
                        catch (Exception ex) { Logging.PrintLog((int)ELogType.Error, string.Format("SaveAnnotatedImage Error: {0}", ex.Message)); }   //260403 hbk
                        finally { mat.Dispose(); }   //260403 hbk -- Clone 해제
                    }, annotatedClone);
                }
            }
            catch (Exception ex)
            {
                Logging.PrintLog((int)ELogType.Error, string.Format("SaveImage Error: {0}", ex.Message));   //260403 hbk
            }
        }

        #endregion

        #region constructors
        public Action_Inspection(EAction id, string name, int shotIndex) : base(id, name)   //260326 hbk — 생성자: EAction ID, Action 이름, Shot 인덱스
        {
            Context  = new InspectionActionContext(this);                           //260326 hbk — Action Context 생성
            Param    = new InspectionParam(this, shotIndex);                        //260326 hbk — Shot별 파라미터 생성
            _MyParam = Param as InspectionParam;                                    //260326 hbk — 타입 캐스트 캐시
            _MyParam.DeviceName     = DeviceHandler.INSPECTION_CAMERA;             // 레시피 미로드 시 DeviceName null 방지
            _MyParam.LightGroupName = LightHandler.LIGHT_DEFAULT;                  // 기본 조명 그룹 초기화
        }
        #endregion
    }
}
