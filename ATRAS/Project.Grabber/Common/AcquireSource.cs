using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class AcquireSource : NotifySingleton<AcquireSource>, IDisposable
    {
        #region fields
        protected event GrabberOpened grabberOpenedEvent;
        protected event GrabberClosed grabberClosedEvent;

        protected event PreGrabEvent preGrabEvent;

        protected event BufferGrabDoneEvent bufferGrabDoneEvent;

        protected event GrabCompleteEvent grabCompleteEvent;


        protected GrabberBase _GrabberBase;

        protected ConcurrentDictionary<int, List<ByteImage>> _frame_image_dic;
        protected ConcurrentDictionary<int, ByteImage>  _grab_image_dic;
        #endregion

        #region propertise
        public CameraInfoBase[] CameraInfo
        {
            get
            {
                if (_GrabberBase != null)
                {
                    return _GrabberBase.CameraInfos;
                }

                return null;
            }
        }
        public int CameraCount
        {
            get
            {
                if (_GrabberBase != null)
                {
                    return _GrabberBase.CameraInfos.Length;
                }

                return 0;
            }
        }
        public int [] FrameBufferCount
        {
            get { return CameraInfo.Select(s => s.FrameBufferCount).ToArray(); }
        }
        public ConcurrentDictionary<int, List<ByteImage>> FrameImages
        {
            get { return _frame_image_dic; }
        }
        public ConcurrentDictionary<int, ByteImage> GrabImage
        {
            get { return _grab_image_dic; }
        }
        #endregion

        #region methods
        protected void OnGrabberOpened()
        {
            if (grabberOpenedEvent != null)
                grabberOpenedEvent();
        }
        protected void OnGrabberClosed()
        {
            if (grabberClosedEvent != null)
                grabberClosedEvent();
        }
        protected void OnPreGrab(int camera_index, GrabTypes grab_type)
        {            
            _frame_image_dic[camera_index] = new List<ByteImage>();

            int frame_count = FrameBufferCount[camera_index];

            int width = CameraInfo[camera_index].Width;
            int height = CameraInfo[camera_index].Height;

            ByteImage image = new ByteImage(width, height * frame_count);

            _grab_image_dic[camera_index] = image;

            AppLogger.Info()("[{0}] camera's frame({1} count), grab image([{2}, {3}]) created.",
                            camera_index, frame_count, width, height * frame_count);
        }
        protected void OnBufferGrabedDone(int camera_index, int buffer_index, ByteImage image)
        {
            _frame_image_dic[camera_index].Add(image);

            //RoiRectangle roi = new RoiRectangle(image.Height * buffer_index, 0, image.Height * (buffer_index + 1), image.Width);
            //_grab_image_dic[camera_index].InsertChildBuffer(roi, image.Data);

            //AppLogger.Info()("[{0}] camera {1} buffer grab done.", camera_index, buffer_index);
        }


        // 참고하여 하위에서 적용할것

        protected void OnGrabComplete(int camera_index)
        {
            if (_frame_image_dic.ContainsKey(camera_index))
            {
                var list = _frame_image_dic[camera_index];
                for (int i = 0; i < list.Count; i++)
                {
                    var image = list[i];
                    RoiRectangle roi = new RoiRectangle(image.Height * i, 0, image.Height * (i + 1), image.Width);
                    _grab_image_dic[camera_index].InsertChildBuffer(roi, image.Data);
                }
            }
            AppLogger.Info()("[{0}] camera grab complete.", camera_index);
        }
        public bool GrabOnce(int camera_index)
        {
            return _GrabberBase.GrabOnce(camera_index);
        }
        public bool GrabStart(int camera_index)
        {
            return _GrabberBase.GrabStart(camera_index);
        }
        public bool GrabStop(int camera_index)
        {
            return _GrabberBase.GrabStop(camera_index);
        }
        public void RegisterEvent(GrabberOpened opened,
                            GrabberClosed closed,
                            PreGrabEvent preGrab,
                            BufferGrabDoneEvent buffer_grab_done,
                            GrabCompleteEvent grabComplete)
        {
            if (opened != null)
                this.grabberOpenedEvent += opened;

            if (closed != null)
                this.grabberClosedEvent += closed;

            if (preGrab != null)
                this.preGrabEvent += preGrab;

            if (buffer_grab_done != null)
                this.bufferGrabDoneEvent += buffer_grab_done;

            if (grabComplete != null)
                this.grabCompleteEvent += grabComplete;
        }
        protected bool CreateGrabber(GrabberTypes grabber_type)
        {
            if (_GrabberBase != null)
            {
                if (!_GrabberBase.Close())
                {
                    AppLogger.Error()(string.Format("[{0}] grabber close is failed.", _GrabberBase.Name));
                    return false;
                }
            }

            if (grabber_type == GrabberTypes.HIK)
            {
                _GrabberBase = new HIKGrabber();
            }
            else if (grabber_type == GrabberTypes.Matrox_PCI)
            {
                _GrabberBase = new Matrox_PCI();
            }
            else if(grabber_type == GrabberTypes.Matrox_GigE)
            {
                _GrabberBase = new Matrox_GigE();
            }
            else if(grabber_type == GrabberTypes.Vieworks)
            {
                _GrabberBase = new VieworksGrabber();
            }
            else // GrabberTypes.Simulator
            {
                _GrabberBase = new GrabberSimulator();
            }

            _GrabberBase.RegisterEvent(preGrabEvent, bufferGrabDoneEvent, grabCompleteEvent);

            AppLogger.Info()(string.Format("[{0}] Grabber Created.", _GrabberBase.Name));
            return true;
        }

        public bool Initialize(GrabberTypes grabber_type, List<CameraInfoBase> cameraInfos)
        {
            if (!CreateGrabber(grabber_type))
            {
                AppLogger.Error()(string.Format("[{0}] grabber create failed.", grabber_type.ToString()));
                return false;
            }

            if (!_GrabberBase.SetCameraInformation(cameraInfos))
            {
                AppLogger.Error()(string.Format("[{0}] grabber set camera information failed.", grabber_type.ToString()));
                return false;
            }

            if (!_GrabberBase.Initialize())
            {
                AppLogger.Error()(string.Format("[{0}] grabber initialize failed.", grabber_type.ToString()));
                return false;
            }

            OnGrabberOpened();

            AppLogger.Info()("Acquire source initialize success.");

            return true;
        }
        public void Dispose()
        {
            OnGrabberClosed();
        }






        #endregion

        #region constructors
        protected AcquireSource() 
        {
            grabberOpenedEvent = null;
            grabberClosedEvent = null;

            _GrabberBase = null;

            preGrabEvent = OnPreGrab;
            bufferGrabDoneEvent = OnBufferGrabedDone;

            //grabCompleteEvent = OnGrabComplete;

            _frame_image_dic = new ConcurrentDictionary<int, List<ByteImage>>();
            _grab_image_dic = new ConcurrentDictionary<int, ByteImage>();

        }
        #endregion
    }
}
