using System;
using System.Collections.Generic;
using System.Windows;
using OpenCvSharp;
using PropertyTools.DataAnnotations;
using FinalVisionProject.Device;
using FinalVisionProject.Sequence;

//260401 hbk — ShotConfig: 카메라 위치(Shot) 단위 파라미터 + FAI 목록
namespace FinalVisionProject.Sequence
{
    public class ShotConfig : CameraSlaveParam
    {
        #region fields
        public readonly int ShotIndex;   //260401 hbk — Shot 순번 (0-based)
        private readonly object _imageLock = new object();   //260401 hbk
        #endregion

        #region constructors
        public ShotConfig(object owner, int shotIndex) : base(owner)   //260401 hbk
        {
            ShotIndex = shotIndex;
            ShotName = $"Shot_{shotIndex + 1}";
        }
        #endregion

        #region properties — 식별

        [Category("Shot")]
        [PropertyTools.DataAnnotations.ReadOnly(true)]
        public string ShotName { get; set; }   //260401 hbk — "Shot_1" 등 표시 이름

        #endregion

        #region properties — Z축 위치

        [Category("Position")]
        public double ZPosition { get; set; }   //260401 hbk — Z축 이동 위치

        #endregion

        #region properties — 촬상

        [Category("Grab")]
        public int DelayMs { get; set; } = 0;   //260401 hbk — Grab 전 대기 시간 (ms)

        [Category("Grab")]
        public string SimulImagePath { get; set; } = "";   //260401 hbk — SIMUL 모드 테스트 이미지 경로

        #endregion

        #region properties — FAI 목록 (런타임, INI에서 별도 관리)

        [PropertyTools.DataAnnotations.Browsable(false)]
        public List<FAIConfig> FAIs { get; set; } = new List<FAIConfig>();   //260401 hbk

        #endregion

        #region properties — 이미지 버퍼 (런타임)

        [PropertyTools.DataAnnotations.Browsable(false)]
        public Mat LastImage { get; private set; }   //260401 hbk — 마지막 Grab 이미지

        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool HasImage => LastImage != null && !LastImage.Empty();   //260401 hbk

        #endregion

        #region methods — 이미지 관리

        public void SetImage(Mat img)   //260401 hbk — Grab 이미지 저장 (clone)
        {
            lock (_imageLock)
            {
                LastImage?.Dispose();
                LastImage = img?.Clone();
            }
        }

        public Mat GetImageClone()   //260401 hbk — 이미지 복사본 반환 (스레드 안전)
        {
            lock (_imageLock)
            {
                return LastImage?.Clone();
            }
        }

        public void ClearImage()   //260401 hbk
        {
            lock (_imageLock)
            {
                LastImage?.Dispose();
                LastImage = null;
            }
        }

        #endregion

        #region methods — FAI 관리

        public FAIConfig AddFAI(string faiName)   //260401 hbk — FAI 추가
        {
            int index = FAIs.Count;
            var fai = new FAIConfig(Owner, index)
            {
                FAIName = faiName,
                ShotIndex = ShotIndex
            };
            FAIs.Add(fai);
            return fai;
        }

        public void RemoveFAI(int faiIndex)   //260401 hbk — FAI 제거 + 인덱스 재정렬
        {
            if (faiIndex < 0 || faiIndex >= FAIs.Count) return;
            FAIs.RemoveAt(faiIndex);
        }

        public void ClearAllResults()   //260401 hbk — 모든 FAI 결과 초기화
        {
            foreach (var fai in FAIs)
                fai.ClearResult();
        }

        #endregion

        #region methods — CopyTo

        public override bool CopyTo(ParamBase param)   //260401 hbk
        {
            if (!(param is ShotConfig target)) return base.CopyTo(param);

            bool result = base.CopyTo(param);
            target.ShotName = this.ShotName;
            target.ZPosition = this.ZPosition;
            target.DelayMs = this.DelayMs;
            target.SimulImagePath = this.SimulImagePath;
            return result;
        }

        #endregion
    }
}
