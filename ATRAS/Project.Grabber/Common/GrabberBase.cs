using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public abstract class GrabberBase
    {
        #region fields
        protected PreGrabEvent preGrabEvent;
        protected BufferGrabDoneEvent bufferGrabDoneEvent;
        protected GrabCompleteEvent grabCompleteEvent;

        protected ILogger logger;

        private const int TIMEOUT = 10000;

        protected SystemTypes systemType;

        protected bool initialized;

        //protected List<CameraInfo> camera_infos;
        protected ConcurrentDictionary<int, CameraInfoBase> camera_infos;
        
        protected GrabberTypes grabberType;



        #endregion

        #region propertise
        public int CameraCount
        {
            get { return camera_infos.Count; }
        }
        public CameraInfoBase[] CameraInfos
        {
            get 
            {
                //return camera_infos.ToArray(); 
                return camera_infos.Values.ToArray();
            }
        }
        public GrabberTypes GrabberType
        {
            get { return grabberType; }

            set { grabberType = value; }

        }
        public string Name
        {
            get { return grabberType.ToString(); }
        }
        public bool Initialized
        {
            get { return initialized; }
        }
        #endregion

        #region methods
        public virtual void RegisterEvent(
            PreGrabEvent preGrab,
            BufferGrabDoneEvent buffer_grab_done,
            GrabCompleteEvent grabComplete)
        {
            this.preGrabEvent += preGrab;
            this.bufferGrabDoneEvent += buffer_grab_done;
            this.grabCompleteEvent += grabComplete;
        }

        public void ClearRegisterEvent()
        {
            preGrabEvent = null;
            bufferGrabDoneEvent = null;
            grabCompleteEvent = null;
        }
        public void OnPreGrabed(int camera_index, GrabTypes grab_type)
        {
            if(preGrabEvent != null)
            {
                preGrabEvent(camera_index, grab_type);
            }
        }
        public void OnBufferGrabDone(int camera_index, int buffer_index, ByteImage image)
        {
            if(bufferGrabDoneEvent != null)
            {
                bufferGrabDoneEvent(camera_index, buffer_index, image);
            }
        }
        public void OnGrabComplete(int camera_index)
        {
            if (grabCompleteEvent != null)
            {
                grabCompleteEvent(camera_index);
            }
        }
        public virtual bool SetCameraInformation(List<CameraInfoBase> camera_infos)
        {
            if(camera_infos == null || camera_infos.Count == 0)
            {
                logger.Error()("Camera information can't be null or 0.");
                return false;
            }
            //this.camera_infos = camera_infos;
            this.camera_infos = new ConcurrentDictionary<int, CameraInfoBase>();

            for(int i = 0; i < camera_infos.Count; i++)
            {
                this.camera_infos[i] = camera_infos[i];
            }    
            return true;
        }
        public virtual bool Initialize()
        {
            if(initialized)
            {
                if(!Reset())
                {
                    logger.Error()("[{0}] grabber reset failed.", Name);
                    return false;
                }
            }

            initialized = false;
            if (camera_infos == null || camera_infos.Count == 0)
            {
                logger.Error()("[{0}] grabber initialize failed. CameraInfo is not Allocated.", Name);
                return initialized;

            }

            // Frame Buffer Alloc
            //frame_buffer_dic = new ConcurrentDictionary<int, ByteImage[]>();

            if (!Open())
            {
                logger.Error()("[{0}] grabber initialize failed.", Name);
                return initialized;
            }

            initialized = true;

            logger.Info()("[{0}] grabber initialize success!", Name);

            return initialized;
        }
        public abstract bool Open();
        public abstract bool Close();
        public abstract bool Reset();

        // 한번 촬상
        public abstract bool GrabOnce(int camera_idx);

        // GrabStop 호출 전까지 계속 촬상
        public abstract bool GrabStart(int camera_idx);
        public abstract bool GrabStop(int camera_idx);
        #endregion

        #region constructor
        public GrabberBase()
        {           
            grabberType = GrabberTypes.Simulator;

            //camera_infos = new List<CameraInfo>();
            camera_infos = new ConcurrentDictionary<int, CameraInfoBase>();

            systemType = SystemTypes.Simulation;

            initialized = false;

            preGrabEvent = null;
            bufferGrabDoneEvent = null;
        }
        public GrabberBase(GrabberTypes type)
            : this()
        {
            grabberType = type;
            logger = LogManager.GetLogger("Grabber");            
        }
        #endregion
    }
}
