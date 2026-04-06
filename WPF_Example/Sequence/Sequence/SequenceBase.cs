using FinalVisionProject.Define;
using System;
using System.Collections.Generic;
using System.Threading;
using FinalVisionProject.Setting;
using FinalVisionProject.Utility;
using FinalVisionProject.Network;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using OpenCvSharp;
using System.IO;
using FinalVisionProject.Device;
using FinalVisionProject.UI;

namespace FinalVisionProject.Sequence {
   
    #region delegates
    public delegate void EventSequenceStateChanged(SequenceContext context);
    public delegate void EventActionChanged(ActionContext context);
    #endregion

    #region enums

    public enum ESequenceCommmand {
        Stop,
        Start,
        Pause,
        Resume,
    }
    
    #endregion

    public partial class SequenceBase {
        public bool IsInitialized { get; protected set; }

        public ESequence ID { get; private set; }

        public string Name { get; private set; }

        public string TargetID { get; set; }    //Sequence target id, wafer id, barcode ....

        public int CurrentActionIndex { get; protected set; }
        public int EndActionIndex { get; protected set; }
       
        protected ActionBase [] Actions;
        protected ActionBase CurAction = null;
        
        public ParamBase Param { get; protected set; }

        public string CurActionName {
            get {
                if (CurAction == null) return EContextState.Idle.ToString();
                return CurAction.Name;
            }
        }

        private Thread MainThread;
        private bool IsTerminated = false;

        protected bool bCreated { get; private set; } = false;
        public SequenceContext Context { get; protected set; } = null;
        protected bool IsDoneBegin = false;
        public bool IsFinished { get; protected set; } = false;

        public ESequenceCommmand Command { get; protected set; }

        //inspect 요청자 정보
        public TestPacket RequestPacket { get; private set; } = null;
        public ConcurrentQueue<TestResultPacket> ResponseQueue { get; private set; } = new ConcurrentQueue<TestResultPacket>();

        public SequenceBase(ESequence id, string name) {
            ID = id;
            Name = name;

            CurrentActionIndex = 0;

            IsTerminated = false;
            MainThread = new Thread(MainExecute);
            MainThread.Priority = ThreadPriority.Highest;
            MainThread.Name = Name;
            MainThread.Start();
        }

        public SequenceBase(ESequence id, params ActionBase [] actions) {
            ID = id;
            Name = Enum.GetName(typeof(ESequence), id);
            Actions = actions;

            CurrentActionIndex = 0;

            Context = new SequenceContext(this);

            IsTerminated = false;
            MainThread = new Thread(MainExecute);
            MainThread.Priority = ThreadPriority.Normal;
            MainThread.Name = Name;
            MainThread.Start();
        }

        public void AddAction(params ActionBase[] actions) {
            Actions = actions;
            //set cam param
            Param.Parent = this;
            foreach (ActionBase act in Actions) {
                if ((Param != null) && (Param is CameraMasterParam)) {
                    CameraMasterParam masterParam = Param as CameraMasterParam; 
                    if (act.Param is CameraSlaveParam) {
                        CameraSlaveParam camParam = act.Param as CameraSlaveParam;
                        masterParam.AddChild(camParam);
                    }
                }
                act.Param.Parent = this;
            }

            
        } 
        
        public EContextState State {
            get { return Context.State; }
            private set { Context.State = value; }
        }

        public EContextResult Result {
            get { return Context.Result; }
            private set { Context.Result = value; }
        }

        public void SequenceCheck() {
            //sequence 정합성 검사
            if (Actions.Length == 0) throw new InvalidOperationException("Action list is Empty.");
            //if (Actions.First().ActionType != EActionType.Begin) throw new InvalidOperationException("First Action must be 'Begin' Type.");
            //if (Actions.Last().ActionType != EActionType.End) throw new InvalidOperationException("Last Action must be 'End' Type.");
            //int beginCount = Actions.Count(action => action.ActionType == EActionType.Begin);
            //int endCount = Actions.Count(action => action.ActionType == EActionType.End);
            //if (beginCount > 1) throw new InvalidOperationException("Begin Action must be exist one.");
            //if (endCount > 1) throw new InvalidOperationException("End Action must be exist one.");
        }

