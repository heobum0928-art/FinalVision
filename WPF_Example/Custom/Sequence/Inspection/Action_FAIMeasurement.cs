using System;
using OpenCvSharp;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Setting;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;

//260401 hbk — Action_FAIMeasurement: Shot-FAI 구조 측정 Action (Action_Inspection 대체)
//             Grab(Shot 공유) => Measure(Phase 8 구현) => SaveImage => End
namespace FinalVisionProject.Sequence
{
    public class Action_FAIMeasurement : ActionBase
    {
        #region fields
        private VirtualCamera _Camera;              //260401 hbk
        private ShotConfig _Shot;                   //260401 hbk — 소속 Shot 설정
        private FAIConfig _FAI;                     //260401 hbk — 측정 FAI 설정
        private bool _IsOK;                         //260401 hbk — 판정 결과

        public enum EStep                           //260401 hbk
        {
            Grab       = 0,   // Shot 이미지 촬상 (같은 Shot이면 재사용)
            Measure    = 1,   // 에지 측정 (Phase 8 상세 구현, 현재 stub)
            SaveImage  = 2,   // 결과 이미지 저장
            End        = 3,   // 완료
        }
        #endregion

        #region properties
        public ShotConfig Shot => _Shot;    //260401 hbk
        public FAIConfig FAI => _FAI;       //260401 hbk
        #endregion

        #region constructors
        public Action_FAIMeasurement(EAction id, string name, ShotConfig shot, FAIConfig fai)
            : base(id, name)   //260401 hbk
        {
            _Shot = shot;
            _FAI = fai;
            Context = new InspectionActionContext(this);
            Param = shot;   // ParamBase = ShotConfig (카메라 파라미터 포함)
        }
        #endregion

        #region methods — lifecycle

        public override void OnLoad()   //260401 hbk
        {
            _Camera = SystemHandler.Handle.Devices[_Shot.DeviceName];
            if (_Camera != null)
            {
                if (_Camera.Properties == null)
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Not Open!",
                        "Camera is not open. Please check your connection status.",
                        System.Windows.MessageBoxImage.Error);
                    return;
                }
                if (!_Camera.Properties.ApplyFromParam(_Shot))
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Property Set Fail!",
                        "Check camera settings. or camera state.",
                        System.Windows.MessageBoxImage.Error);
                }
                if (!_Camera.SetSoftwareTriggerMode())
                {
                    CustomMessageBox.Show(_Camera.Name + " Camera Software trigger mode Set Fail!",
                        "Check camera settings. or camera state.",
                        System.Windows.MessageBoxImage.Error);
                }
            }
            base.OnLoad();
        }

        public override void OnBegin(SequenceContext prevResult = null)   //260401 hbk
        {
            _FAI.ClearResult();
            base.OnBegin(prevResult);
        }

        public override ActionContext Run()   //260401 hbk
        {
            switch ((EStep)Step)
            {
                case EStep.Grab:   //260401 hbk — Shot 이미지 촬상 (같은 Shot 이미지 재사용)
                    if (!_Shot.HasImage)
                    {
                        if (_Shot.DelayMs > 0)
                            System.Threading.Thread.Sleep(_Shot.DelayMs);

                        Mat grabbed = _Camera.GrabImage();
                        _Shot.SetImage(grabbed);
                        grabbed?.Dispose();

                        if (!_Shot.HasImage)
                        {
                            Logging.PrintLog((int)ELogType.Trace,
                                "[FAI] {0} Shot_{1} Grab 실패 NG", Name, _Shot.ShotIndex);
                            _IsOK = false;
                            Step = (int)EStep.SaveImage;
                            break;
                        }
                        Logging.PrintLog((int)ELogType.Trace,
                            "[FAI] {0} Shot_{1} Grab OK", Name, _Shot.ShotIndex);
                    }
                    Step++;
                    break;

                case EStep.Measure:   //260401 hbk — 에지 측정 (Phase 8에서 Halcon 구현, 현재 stub)
                    _IsOK = RunMeasurement();
                    Logging.PrintLog((int)ELogType.Trace,
                        "[FAI] {0} Measure Result:{1}", Name, _IsOK ? "OK" : "NG");
                    Step++;
                    break;

                case EStep.SaveImage:   //260401 hbk
                    SaveResultImage(_IsOK);
                    Step++;
                    break;

                case EStep.End:   //260401 hbk
                    Logging.PrintLog((int)ELogType.Trace,
                        "[FAI] {0} End, Result:{1}", Name, _IsOK ? "Pass" : "Fail");
                    FinishAction(_IsOK ? EContextResult.Pass : EContextResult.Fail);
                    break;
            }
            return base.Run();
        }

        #endregion

        #region methods — measurement

        /// <summary>
        /// 에지 간 거리 측정. Phase 8에서 Halcon MeasureHandle로 교체 예정.
        /// 현재: stub — 항상 Pass 반환.
        /// </summary>
        private bool RunMeasurement()   //260401 hbk
        {
            using (Mat shotImage = _Shot.GetImageClone())
            {
                if (shotImage == null) return false;

                // TODO Phase 8: Halcon 에지 측정 구현
                // 1. ROI(_FAI.ROI) 영역 추출
                // 2. HMeasure 생성 => MeasurePairs
                // 3. 에지 간 거리 계산
                // 4. _FAI.SetResult(distance) 호출

                // Stub: 측정값 0, 공차 판정
                _FAI.SetResult(0);
                return _FAI.IsPass;
            }
        }

        #endregion

        #region methods — image save

        private void SaveResultImage(bool isOK)   //260401 hbk
        {
            var setting = SystemSetting.Handle;
            if (isOK && !setting.SaveGoodImage) return;   //260403 hbk -- SaveOkImage => SaveGoodImage
            if (!isOK && !setting.SaveNGImage) return;    //260403 hbk -- SaveNgImage => SaveNGImage

            try
            {
                using (Mat img = _Shot.GetImageClone())
                {
                    if (img == null) return;

                    string dateDir = DateTime.Now.ToString("yyyyMMdd");
                    string timeStr = DateTime.Now.ToString("HHmmss_fff");
                    string resultStr = isOK ? "OK" : "NG";
                    string dir = System.IO.Path.Combine(@"D:\Log", dateDir);
                    System.IO.Directory.CreateDirectory(dir);
                    string filePath = System.IO.Path.Combine(dir,
                        $"{_FAI.FAIName}_{resultStr}_{timeStr}.jpg");
                    img.SaveImage(filePath);
                }
            }
            catch (Exception ex)
            {
                Logging.PrintLog((int)ELogType.Error, "SaveImage Error: {0}", ex.Message);
            }
        }

        #endregion
    }
}
