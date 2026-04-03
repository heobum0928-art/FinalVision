using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Network;
using FinalVisionProject.UI;
using FinalVisionProject.Utility;   //260403 hbk -- ImageFolderManager 참조

//260326 hbk — Sequence_Inspection: CornerAlignSequence 대체
namespace FinalVisionProject.Sequence
{
    public class InspectionSequenceContext : SequenceContext   //260326 hbk
    {
        #region properties

        public string CurrentFolderPath { get; set; } = "";   //260403 hbk -- inspection time-folder path (D-09)

        #endregion

        #region methods

        public override void Clear()
        {
            base.Clear();
            CurrentFolderPath = ImageFolderManager.BeginInspection();   //260403 hbk -- create time-folder at inspection start (D-02, D-09)
        }

        public override void CopyFrom(ActionContext actionContext)
        {
            base.CopyFrom(actionContext);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void RenderResult(DrawingContext dc)
        {
            base.RenderResult(dc);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

        #region constructors
        public InspectionSequenceContext(SequenceBase source) : base(source)   //260326 hbk
        {
        }
        #endregion
    }

    public class Sequence_Inspection : SequenceBase   //260326 hbk
    {
        #region fields
        private DeviceHandler pDevs;
        private VirtualCamera pCam;

        private SequenceContext _MyContext;
        private CameraMasterParam _MyParam;

        private readonly string DefaultCamera;
        private readonly string DefaultLight;
        #endregion

        #region methods

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void OnCreate()   //260326 hbk
        {
            _MyParam.LightGroupName = DefaultLight;
            _MyParam.DeviceName = DefaultCamera;

            pCam = pDevs[_MyParam.DeviceName];

            if (pCam == null)
            {
                CustomMessageBox.Show("Error", string.Format("Camera {0} - Initialize Fail", _MyParam.DeviceName), System.Windows.MessageBoxImage.Error);
                IsInitialized = false;
                Context.State = EContextState.Error;
                return;
            }
            if (pCam.Properties == null)
            {
                CustomMessageBox.Show("Error", string.Format("Camera Property {0} - Initialize Fail", _MyParam.DeviceName), System.Windows.MessageBoxImage.Error);
                IsInitialized = false;
                Context.State = EContextState.Error;
                return;
            }

            IsInitialized = true;

            base.OnCreate();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        public override void OnLoad()   //260326 hbk
        {
            if (!SystemHandler.Handle.Lights.ApplyLight(_MyParam))
            {
            }
            base.OnLoad();
        }

        protected override void AddResponse()   //260327 hbk — 검사 완료 후 TCP 결과 패킷 Enqueue
        {
            base.AddResponse();

            TestResultPacket packet = new TestResultPacket();
            packet.Target         = RequestPacket.Sender;
            packet.Site           = RequestPacket.Site;
            packet.InspectionType = RequestPacket.TestType;
            packet.Result         = (Context.Result == EContextResult.Pass)
                                    ? EVisionResultType.OK
                                    : EVisionResultType.NG;
            ResponseQueue.Enqueue(packet);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

        #region constructors
        public Sequence_Inspection(ESequence id, string name, string defaultCamera, string defaultLight) : base(id, name)   //260326 hbk
        {
            pDevs = SystemHandler.Handle.Devices;

            Context = new InspectionSequenceContext(this);   //260326 hbk
            _MyContext = Context as InspectionSequenceContext;

            Param = new CameraMasterParam(this);
            _MyParam = Param as CameraMasterParam;

            DefaultLight = defaultLight;
            DefaultCamera = defaultCamera;
        }
        #endregion
    }
}