        public virtual void OnCreate() {
            if (Context == null) Context = new SequenceContext(this);

            foreach(ActionBase action in Actions) {
                action.OnCreate();
            }
            bCreated = true;
        }

        public virtual void OnRelease() {

        }

        public void Release() {
            foreach (ActionBase action in Actions) {
                action.Release();
            }

            IsTerminated = true;
            MainThread.Join(1000);
        }

        public int ActionCount { get => Actions.Length; }
 
        public ActionBase this[int index] {
            get {
                if (index >= Actions.Length) return null;
                return Actions[index];
            }
        }

        public ActionBase this[EAction id] {
            get {
                foreach (ActionBase act in Actions) {
                    if (act.ID == id) return act;
                }
                return null;
            }
        }

        public ActionBase this[string name] {
            get {
                EAction id = (EAction)Enum.Parse(typeof(EAction), name);
                return this[id];
            }
        }

        public ActionBase GetAction(int index) {
            if (index >= Actions.Length) return null;
            return Actions[index];
        }

        public int GetIndexOf(string name) {
            for (int i = 0; i < Actions.Length; i++) {
                if (Actions[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public int GetIndexOf(EAction actionID) {
            for(int i = 0; i < Actions.Length; i++) {
                if(Actions[i].ID == actionID) {
                    return i;
                }
            }
            return -1;
        }

        private void ExecuteAction(ActionBase action) {
            if (IsDoneBegin == false) {
                Context.ActionParam = action.Param;
                if (TargetID != null)
                    Context.TargetCode = TargetID;
                action.OnBegin(Context);
                IsDoneBegin = true;
            }
            
            ActionContext actionContext = action.Run();
            
            if (actionContext.Result == EContextResult.Error) {
                CurAction.OnEnd();
                IsDoneBegin = false;

                Context.CopyFrom(actionContext);
                OnActionChanged?.Invoke(actionContext);
                
                Error();
            }
            else if (actionContext.State == EContextState.Finish) {
                CurAction.OnEnd();
                IsDoneBegin = false;

                Context.CopyFrom(actionContext);
                OnActionChanged?.Invoke(actionContext);
                
                if (CurrentActionIndex >= EndActionIndex) {
                    Context.Result = actionContext.Result;
                    
                    Finish();
                }
            }
        }

        private void MainExecute() {
            while(IsTerminated == false) {
                if (bCreated == false) {
                    Thread.Sleep(1000);
                    continue;
                }

                switch (Command) {
                    case ESequenceCommmand.Stop:
                        State = EContextState.Idle;
                        break;
                    case ESequenceCommmand.Pause:
                        State = EContextState.Paused;
                        break;
                    case ESequenceCommmand.Start:
                        State = EContextState.Running;
                        CurAction = Actions[CurrentActionIndex];
                        ExecuteAction(CurAction);
                        break;
                }
                Thread.Sleep(5);
            }
        }
        public virtual void OnLoad() {
            foreach(ActionBase act in Actions) {
                act.OnLoad();
            }
        }

        /// <summary>
        /// 요청 패킷으로 sequence 수행
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Start(TestPacket packet) {
            if (State != EContextState.Idle) return false;

            RequestPacket = packet;
            //string seqName = packet.Identifier;
            string actName = packet.Identifier2;
            int actionIndex = GetIndexOf(actName);
            if(actionIndex == -1) {

                return false;
            }
            return Start(actionIndex);
        }

        /// <summary>
        /// 이벤트로 수행되는 start
        /// </summary>
        /// <param name="actionID"></param>
        /// <returns></returns>
        public bool Start(EAction actionID) {
            if (State != EContextState.Idle) return false;

            RequestPacket = null;
            //id가 없으면 첫 번째 action 수행
            if (actionID == EAction.Unknown) {
                return Start(0);
            }
            int i = GetIndexOf(actionID);
            if(i == -1) {
                return false;
            }
            return Start(i);
        }

        protected bool Start(int actionIndex = 0) {
            if (State != EContextState.Idle) return false;

            CurrentActionIndex = actionIndex;
            EndActionIndex = actionIndex;

            Context.Clear();
            Command = ESequenceCommmand.Start;
            IsFinished = false;

            string actName = (Actions != null && actionIndex < Actions.Length) ? Actions[actionIndex].Name : "?";
            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Start, Action:{1}", Name, actName);   //260330 hbk

            OnStart?.Invoke(Context);
            return true;
        }

        public bool Stop() {
            if (State == EContextState.Idle) return false;
            Context.Timer.Restart();
            Command = ESequenceCommmand.Stop;
            if(RequestPacket != null) AddResponse();

            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Stop", Name);   //260330 hbk
            OnStop?.Invoke(Context);
            return true;
        }

        //생성된 파일명을 반환한다.
        protected void SaveResultImage(string actionName) {
            if (Context.ResultImage == null) {   //260403 hbk -- SaveFailImage 삭제, null 체크만 유지
                Context.ResultImageFileName = null;
                return;
            }

            Task.Factory.StartNew((object obj) => {
                Mat resultImage = obj as Mat;
                Mat grayImage = new Mat(resultImage.Size(), MatType.CV_8UC1);
                Cv2.CvtColor(resultImage, grayImage, ColorConversionCodes.BGR2GRAY);

                string filePath = SystemHandler.Handle.Setting.GetResultImageSavePath(Name, actionName);

                //context에 지정한다.
                Context.ResultImageFileName = Path.GetFileName(filePath);

                if(grayImage.SaveImage(filePath) == false) {
                    CustomMessageBox.Show("Fail to Save Image", "Image Save Fail : " + filePath, System.Windows.MessageBoxImage.Error);
                }
                grayImage.Dispose();
                resultImage.Dispose(); //clone이므로 삭제 해도 됨.

            }, Context.ResultImage.Clone()); //원본이미지가 도중에 clear 되거나 변경될 우려가 있으므로 복사본을 저장하도록 한다.(저장 시간에 따라 발생 가능)
        }

        protected bool Error() {
            //if (State == EContextState.Idle) return false;
            Context.State = EContextState.Error;
            Context.Result = EContextResult.Error;

            IsFinished = true;

            Context.Timer.Restart();
            Command = ESequenceCommmand.Stop;
            if (RequestPacket != null) AddResponse();

            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Error, Action:{1}", Name, CurActionName);   //260330 hbk
            SaveResultImage(CurActionName);
            OnError?.Invoke(Context);

            return true;
        }

        protected bool Finish() {
            //if (State == EContextState.Idle) return false;
            Context.State = EContextState.Finish;
            IsFinished = true;

            Context.Timer.Restart();
            Command = ESequenceCommmand.Stop;
            if (RequestPacket != null) AddResponse();

            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Finish, Result:{1}", Name, Context.Result);   //260330 hbk
            if (Context.Result == EContextResult.Fail) {
                SaveResultImage(CurActionName);
            }

            OnFinish?.Invoke(Context);
            return true;
        }

        public bool Pause() {
            if (State != EContextState.Running) return false;

            if(CurAction != null) {
                CurAction.OnPaused();
                Context.Timer.Restart();
            }
            Command = ESequenceCommmand.Pause;
            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Paused", Name);   //260330 hbk
            OnPaused?.Invoke(Context);
            return true;
        }

        public bool Resume() {
            if (State != EContextState.Paused) return false;

            if(CurAction != null) {
                CurAction.OnResume();
                Context.Timer.Restart();
            }
            Command = ESequenceCommmand.Resume;
            Logging.PrintLog((int)ELogType.Trace, "[SEQ] {0} Resumed", Name);   //260330 hbk
            OnResume?.Invoke(Context);
            return true;
        }

        protected virtual void AddResponse() {
        }

        public int IsResponseReady {
            get {
                return ResponseQueue.Count;
            }
        }

        public TestResultPacket PopResponse() {
            if(IsResponseReady > 0) {
                if(ResponseQueue.TryDequeue(out TestResultPacket respInfo)) {
                    return respInfo;
                }
                return null;
            }
            return null;
        }

        public override string ToString() {
            return Name;
        }

        public event EventSequenceStateChanged OnStart;
        public event EventSequenceStateChanged OnStop;
        public event EventSequenceStateChanged OnFinish;
        public event EventSequenceStateChanged OnPaused;
        public event EventSequenceStateChanged OnResume;
        public event EventSequenceStateChanged OnError;
        public event EventActionChanged OnActionChanged;

    }
}
