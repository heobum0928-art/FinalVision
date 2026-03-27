using Matrox.MatroxImagingLibrary;
using Project.BaseLib.Communication;
using Project.BaseLib.DataStructures;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class Matrox_HookData
    {
        #region fields

        protected BufferGrabDoneEvent bufferGrabDoneEvent;
        protected GrabCompleteEvent grabCompleteEvent;

        protected int buffer_index;
        protected int buffer_total_count;
        protected int camera_index;

        protected GrabTypes grab_type;



        #endregion

        #region propertise
        public int Camera_Index
        {
            get { return camera_index; }
        }

        #endregion

        #region methods

        public ByteImage MilImageToByteImage(MIL_ID milImage)
        {
            MIL_INT width = MIL.MbufInquire(milImage, MIL.M_SIZE_X);
            MIL_INT height = MIL.MbufInquire(milImage, MIL.M_SIZE_Y);

            ByteImage buffer = new ByteImage(((int)width), (int)height);

            MIL.MbufGet2d(milImage, 0, 0, width, height, buffer.Data);
            return buffer;
        }

        public void OnBufferGrabed(MIL_ID bufId)
        {
            if (bufferGrabDoneEvent != null)
            {
                var image = MilImageToByteImage(bufId);

                bufferGrabDoneEvent(camera_index, buffer_index, image);

                buffer_index++;
            }

            if (buffer_total_count == buffer_index && grab_type == GrabTypes.OnceGrab)
            {
                OnGrabComplete();
            }
        }

        protected void OnGrabComplete()
        {
            if (grabCompleteEvent != null)
            {
                grabCompleteEvent(camera_index);
            }
        }
        #endregion

        #region constructors
        private Matrox_HookData() 
        {
            this.bufferGrabDoneEvent = null;
            this.grabCompleteEvent = null;
            this.buffer_index = -1;
            this.camera_index = -1;
            this.buffer_total_count = 0;
            this.grab_type = GrabTypes.Unknown;
        }
        public Matrox_HookData(int camera_index, 
                               int buffer_total_count, 
                               GrabTypes grab_type, 
                               BufferGrabDoneEvent bufferGrabDone, 
                               GrabCompleteEvent grabComplete)
            : this()
        {
            this.bufferGrabDoneEvent = bufferGrabDone;
            this.grabCompleteEvent = grabComplete;
            this.buffer_index = 0;
            this.camera_index = camera_index;
            this.buffer_total_count = buffer_total_count;

            this.grab_type = grab_type;
        }
        #endregion
    }

    public class MatroxGrabber : GrabberBase, IDisposable
    {
        #region fields

        protected MIL_ID _MilApplication;

        protected MIL_ID [] _MilSystems;

        protected MIL_ID [] _MilDigitizers;

        protected Matrox_HookData [] hookDatas;

        protected ConcurrentDictionary<int, MIL_ID[]> _MilGrabBuffers;

        protected int _ConnectedCameraCount;

        private MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;
        //protected MatroxController[] _matroxController;
        protected ConcurrentDictionary<int, MatroxController> _matroxControllers;

        //protected SerialChannel[] _cam_controls;

        //protected const byte delemeter = 0x0d;

        //protected string recvMessage = string.Empty;
        //protected string recvData = string.Empty;

        //protected CommandDoneEvent _CommandDoneEvent = new CommandDoneEvent();

        //protected ManualResetEvent commandFinishedEvent = null;

        #endregion

        #region propertise
        public new MatroxCameraInfo [] CameraInfos
        {
            get
            {
                return camera_infos.Values.Cast<MatroxCameraInfo>().ToArray();
            }
        }
        public MIL_ID [] MilSystems
        {
            get { return _MilSystems; }
        }
        public int BoardCount
        {
            get { return CameraInfos.Select(s => s.Board_ID).Distinct().Count(); }
        }
        public int [] BoardNumbers
        {
            get { return CameraInfos.Select(s => s.Board_ID).Distinct().ToArray(); }
        }
        #endregion

        #region methods
        private static MIL_INT ProcessingFunction(MIL_INT HookType, MIL_ID HookId, IntPtr HookDataPtr)
        {
            MIL_ID MoBufID = MIL.M_NULL;
            if (!IntPtr.Zero.Equals(HookDataPtr))
            {
                GCHandle hUserData = GCHandle.FromIntPtr(HookDataPtr);
                Matrox_HookData UserData = hUserData.Target as Matrox_HookData;
                MIL.MdigGetHookInfo(HookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref MoBufID);

                UserData.OnBufferGrabed(MoBufID);
            }
            return 0;
        }
        public ByteImage MilImageToByteImage(MIL_ID milImage)
        {
            MIL_INT width = MIL.MbufInquire(milImage, MIL.M_SIZE_X);
            MIL_INT height = MIL.MbufInquire(milImage, MIL.M_SIZE_Y);

            ByteImage buffer = new ByteImage(((int)width), (int)height);

            MIL.MbufGet2d(milImage, 0, 0, width, height, buffer.Data);
            return buffer;
        }
        public MIL_ID GetSystemID(int camera_number)
        {
            var info = CameraInfos.FirstOrDefault(i => i.CamNumber == camera_number);

            if (info == null)
                return MIL.M_NULL;

            var board_number = info.Board_ID;

            return _MilSystems[board_number];
        }
        protected virtual bool Allocate() 
        {
            AppLogger.Error()("Allocate() is empty.");
            return false;
        }
        protected virtual bool ApplyParam()
        {
            AppLogger.Error()("ApplyParam() is empty.");
            return false;
        }
        public override bool Close()
        {
            Reset();

            if (_MilApplication != MIL.M_NULL)
            {
                MIL.MappFree(_MilApplication);
                _MilApplication = MIL.M_NULL;
            }

            ClearRegisterEvent();

            return true;
        }
        public void Dispose()
        {
            Close();
        }            
        public override bool GrabOnce(int camera_idx)
        {
            int buffer_count = _MilGrabBuffers[camera_idx].Length;

            try
            {
                OnPreGrabed(camera_idx, GrabTypes.OnceGrab);

                hookDatas[camera_idx] = new Matrox_HookData(camera_idx, buffer_count, GrabTypes.OnceGrab, bufferGrabDoneEvent, grabCompleteEvent);

                GCHandle hUserData = GCHandle.Alloc(hookDatas[camera_idx]);

                MIL.MdigProcess(_MilDigitizers[camera_idx],
                                _MilGrabBuffers[camera_idx],
                                buffer_count,
                                MIL.M_SEQUENCE + MIL.M_COUNT(buffer_count),
                                //MIL.M_ASYNCHRONOUS,
                                MIL.M_SYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB,
                                ProcessingFunctionPtr,
                                GCHandle.ToIntPtr(hUserData));
            }
            catch (Exception e)
            {
                logger.Error()("[{0}] camera grab start error. message : {1}", camera_idx, e.Message);
                return false; ;
            }

            logger.Info()("{0} camera, buffer_count : {1} grab once start. ", camera_infos, buffer_count);

            return true;
        }
        public override bool GrabStart(int camera_idx)
        {
            try
            {
                OnPreGrabed(camera_idx, GrabTypes.ContinusGrab);

                int buffer_count = _MilGrabBuffers[camera_idx].Length;

                hookDatas[camera_idx] = new Matrox_HookData(camera_idx, buffer_count, GrabTypes.ContinusGrab, bufferGrabDoneEvent, grabCompleteEvent);

                GCHandle hUserData = GCHandle.Alloc(hookDatas[camera_idx]);

                MIL.MdigProcess(_MilDigitizers[camera_idx],
                            _MilGrabBuffers[camera_idx],
                            buffer_count,
                            MIL.M_START,
                            //MIL.M_ASYNCHRONOUS,
                            MIL.M_SYNCHRONOUS + MIL.M_TRIGGER_FOR_FIRST_GRAB,
                            ProcessingFunctionPtr,
                            GCHandle.ToIntPtr(hUserData));

            }
            catch (Exception e)
            {
                logger.Error()("[{0}] camera grab start error. message : {1}", camera_idx, e.Message);
                return false;
            }

            logger.Info()("{0} camera grab start.", camera_idx);

            return true;
        }
        public override bool GrabStop(int camera_idx)
        {
            try
            {
                int buffer_count = _MilGrabBuffers[camera_idx].Length;

                GCHandle hUserData = GCHandle.Alloc(hookDatas[camera_idx]);

                MIL.MdigProcess(_MilDigitizers[camera_idx],
                                _MilGrabBuffers[camera_idx],
                                buffer_count,
                                MIL.M_STOP,
                                MIL.M_DEFAULT,
                                ProcessingFunctionPtr,
                                GCHandle.ToIntPtr(hUserData));

                OnGrabComplete(camera_idx);
            }
            catch (Exception e)
            {
                logger.Error()("[{0}] camera grab stop error. message : {1}", camera_idx, e.Message);
                return false;
            }

            logger.Info()("{0} camera grab stop.", camera_idx);
            return true;
        }
        public override bool Open()
        {
            if (!Allocate())
            {
                logger.Error()("Matrox Grabber Allocate failed.");
                return false;
            }

            if (!ApplyParam())
            {
                logger.Error()("Matrox grabber applay parameter failed.");
                return false;
            }

            logger.Info()("Matrox grabber open success.");
            return true;
        }
        public override bool Reset()
        {
            // Buffer Free
            if(_MilGrabBuffers != null)
            {
                foreach (var bufs in _MilGrabBuffers.Values)
                {
                    if (bufs != null)
                    {
                        //foreach (var buf in bufs)
                        for(int i = 0; i < bufs.Length; i++)
                        {
                            if (bufs[i] != MIL.M_NULL)
                            {
                                MIL.MbufFree(bufs[i]);
                                bufs[i] = MIL.M_NULL;
                            }
                        }
                    }
                }

                _MilGrabBuffers = null;
            }

            // Camera Free
            if(_MilDigitizers != null)
            {
                for(int i = 0; i < _MilDigitizers.Length; i++)
                {
                    if(_MilDigitizers[i] != MIL.M_NULL)
                    {
                        MIL.MdigFree(_MilDigitizers[i]);
                        _MilDigitizers[i] = MIL.M_NULL;
                    }
                }
                _MilDigitizers = null;
            }

            // Board Free
            if (_MilSystems != null)
            {
                for (int i = 0; i < _MilSystems.Length; i++)
                {
                    if (_MilSystems[i] != MIL.M_NULL)
                    {
                        MIL.MsysFree(_MilSystems[i]);
                        _MilSystems[i] = MIL.M_NULL;
                    }
                }

                _MilSystems = null;
            }

            return true;
        }
        #endregion

        #region constructors
        public MatroxGrabber(GrabberTypes grabberType)
            : base(grabberType)
        {
            MIL.MappAlloc(MIL.M_DEFAULT, ref _MilApplication);

            _ConnectedCameraCount = 0;

            if (_MilApplication == MIL.M_NULL)
            {
                logger.Error()("Mil Application allocated failed.");
            }

            //MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
            ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);

            _MilSystems = null;
            _MilDigitizers = null;
            _MilGrabBuffers = null;

            _matroxControllers = new ConcurrentDictionary<int, MatroxController>();

        }
        #endregion

    }
}
