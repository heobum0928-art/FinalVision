using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vieworks;

using VWGIGE_HANDLE = System.IntPtr;
using HINTERFACE = System.IntPtr;
using HCAMERA = System.IntPtr;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Globalization;
using Project.BaseLib.DataStructures;

namespace Project.Grabber
{
    public class VieworksGrabber : GrabberBase, IDisposable
    {
        #region fields
        Vieworks.VwGigE.ImageCallbackFn [] CallbackFuncs;

        protected Vieworks.CAMERA_INFO_STRUCT[] _DeviceCamInfos;
        protected IntPtr[] m_pobjectInfos = new IntPtr[4];

        protected HCAMERA[] _hCameras;

        protected VWGIGE_HANDLE m_pvwGigE = IntPtr.Zero;

        protected GCHandle gchGigE;
        protected GCHandle gchobjectInfo;
        protected GCHandle[] _gchCallback;

        #endregion

        #region propertise

        #endregion

        #region methods

        public override bool GrabOnce(int camera_idx)
        {

            if(!IsValid())
            {
                AppLogger.Error()("[{0}] GrabberType, [{1}] camera is not valid.", Name, camera_idx);
                return false;
            }

            if (IsGrabbing(camera_idx))
            {
                //AppLogger.Info()("[{0}] GrabberType, [{1}] camera is Grabbing.", Name, camera_idx);
                return true;
            }

            //if(preGrabEvent != null)
            //{
            //    preGrabEvent(camera_idx, GrabTypes.OnceGrab);
            //}



            Vieworks.RESULT ret = Vieworks.VwGigE.CameraSnap(_hCameras[camera_idx], 1);
            //Vieworks.RESULT ret = Vieworks.VwGigE.CameraGrab(_hCameras[camera_idx]);

            if (ret  != Vieworks.RESULT.RESULT_SUCCESS)
            {
                // fail
                AppLogger.Error()("[{0}] GrabberType, GrabOnce({1}) Success. Message : [{2}]", Name, camera_idx, ret);
                return false;
            }

            AppLogger.Info()("{0} GrabberType : GrabOnce({1})", Name, camera_idx);
            return true;
        }

        public override bool GrabStart(int camera_idx)
        {

            if (!IsValid())
            {
                AppLogger.Error()("[{0}] GrabberType, [{1}] camera is not valid.", Name, camera_idx);
                return false;
            }

            if (IsGrabbing(camera_idx))
            {
                //AppLogger.Info()("[{0}] GrabberType, [{1}] camera is Grabbing.", Name, camera_idx);
                return true;
            }





            //Vieworks.RESULT ret = Vieworks.VwGigE.CameraSnap(_hCameras[camera_idx], 1);
            Vieworks.RESULT ret = Vieworks.VwGigE.CameraGrab(_hCameras[camera_idx]);

            if (ret != Vieworks.RESULT.RESULT_SUCCESS)
            {
                // fail
                AppLogger.Error()("[{0}] GrabberType, GrabStart({1}) Success. Message : [{2}]", Name, camera_idx, ret);
                return false;
            }

            AppLogger.Info()("{0} GrabberType : GrabStart({1})", Name, camera_idx);
            return true;
        }

