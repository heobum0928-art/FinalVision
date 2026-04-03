using System;
using System.Windows;
using PropertyTools.DataAnnotations;
using FinalVisionProject.Sequence;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;

//260401 hbk — FAIConfig: Shot 내 개별 측정 영역 (에지 간 거리 측정 파라미터)
namespace FinalVisionProject.Sequence
{
    public class FAIConfig : ParamBase
    {
        #region fields
        public readonly int FAIIndex;   //260401 hbk — FAI 순번 (Shot 내 0-based)
        #endregion

        #region constructors
        public FAIConfig(object owner, int faiIndex) : base(owner)   //260401 hbk
        {
            FAIIndex = faiIndex;
        }
        #endregion

        #region properties — 식별

        [PropertyTools.DataAnnotations.Category("FAI")]
        [PropertyTools.DataAnnotations.ReadOnly(true)]
        public string FAIName { get; set; } = "";   //260401 hbk — "FAI_01" 등 사용자 정의 이름

        [PropertyTools.DataAnnotations.Browsable(false)]
        public int ShotIndex { get; set; }   //260401 hbk — 소속 Shot 인덱스

        #endregion

        #region properties — ROI

        [PropertyTools.DataAnnotations.Category("ROI")]
        [Rectangle, Converter(typeof(FinalVisionProject.UI.RectConverter))]
        public Rect ROI { get; set; }   //260401 hbk — 측정 영역 ROI

        #endregion

        #region properties — 에지 측정 파라미터 (Phase 8 상세 구현, 틀만 정의)

        [PropertyTools.DataAnnotations.Category("Measurement")]
        public int EdgeThreshold { get; set; } = 30;   //260401 hbk — 에지 검출 임계값

        [PropertyTools.DataAnnotations.Category("Measurement")]
        public double MeasureLength { get; set; } = 100;   //260401 hbk — 측정 프로파일 길이

        [PropertyTools.DataAnnotations.Category("Measurement")]
        public double MeasureSigma { get; set; } = 1.0;   //260401 hbk — 가우시안 스무딩 시그마

        #endregion

        #region properties — 공차

        [PropertyTools.DataAnnotations.Category("Tolerance")]
        public double Nominal { get; set; }   //260401 hbk — 기준값 (mm)

        [PropertyTools.DataAnnotations.Category("Tolerance")]
        public double UpperTol { get; set; }   //260401 hbk — 상한 공차

        [PropertyTools.DataAnnotations.Category("Tolerance")]
        public double LowerTol { get; set; }   //260401 hbk — 하한 공차

        #endregion

        #region properties — 결과 (런타임, INI 저장 안 함)

        [PropertyTools.DataAnnotations.Browsable(false)]
        public double MeasuredValue { get; set; }   //260401 hbk — 측정 결과 (mm)

        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool IsPass { get; set; }   //260401 hbk — 판정 결과

        [PropertyTools.DataAnnotations.Browsable(false)]
        public bool HasResult { get; set; }   //260401 hbk — 결과 존재 여부

        #endregion

        #region methods

        public void ClearResult()   //260401 hbk — 결과 초기화
        {
            MeasuredValue = 0;
            IsPass = false;
            HasResult = false;
        }

        public void SetResult(double value)   //260401 hbk — 측정 결과 설정 + 공차 판정
        {
            MeasuredValue = value;
            HasResult = true;
            double diff = value - Nominal;
            IsPass = (diff >= -Math.Abs(LowerTol)) && (diff <= Math.Abs(UpperTol));
        }

        public override bool CopyTo(ParamBase param)   //260401 hbk
        {
            if (!(param is FAIConfig target)) return false;

            target.FAIName = this.FAIName;
            target.ShotIndex = this.ShotIndex;
            target.ROI = this.ROI;
            target.EdgeThreshold = this.EdgeThreshold;
            target.MeasureLength = this.MeasureLength;
            target.MeasureSigma = this.MeasureSigma;
            target.Nominal = this.Nominal;
            target.UpperTol = this.UpperTol;
            target.LowerTol = this.LowerTol;
            return true;
        }

        #endregion
    }
}
