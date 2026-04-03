using OpenCvSharp;
using FinalVisionProject.Define;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FinalVisionProject.Sequence {

#region Enums
    public enum EContextResult {
        None, //no execute
        Pass,
        Fail,
        Error,
        WaferAngleError,
        WaferDieTeaching,   // 03.20 insert
    }

    public enum EContextState {
        Idle,
        Paused,
        Running,
        Finish,
        Error,
    }
#endregion

    public class ActionContext {
        public ActionBase Source { get; private set; }

        public Mat ResultImage { get; set; }
        
        public EContextState State { get; set; }

        public EContextResult Result { get; set; }

        public int CurrentStep { get; set; }

        public Stopwatch Timer { get; private set; } = new Stopwatch();

        public ActionContext(ActionBase source) {
            Source = source;
            //ResultImage = new Mat();
        }

        public virtual void Clear() {
            State = EContextState.Idle;
            Result = EContextResult.None;
            CurrentStep = 0;
        }

        public string GetStateString {
            get {
                return Enum.GetName(typeof(EContextState), State);
            }
        }

        public string GetResultString {
            get {
                return Enum.GetName(typeof(EContextResult), Result);
            }
        }

        public void CopyFrom(SequenceContext seqContext) {
            if (seqContext == null) return;
            if (seqContext.ResultImage != null) {
                if (ResultImage == null) ResultImage = new Mat();
                else if (ResultImage.IsDisposed) ResultImage = new Mat();

                seqContext.ResultImage.CopyTo(ResultImage);
            }
        }
    }
    

    public class SequenceContext {
        public SequenceBase Source { get; private set; }
        
        public ParamBase ActionParam { get; set; }

        public Mat ResultImage { get; set; }

        public string TargetCode { get; set; }

        public string ResultImageFileName { get; set; }

        private EContextState _State;

        public EContextState State {
            get {
                if(Source != null) {
                    if (Source.IsInitialized == false) return EContextState.Error;
                }
                return _State;
            }
            set {
                _State = value; //260402 hbk State setter 버그 수정 (State→value)
            }
        }
        
        public EContextResult Result { get; set; }

        public string ResultStr { get; set; }

        public Stopwatch Timer { get; private set; } = new Stopwatch();

        //protected static Brush OkColor = Brushes.Lime;
        protected static Brush OkColor = Brushes.Blue;
        protected static Brush NgColor = Brushes.Red;
        protected static Pen OkPen = new Pen(OkColor, 3);
        protected static Pen NgPen = new Pen(NgColor, 3);
        protected static Typeface TextDrawFont = new Typeface("맑은 고딕");
        protected static int FontSize = 60;

        public SequenceContext(SequenceBase source) {   //260326 hbk — 불필요한 CornerAlignSequenceContext 오버로드 제거
            Source = source;
        }

        public virtual void RenderResult(DrawingContext dc) {

        }
        
        public string StateString {
            get {
                return Enum.GetName(typeof(EContextState), State);
            }
        }

        public string ResultString {
            get {
                return Enum.GetName(typeof(EContextResult), Result);
            }
        }

        //sequence를 clear하면 소속된 action 모두 clear 시킨다.
        public virtual void Clear() {
            for(int i = 0; i < Source.ActionCount; i++) {
                ActionBase act = Source.GetAction(i);
                act.Context.Clear();
            }
            //현재 시작하는 index만 clear 시킬 것인지?
            //Source.CurrentActionIndex
            ResultImageFileName = "";
            ResultImage = null;   //260331 hbk — 이전 실행의 stale 이미지 방지

            Timer.Restart();
            State = EContextState.Idle;
            Result = EContextResult.None;
        }

        public virtual void CopyFrom(ActionContext actionContext) {
            if ((actionContext.ResultImage == null) || actionContext.ResultImage.IsDisposed) return;
            if (ResultImage == null) ResultImage = new Mat();
            //if((ResultImage.Width != actionContext.ResultImage.Width) || (ResultImage.Height != actionContext.ResultImage.Height))  

            actionContext.ResultImage.CopyTo(ResultImage);
            
        }
    }
}