        public override bool GrabStop(int camera_idx)
        {

            //if (IntPtr.Zero == m_pCamera)
            //{
            //    System.Diagnostics.Trace.WriteLine("Camera == zero");
            //    return;
            //}

            if(!IsValid())
            {
                AppLogger.Error()("[{0}] GrabberType, [{1}] camera is not valid.", Name, camera_idx);
                return false;
            }


            if (!IsGrabbing(camera_idx))
            {
                //AppLogger.Info()("[{0}] GrabberType, [{1}] camera already stopping.", Name, camera_idx);
                return true;
            }


            Vieworks.RESULT ret = Vieworks.VwGigE.CameraAbort(_hCameras[camera_idx]);

            if (ret == Vieworks.RESULT.RESULT_SUCCESS ||
                 ret == Vieworks.RESULT.RESULT_ERROR_ABORTED_ALREADY)
            {
                //System.Diagnostics.Debug.WriteLine("CameraAbort Successed.");
                AppLogger.Info()("[{0}] GrabberType : GrabStop({1}) Successed.", Name, camera_idx);

                return true;
            }

            AppLogger.Info()("[{0}] GrabberType : GrabStop({1}) failed.", Name, camera_idx);
            return false;
        }
        public override bool Close()
        {            
            if (gchGigE.IsAllocated)
            {
                gchGigE.Free();
            }

            if (gchobjectInfo.IsAllocated)
            {
                gchobjectInfo.Free();
            }

            if(_gchCallback != null)
            {
                foreach(var gc in _gchCallback)
                {
                    if (gc.IsAllocated)
                        gc.Free();
                }
            }

            if (IntPtr.Zero != m_pvwGigE)
            {
                Vieworks.VwGigE.CloseVwGigE(ref m_pvwGigE);
                m_pvwGigE = IntPtr.Zero;
            }

            if (m_pobjectInfos != null)
            {
                foreach (var obj in m_pobjectInfos)
                {
                    if (obj != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(obj);
                    }
                }
            }
            return true;
        }
        public override bool Open()
        {
            if(!init())
            {
                AppLogger.Error()("[{0}] GrabberType init() failed.", Name);
                return false;
            }

            if(!Camera_Discovery())
            {
                AppLogger.Error()("[{0}] GrabberType Camera_Discovery() failed.", Name);
                return false;
            }

            if(!Camera_Open())
            {
                AppLogger.Error()("[{0}] GrabberType Camera_Open() failed.", Name);
                return false;
            }

            return true;
        }
        public override bool Reset()
        {
            if(!Camera_close())
            {
                AppLogger.Error()("[{0}] GrabberType Camera_Close() failed.", Name);
                return false;
            }

            if(!Camera_Open())
            {
                AppLogger.Error()("[{0}] GrabberType Camera_Open() failed.", Name);
                return false;
            }

            AppLogger.Info()("[{0}] GrabberType : Reset() is empty.", Name);
            return true;
        }

        //
        public bool IsValid()
        {
            if (gchGigE.IsAllocated)
                return true;
            else
                return false;
        }
        public bool IsGrabbing(int camera_index)
        {
            if (IntPtr.Zero == _hCameras[camera_index])
            {
                AppLogger.Info()("[{0}] GrabType {1} camera is not alloced.", Name, camera_index);
                return false;
            }

            bool bGrabbing = false;
            Vieworks.VwGigE.CameraGetGrabCondition(_hCameras[camera_index], ref bGrabbing);

            return bGrabbing;
        }
        public unsafe void GetImageEvent1(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            int cam_index = 0;
            if (preGrabEvent != null)
            {
                preGrabEvent(cam_index, GrabTypes.OnceGrab);
            }

            if(bufferGrabDoneEvent != null)
            {
                var image = ImageConvert(_hCameras[cam_index], pImageInfo.pImage, cam_index);

                bufferGrabDoneEvent(cam_index, 0, image);
            }
            AppLogger.Info()("[{0}] GrabberType. [{1}] camera GetImageEvent1()", Name, cam_index);
        }
        public unsafe void GetImageEvent2(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            int cam_index = 1;
            if (preGrabEvent != null)
            {
                preGrabEvent(cam_index, GrabTypes.OnceGrab);
            }
            if (bufferGrabDoneEvent != null)
            {
                var image = ImageConvert(_hCameras[cam_index], pImageInfo.pImage, cam_index);

                bufferGrabDoneEvent(cam_index, 0, image);
            }
            AppLogger.Info()("[{0}] GrabberType. [{1}] camera GetImageEvent2()", Name, cam_index);
        }
        public unsafe void GetImageEvent3(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            int cam_index = 2;
            if (preGrabEvent != null)
            {
                preGrabEvent(cam_index, GrabTypes.OnceGrab);
            }
            if (bufferGrabDoneEvent != null)
            {
                var image = ImageConvert(_hCameras[cam_index], pImageInfo.pImage, cam_index);

                bufferGrabDoneEvent(cam_index, 0, image);
            }
            AppLogger.Info()("[{0}] GrabberType. [{1}] camera GetImageEvent3()", Name, cam_index);
        }
        public unsafe void GetImageEvent4(IntPtr pObjectInfo, ref IMAGE_INFO pImageInfo)
        {
            int cam_index = 3;
            if (preGrabEvent != null)
            {
                preGrabEvent(cam_index, GrabTypes.OnceGrab);
            }
            if (bufferGrabDoneEvent != null)
            {
                var image = ImageConvert(_hCameras[cam_index], pImageInfo.pImage, cam_index);

                bufferGrabDoneEvent(cam_index, 0, image);
            }
            AppLogger.Info()("[{0}] GrabberType. [{1}] camera GetImageEvent4()", Name, cam_index);
        }

        public ByteImage ImageConvert(HCAMERA hCamera, IntPtr pImage, int camera_index)
        {
            int width = CameraInfos[camera_index].Width;
            int height = CameraInfos[camera_index].Height;
            int nSize = width * height;
            byte []array = new byte[nSize];
            Marshal.Copy(pImage, array, 0, nSize);
            return new ByteImage(width, height, width, array, 0);
        }
        public unsafe void GetDeviceInfo(HCAMERA hCamera, int nIndex, ref string strVenderName, ref string strModelName, ref string strDeviceVersion, ref string strDeviceID)
        {
            if (hCamera == IntPtr.Zero)
            {
                return;
            }

            const int STR_SIZE = 256;
            Byte[] btVenderName = new Byte[STR_SIZE];
            IntPtr pcbVendor = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btModelName = new Byte[STR_SIZE];
            IntPtr pcbModel = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btVersion = new Byte[STR_SIZE];
            IntPtr pcbVersion = Marshal.AllocHGlobal(sizeof(int));

            Byte[] btID = new Byte[STR_SIZE];
            IntPtr pcbID = Marshal.AllocHGlobal(sizeof(int));


            int[] nSize = new int[1];
            if (Vieworks.VwGigE.CameraGetDeviceVendorName(hCamera, nIndex, btVenderName, pcbVendor) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                Marshal.Copy(pcbVendor, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btVenderName[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }

                acHashedData = Encoding.Default.GetChars(btVenderName, 0, nValidCount);

                string temp = new string(acHashedData);
                strVenderName = temp;
            }

            if (Vieworks.VwGigE.CameraGetDeviceModelName(hCamera, nIndex, btModelName, pcbModel) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                Marshal.Copy(pcbModel, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btModelName[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }

                acHashedData = Encoding.Default.GetChars(btModelName, 0, nValidCount);

                string temp = new string(acHashedData);
                strModelName = temp;
            }

            if (Vieworks.VwGigE.CameraGetDeviceVersion(hCamera, nIndex, btVersion, pcbVersion) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                Marshal.Copy(pcbVersion, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btVersion[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }
                acHashedData = Encoding.Default.GetChars(btVersion, 0, nValidCount);

                string temp = new string(acHashedData);
                strDeviceVersion = temp;
            }

            if (Vieworks.VwGigE.CameraGetDeviceID(hCamera, nIndex, btID, pcbID) == Vieworks.RESULT.RESULT_SUCCESS)
            {
                Marshal.Copy(pcbID, nSize, 0, 1);
                char[] acHashedData = new char[nSize[0]];

                int nValidCount = 0;
                for (int i = 0; i < nSize[0]; i++)
                {
                    if (btID[i] == 0)
                    {
                        break;
                    }
                    nValidCount++;
                }
                acHashedData = Encoding.Default.GetChars(btID, 0, nValidCount);

                string temp = new string(acHashedData);
                strDeviceID = temp;
            }

            Marshal.FreeHGlobal(pcbVendor);
            Marshal.FreeHGlobal(pcbModel);
            Marshal.FreeHGlobal(pcbVersion);
            Marshal.FreeHGlobal(pcbID);
        }
        public Vieworks.RESULT GetCustomCommand(HCAMERA hCamera, string sFeatureName, ref int value, Vieworks.GET_CUSTOM_COMMAND eCmdType = Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            unsafe
            {
                Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
                int arrSize = 1024;
                IntPtr pSize = new IntPtr(&arrSize);
                Byte[] btArgument = new Byte[arrSize];
                int nCmdType = (int)eCmdType;
                eRet = Vieworks.VwGigE.CameraGetCustomCommand(hCamera, btFeatureName, btArgument, pSize, nCmdType);
                if (eRet == Vieworks.RESULT.RESULT_SUCCESS)
                {
                    string str = Encoding.Default.GetString(btArgument).TrimEnd('\0');
                    if (sFeatureName.Equals("PixelSize")
                        && eCmdType == Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE)
                    {
                        // Bpp8 Bpp10 Bpp12
                        str = str.Substring(3);
                    }
                    value = Convert.ToInt32(str);
                }
            }

            return eRet;
        }
        public Vieworks.RESULT GetCustomCommandString(HCAMERA hCamera, string sFeatureName, ref string sValue, Vieworks.GET_CUSTOM_COMMAND eCmdType = Vieworks.GET_CUSTOM_COMMAND.GET_CUSTOM_COMMAND_VALUE)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            unsafe
            {
                Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
                int arrSize = 1024;
                IntPtr pSize = new IntPtr(&arrSize);
                Byte[] btArgument = new Byte[arrSize];
                int nCmdType = (int)eCmdType;
                eRet = Vieworks.VwGigE.CameraGetCustomCommand(hCamera, btFeatureName, btArgument, pSize, nCmdType);
                if (eRet == Vieworks.RESULT.RESULT_SUCCESS)
                {
                    sValue = Encoding.Default.GetString(btArgument).TrimEnd('\0');
                }
            }

            return eRet;
        }
        public Vieworks.RESULT SetCustomCommand(HCAMERA hCamera, string sFeatureName, string sArgument)
        {
            Vieworks.RESULT eRet = Vieworks.RESULT.RESULT_ERROR;
            Byte[] btFeatureName = Encoding.UTF8.GetBytes(sFeatureName);
            Byte[] btArgument = Encoding.UTF8.GetBytes(sArgument);
            eRet = Vieworks.VwGigE.CameraSetCustomCommand(hCamera, btFeatureName, btArgument);

            return eRet;
        }

        protected bool init()
        {
            if(!Close())
            {
                AppLogger.Error()("[{0}] GrabberType close failed.", Name);
                return false;
            }

            CallbackFuncs = new VwGigE.ImageCallbackFn[4];
            CallbackFuncs[0] = GetImageEvent1;
            CallbackFuncs[1] = GetImageEvent2;
            CallbackFuncs[2] = GetImageEvent3;
            CallbackFuncs[3] = GetImageEvent4;

            _hCameras = new VWGIGE_HANDLE[CameraInfos.Length];
            _DeviceCamInfos = new CAMERA_INFO_STRUCT[CameraInfos.Length];

            _gchCallback = new GCHandle[CameraInfos.Length];


            for (int i = 0; i < CameraInfos.Length; i++)
            {
                _hCameras[i] = IntPtr.Zero;
            }

            // Open GigE
            Vieworks.RESULT result = Vieworks.VwGigE.OpenVwGigE(ref m_pvwGigE);
            gchGigE = GCHandle.Alloc(m_pvwGigE);

            if (result != Vieworks.RESULT.RESULT_SUCCESS)
            {
                AppLogger.Error()("{0} GrabberType : Failed to VwGigE.OpenVwGigE().", Name);
                return false;
            }

            return true;
        }
        protected bool Camera_Discovery()
        {
            // Discovery
            Vieworks.RESULT result = Vieworks.VwGigE.VwDiscovery(m_pvwGigE);

            if (Vieworks.RESULT.RESULT_SUCCESS != result)
            {
                AppLogger.Error()("[{0}] GrabType : Failed to VwDiscovery(). Message : [{1}]", Name, result.ToString());
                return false;
            }

            int nCameraNum = 0;
            result = Vieworks.VwGigE.VwGetNumCameras(m_pvwGigE, ref nCameraNum);

            if (Vieworks.RESULT.RESULT_SUCCESS != result)
            {
                AppLogger.Error()("[{0}] GrabType : Failed to VwGetNumCameras(). Message : [{1}]", Name, result.ToString());
                return false;
            }

            if (nCameraNum != CameraInfos.Length)
            {
                AppLogger.Error()("[{0}] GrabType : Camera count is over. data = [{1}], device = [{2}]",
                    Name, CameraInfos.Length, nCameraNum);

                return false;
            }

            return true;
        }
        protected bool Camera_Open()
        {
            int nCameraNum = CameraInfos.Length;

            for (int i = 0; i < nCameraNum; i++)
            {
                Vieworks.CAMERA_INFO_STRUCT tempCamInfo = new CAMERA_INFO_STRUCT();

                Vieworks.RESULT result = Vieworks.VwGigE.VwDiscoveryCameraInfo(m_pvwGigE, i, ref tempCamInfo);
                if (Vieworks.RESULT.RESULT_SUCCESS != result)
                {
                    AppLogger.Error()("{0} GrabberType : Failed to VwDiscoveryCameraInfo(). Camera index ={1}", Name, i);
                    return false;
                }

                var cam_info = CameraInfos.FirstOrDefault(f => (f as MatroxCameraInfo).CameraName == tempCamInfo.name);
                if (cam_info == null)
                {
                    AppLogger.Error()("[{0}] GrabberType, camera name is not matched. Device Name : [{2}]", Name, tempCamInfo.name);
                    return false;
                }

                var cam_num = cam_info.CamNumber;

                _DeviceCamInfos[cam_num] = tempCamInfo;


                OBJECT_INFO m_objectInfo = new OBJECT_INFO();

                int m_imagebuffernumber = 1;


                gchobjectInfo = GCHandle.Alloc(m_objectInfo);

                m_pobjectInfos[cam_num] = Marshal.AllocHGlobal(Marshal.SizeOf(m_objectInfo));


                Marshal.StructureToPtr(m_objectInfo, m_pobjectInfos[cam_num], true);
                GCHandle.Alloc(m_pobjectInfos[cam_num]);

                result = Vieworks.RESULT.RESULT_ERROR;

                Vieworks.VwGigE.ImageCallbackFn pCallback = new Vieworks.VwGigE.ImageCallbackFn(CallbackFuncs[cam_num]);
                _gchCallback[cam_num] = GCHandle.Alloc(pCallback);

                IntPtr ptrCallback = Marshal.GetFunctionPointerForDelegate(pCallback);

                const int STR_SIZE = 256;
                Byte[] btCameraName = new Byte[STR_SIZE];
                btCameraName = Encoding.UTF8.GetBytes(_DeviceCamInfos[cam_num].name);
                IntPtr pcbCamera = Marshal.AllocHGlobal(btCameraName.Length + 1);
                Marshal.Copy(btCameraName, 0, pcbCamera, btCameraName.Length);
                Marshal.WriteByte(pcbCamera, btCameraName.Length, 0);

                result = Vieworks.VwGigE.VwOpenCameraByName(m_pvwGigE, pcbCamera, ref _hCameras[cam_num], m_imagebuffernumber, 0, 0,
                                                             0, m_pobjectInfos[cam_num], ptrCallback, IntPtr.Zero);

                Marshal.FreeHGlobal(pcbCamera);

                if (result != Vieworks.RESULT.RESULT_SUCCESS)
                {
                    AppLogger.Error()("[{0}] GrabberType, [{1}] Camera : Failed to VwOpenCameraByName(). Message : [{2}]", Name, cam_num, result.ToString());
                    return false;
                }

                unsafe
                {
                    ((OBJECT_INFO*)m_pobjectInfos[cam_num])->pVwCamera = _hCameras[cam_num];
                }

                // Get device information
                string strVendorName = "";
                string strModelName = "";
                string strVersion = "";
                string strID = "";

                GetDeviceInfo(_hCameras[cam_num], _DeviceCamInfos[cam_num].index, ref strVendorName, ref strModelName, ref strVersion, ref strID);

                int tempwidth = 0;
                int tempheight = 0;
                int nPixelSize = 0;

                GetCustomCommand(_hCameras[cam_num], "Width", ref tempwidth);
                GetCustomCommand(_hCameras[cam_num], "Height", ref tempheight);
                GetCustomCommand(_hCameras[cam_num], "PixelSize", ref nPixelSize);

                if (cam_info.Width != tempwidth && cam_info.Height != tempheight)
                {
                    AppLogger.Error()("[{0}] GrabberType, [{1}] camera : Image Size is not matched. Data = [{2}, {3}], Device = [{4}, {5}]",
                        Name, cam_num, cam_info.Width, cam_info.Height, tempwidth, tempheight);

                    return false;
                }

                AppLogger.Info()("[{0}] GrabberType Open() Success. CamerIndex : [{1}], VendorName : [{2}], ModelName : [{3}], Version : [{4}], ID :[{5}], Width : [{6}], Height : [{7}], PixelSize : [{8}]",
                    Name, cam_num, strVendorName, strModelName, strVersion, strID, tempwidth, tempheight, nPixelSize);


            }

            return true;
        }
        protected bool Camera_close()
        {
            int cam_count = CameraInfos.Length;

            for(int i = 0; i < cam_count; i++)
            {
                Vieworks.RESULT result = Vieworks.VwGigE.CameraClose(_hCameras[i]);
                if (result != Vieworks.RESULT.RESULT_SUCCESS)
                {
                    AppLogger.Error()("[{0}] GrabberType, [{1}] camera close failed. Message : [{2}].", Name, i, result);
                    return false;
                }
            }
            return true;
        }
        public void Dispose()
        {
            Close();
            ClearRegisterEvent();
        }
        #endregion

        #region constructors
        public VieworksGrabber()
        : base(GrabberTypes.Vieworks) { }
        #endregion
    }
}
