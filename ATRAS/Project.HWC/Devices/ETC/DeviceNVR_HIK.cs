using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public enum Channel_Type
    {
        Unknown,
        Analog,
        Digital
    }
    public class Channel_Info
    {
        #region fields
        private int ch_index;
        private int ch_number;
        private bool online;
        private int real_handle;
        private byte stream_type;

        private Channel_Type ch_type;

        #endregion

        #region propertise

        #endregion

        #region methods

        #endregion

        #region constructors
        public Channel_Info()
        {
            ch_index = -1;
            ch_number = -1;
            online = false;
            real_handle = -1;
            stream_type = 0;

            ch_type = Channel_Type.Unknown;
        }
        #endregion
    }


    public class DeviceNVR_HIK : DeviceBase, IDeviceNVRServer, IDisposable
    {
        #region fields

        private bool isReverse = false;
        private bool[] isPause;// = false;
        private object _lock = new object();

        protected Task task;
        protected CancellationTokenSource cancellationTokenSource;

        protected NVRServerConfig _NVRServerConfig;

        protected bool sdk_initialized;

        protected Int32 userID;

        private int dwAChanTotalNum;
        private int dwDChanTotalNum;

        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96];
        private bool[] bOnline = new bool[96];

        private int[] real_play_handle = new int[96];
        private int[] playback_handle = new int[96];
        private int[] download_handle = new int[96];

        private List<Channel_Info> _Channel_Infos;


        private CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        private CHCNetSDK.NET_DVR_IPPARACFG_V40 ip_param_config;
        private CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        private CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        private CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;
        #endregion

        #region propertise

        #endregion

        #region methods

        public override bool Connect()
        {
            AppLogger.Info()("[{0}] device's connect() is empty.", Name);
            return true;
        }

        public override bool Disconnect()
        {
            AppLogger.Info()("[{0}] device's disconnect() is empty.", Name);
            return true;
        }

        public bool CaptureBMP(int cctv_index)
        {
            //throw new NotImplementedException();
            /* PlayBack CaptureBmp
            if (playback_handle[cctv_index] < 0)
            {
                AppLogger.Error()("Please start playback firstly!"); //Playback should be started before BMP Snapshot
                return false;
            }

            string sBmpPicFileName;
            //the path and file name to save picture
            sBmpPicFileName = "D://test.bmp";

            //Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_PlayBackCaptureFile(playback_handle[cctv_index], sBmpPicFileName))
            {
                var iLastErr = CHCNetSDK.NET_DVR_GetLastError();                
                AppLogger.Error()("NET_DVR_PlayBackCaptureFile failed, error code=[{0}] ", iLastErr);
                return false;
            }
            else
            {
                AppLogger.Info()("Successful to capture the BMP file and the saved file is [{0}] ", sBmpPicFileName);
            }
            return true;
            // PlayBack CaptureBmp*/

            /*// Live view CaptureBmp
            if (real_play_handle[cctv_index] < 0)
            {
                AppLogger.Error()("Please start LiveView firstly!"); //Playback should be started before BMP Snapshot
                return false;
            }
            string sBmpPicFileName;
            //the path and file name to save picture
            sBmpPicFileName = "D://CaptureBmp_test.bmp";
            //Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_CapturePicture(real_play_handle[cctv_index], sBmpPicFileName))
            {
                var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("NET_DVR_LiveViewCaptureBmp failed, error code=[{0}] ", iLastErr);
                return false;
            }
            else
            {
                AppLogger.Info()("Successful to capture the BMP file and the saved file is [{0}] ", sBmpPicFileName);
            }
            return true;*/

            // Combine Live view Or PlayBack CaptureBmp
            if ((real_play_handle[cctv_index] < 0) && (playback_handle[cctv_index] < 0))
            {
                AppLogger.Error()("Please start LiveView or PlayBack firstly!"); //LiveView or Playback should be started before BMP Snapshot
                return false;
            }
            string sBmpPicFileName;
            //the path and file name to save picture
            //sBmpPicFileName = "D://CaptureBmp_test.bmp";
            var date = DateTime.Now.ToString("yyyy_MM_dd");
            var time = DateTime.Now.ToString("HH_mm_ss");

            string rootpath = string.Format("{0}\\{1}", "D://CCTV", date);

            if (!Directory.Exists(rootpath))
                Directory.CreateDirectory(rootpath);

            sBmpPicFileName = rootpath +  "\\" + string.Format("{0}_{1}", time , "_CaptureBmp_test.bmp");
            //Capture a BMP picture
            if (real_play_handle[cctv_index] > 0)
            {
                
                //sBmpPicFileName = "D://CCTV/" + datetime + "_CaptureBmp_test.bmp";
                if (!CHCNetSDK.NET_DVR_CapturePicture(real_play_handle[cctv_index], sBmpPicFileName))
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("NET_DVR_LiveViewCaptureBmp failed, error code=[{0}] ", iLastErr);
                    return false;
                }
                else
                {
                    AppLogger.Info()("Successful to capture the BMP file and the saved file is [{0}] ", sBmpPicFileName);
                }
                //return true;
            }

            if (playback_handle[cctv_index] > 0)
            {
                if (!CHCNetSDK.NET_DVR_PlayBackCaptureFile(playback_handle[cctv_index], sBmpPicFileName))
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("NET_DVR_PlayBackCaptureFile failed, error code=[{0}] ", iLastErr);
                    return false;
                }
                else
                {
                    AppLogger.Info()("Successful to capture the BMP file and the saved file is [{0}] ", sBmpPicFileName);
                }
                //return true;

            }

            return true;


        }

        public bool CaptureJPEG(int cctv_index)
        {
            throw new NotImplementedException();
        }

        public bool Login()
        {
            if (!sdk_initialized)
            {
                AppLogger.Error()("[{0}] device is not sdk initialized.", Name);
                return false;
            }
            
            userID = CHCNetSDK.NET_DVR_Login_V30(_NVRServerConfig.IPAddress, _NVRServerConfig.PortNumber, _NVRServerConfig.UserName, _NVRServerConfig.Password, ref DeviceInfo);
            if(userID < 0)
            {
                var lastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device login falied(NET_DVR_Login_V30). Error Code = [{1}].", Name, lastErr);

                return false;
            }
            AppLogger.Info()("[{0}] device login success. UserID : [{1}]", Name, userID);

            dwAChanTotalNum = DeviceInfo.byChanNum;
            dwDChanTotalNum = DeviceInfo.byIPChanNum + 256 * (int)DeviceInfo.byHighDChanNum;


            if(!IPChannel_Info())
            {
                return false;
            }

            if (DeviceType == DeviceTypes.HWS)
            {
                AppLogger.Info()("Device Type is HWS.");
                return true;
            }

            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            task = Task.Run(() =>
            {
                const int delay = 200;
                AppLogger.Info()("[{0}] device CCTV play start delay time : {1} ms", Name, delay);

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        AppLogger.Info()("[{0}] device thread stop.", Name);
                        break;
                    }

                    foreach (var config in _NVRServerConfig.CCTVConfig)
                    {
                        if (playback_handle[config.UI_Index] >= 0)
                            continue;

                        if (real_play_handle[config.UI_Index] >= 0)
                            continue;

                        if (!Real_Play(config.UI_Index))
                            continue;

                        AppLogger.Info()("[{0}] device Real_Play([{1}]) success.", Name, config.UI_Index);
                    }

                    Thread.Sleep(delay);
                }

            }, token);

            return true;
        }
        public bool Logout()
        {
            if(userID > 0)
            {
                if (!CHCNetSDK.NET_DVR_Logout(userID))
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device logout(NET_DVR_Logout) failed. Error code = [{1}]", Name, iLastErr);
                    return false;
                }
            }

            AppLogger.Info()("[{0}] device logout success. UserID : [{1}]", Name, userID);
            userID = -1;
            return true;
        }
        public bool Real_Play(int cctv_index)
        {
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            var item = _NVRServerConfig.CCTVConfig.Find(i => i.UI_Index == cctv_index);

            if (item == null)
            {
                AppLogger.Error()("[{0}] device [{1}] cctv index config is null.", Name, cctv_index);
                return false;
            }

            lpPreviewInfo.hPlayWnd = item.PictureBoxHandle;
            lpPreviewInfo.lChannel = iChannelNum[cctv_index];
            lpPreviewInfo.dwStreamType = 0;
            lpPreviewInfo.dwLinkMode = 0;
            lpPreviewInfo.bBlocked = true; 
            lpPreviewInfo.dwDisplayBufNum = 15;

            IntPtr pUser = IntPtr.Zero;

            real_play_handle[cctv_index] = CHCNetSDK.NET_DVR_RealPlay_V40(userID, ref lpPreviewInfo, null/*RealData*/, pUser);

            return true;
        }

        public bool PlayFast(int cctv_index)
        {
            uint iOutValue = 0;
            uint iLastErr = 0;

            if(playback_handle[cctv_index] < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index is not playing. error code = {2}", Name, cctv_index, iLastErr);
                return false;
            }
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYFAST, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PLAYFRAME(fast) is failed. error code = {2}", Name, cctv_index, iLastErr);
                return false;
            }

            return true;
        }

        public bool PlaySlow(int cctv_index)
        {
            uint iOutValue = 0;
            uint iLastErr = 0;

            if (playback_handle[cctv_index] < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index is not playing. error code = {2}", Name, cctv_index, iLastErr);
                return false;
            }
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYSLOW, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PLAYFRAME(slow) is failed. error code = {2}", Name, cctv_index, iLastErr);
                return false;
            }

            return true;
        }

        public bool FramePlay(int cctv_index)
        {
            uint iOutValue = 0;

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYFRAME, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PLAYFRAME() failed. error code = {2}", Name, cctv_index, iLastErr);
                return false;
            }

            AppLogger.Info()("[{0}] device [{1}] cctv index frame play.", Name, cctv_index);
            return true;
        }

        public bool Record(int cctv_index)
        {
            throw new NotImplementedException();
        }

        public bool PlayBack(int cctv_index, DateTime start, DateTime end)
        {
            var item = _NVRServerConfig.CCTVConfig.Find(i => i.UI_Index == cctv_index);

            if (item == null)
            {
                AppLogger.Error()("[{0}] device cctv index config[{1}] is not exist.", Name, cctv_index);
                return false;
            }

            uint iLastErr = 0;


            if(playback_handle[cctv_index] >= 0)
            {
                //Please stop playback if playbacking now.
                if (!CHCNetSDK.NET_DVR_StopPlayBack(playback_handle[cctv_index]))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopPlayBack() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
            }

            playback_handle[cctv_index] = -1;

            CHCNetSDK.NET_DVR_VOD_PARA struVodPara = new CHCNetSDK.NET_DVR_VOD_PARA();
            struVodPara.dwSize = (uint)Marshal.SizeOf(struVodPara);
            struVodPara.struIDInfo.dwChannel = (uint)iChannelNum[cctv_index]; //Channel number  
            struVodPara.hWnd = item.PictureBoxHandle;//handle of playback

            //Set the starting time to search video files
            struVodPara.struBeginTime.dwYear = (uint)start.Year;
            struVodPara.struBeginTime.dwMonth = (uint)start.Month;
            struVodPara.struBeginTime.dwDay = (uint)start.Day;
            struVodPara.struBeginTime.dwHour = (uint)start.Hour;
            struVodPara.struBeginTime.dwMinute = (uint)start.Minute;
            struVodPara.struBeginTime.dwSecond = (uint)start.Second;

            //Set the stopping time to search video files
            struVodPara.struEndTime.dwYear = (uint)end.Year;
            struVodPara.struEndTime.dwMonth = (uint)end.Month;
            struVodPara.struEndTime.dwDay = (uint)end.Day;
            struVodPara.struEndTime.dwHour = (uint)end.Hour;
            struVodPara.struEndTime.dwMinute = (uint)end.Minute;
            struVodPara.struEndTime.dwSecond = (uint)end.Second;

            //Playback by time
            playback_handle[cctv_index] = CHCNetSDK.NET_DVR_PlayBackByTime_V40(userID, ref struVodPara);
            if (playback_handle[cctv_index] < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackByTime_V40() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                return false;
            }

            uint iOutValue = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            {
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAYSTART) failed, error code = [{2}]", Name, cctv_index, iLastErr);
                return false;
            }


            if (real_play_handle[cctv_index] >= 0)
            {
                if (!CHCNetSDK.NET_DVR_StopRealPlay(real_play_handle[cctv_index]))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopRealPlay() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }

                real_play_handle[cctv_index] = -1;
            }

            AppLogger.Info()("[{0}] device play back start. index : [{1}], st : [{2}], et : [{3}]", Name, cctv_index, start.ToString("F"), end.ToString("F"));
            return true;
        }

        public bool Download_start(int cctv_index, DateTime start, DateTime end, string path, string filename)
        {
            try
            {      
                if (download_handle[cctv_index] >= 0)
                {
                    AppLogger.Error()("[{0}] device cctv index [{1}] already downloading, please stop firstly!", Name, cctv_index);
                    return false;
                }

                if(start.CompareTo(end) == 0)
                {
                    AppLogger.Error()("[{0}] device cctv index [{1}] star time[{2}] and end time[{3}] is same.", Name, cctv_index, start.ToString("G"), end.ToString("G"));
                    return false;
                }

                CHCNetSDK.NET_DVR_PLAYCOND struDownPara = new CHCNetSDK.NET_DVR_PLAYCOND();
                struDownPara.dwChannel = (uint)iChannelNum[cctv_index]; //Channel number  

                //Set the starting time
                struDownPara.struStartTime.dwYear = (uint)start.Year;
                struDownPara.struStartTime.dwMonth = (uint)start.Month;
                struDownPara.struStartTime.dwDay = (uint)start.Day;
                struDownPara.struStartTime.dwHour = (uint)start.Hour;
                struDownPara.struStartTime.dwMinute = (uint)start.Minute;
                struDownPara.struStartTime.dwSecond = (uint)start.Second;

                //Set the stopping time
                struDownPara.struStopTime.dwYear = (uint)end.Year;
                struDownPara.struStopTime.dwMonth = (uint)end.Month;
                struDownPara.struStopTime.dwDay = (uint)end.Day;
                struDownPara.struStopTime.dwHour = (uint)end.Hour;
                struDownPara.struStopTime.dwMinute = (uint)end.Minute;
                struDownPara.struStopTime.dwSecond = (uint)end.Second;

                string rootpath = string.Format("{0}\\{1}\\{2}\\{3}", path, start.Year, start.Month, start.Day);

                if (!Directory.Exists(rootpath))
                    Directory.CreateDirectory(rootpath);


                string full_path = rootpath + "\\" + string.Format("{0}_{1}_s{2}{3}{4}_e{5}{6}{7}.mp4", filename, cctv_index,
                                                                                                        start.Hour, start.Minute, start.Second,
                                                                                                        end.Hour, end.Minute, end.Second);


                AppLogger.Info()("[{0}] device, [{1}] cctv_index, start time : [{2}], end time : [{3}], Full Path : [{4}].", Name, cctv_index, start.ToString("G"), end.ToString("G"), full_path);

                uint iLastErr = 0;
                //Download by time
                download_handle[cctv_index] = CHCNetSDK.NET_DVR_GetFileByTime_V40(userID, full_path, ref struDownPara);

                if (download_handle[cctv_index] < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_GetFileByTime_V40() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }

                uint iOutValue = 0;
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(download_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYSTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    Download_stop(cctv_index);

                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PLAYSTART() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }

                //AppLogger.Info()("Download end.");
                //return true;

                int wait_time = 60 * 5 * 1000;

                Stopwatch sw = new Stopwatch();
                sw.Restart();

                while (true)
                {
                    if (sw.ElapsedMilliseconds > wait_time)
                    {
                        Download_stop(cctv_index);
                        AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_GetFileByTime_V40() failed, TimeOut Error.", Name, cctv_index);
                        return false;
                    }

                    var iPos = CHCNetSDK.NET_DVR_GetDownloadPos(download_handle[cctv_index]);


                    if (iPos == 100)
                    {
                        Download_stop(cctv_index);

                        AppLogger.Info()("[{0}] device [{1}] cctv index download complate.", Name, cctv_index);
                        return true;
                    }

                    if (iPos == 200)
                    {
                        Download_stop(cctv_index);

                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        AppLogger.Error()("[{0}] device [{1}] cctv index downloading is abnormal for the abnormal network! Error code = [{2}]", Name, cctv_index, iLastErr);
                        return false;
                    }

                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error()("[{0}] device Download Error. Error Message : {1}.", Name, ex.Message);
                return false;
            }
        }

        public bool Download_stop(int cctv_index)
        {
            uint iLastErr = 0;
            if(download_handle[cctv_index] >= 0)
            {
                if (!CHCNetSDK.NET_DVR_StopGetFile(download_handle[cctv_index]))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopGetFile() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
                download_handle[cctv_index] = -1;
            }

            return true;
        }

        public void SetNVRServerConfig(NVRServerConfig Config)
        {
            _NVRServerConfig = Config;



        }

        public bool IPChannel_Info()
        {
            if (!sdk_initialized)
            {
                AppLogger.Error()("[{0}] device is not sdk initialized.", Name);
                return false;
            }

            if (userID < 0)
            {
                var lastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device login falied(NET_DVR_Login_V30). Error Code = [{1}].", Name, lastErr);

                return false;
            }

            lock (_lock)
            {
                uint dwSize = (uint)Marshal.SizeOf(ip_param_config);

                IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
                Marshal.StructureToPtr(ip_param_config, ptrIpParaCfgV40, false);

                uint dwReturn = 0;
                int iGroupNo = 0;  //该Demo仅获取第一组64个通道，如果设备IP通道大于64路，需要按组号0~i多次调用NET_DVR_GET_IPPARACFG_V40获取

                if (!CHCNetSDK.NET_DVR_GetDVRConfig(userID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
                {
                    var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device get dvr config(NET_DVR_GET_IPPARACFG_V40) failed. Error code = [{1}]", Name, iLastErr);

                    Marshal.FreeHGlobal(ptrIpParaCfgV40);
                    return false;
                }
                else
                {
                    ip_param_config = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));
                    for (int i = 0; i < dwAChanTotalNum; i++)
                    {
                        iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                    }
                    byte byStreamType = 0;
                    int iDChanNum = 64;

                    if (dwDChanTotalNum < 64)
                    {
                        iDChanNum = dwDChanTotalNum; //如果设备IP通道小于64路，按实际路数获取
                    }

                    for (int i = 0; i < iDChanNum; i++)
                    {
                        iChannelNum[i + dwAChanTotalNum] = i + (int)ip_param_config.dwStartDChan;
                        byStreamType = ip_param_config.struStreamMode[i].byGetStreamType;

                        dwSize = (uint)Marshal.SizeOf(ip_param_config.struStreamMode[i].uGetStream);

                        switch (byStreamType)
                        {
                            case 0: // get stream from device directly
                                IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                                Marshal.StructureToPtr(ip_param_config.struStreamMode[i].uGetStream, ptrChanInfo, false);
                                m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                                //列出IP通道 List the IP channel
                                iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                                Marshal.FreeHGlobal(ptrChanInfo);
                                break;

                            case 1: // get stream from stream media server
                            case 2: // get stream from device after getting the IP address by IPServer
                            case 3: // get the device IP address by IPServer, and then get stream from stream media server
                            case 4: // get stream from stream media server by URL
                            case 5: // get steam from device after connecting the device via hkDDNS
                            case 6: // get steam from device directly (extended)
                                IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                                Marshal.StructureToPtr(ip_param_config.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                                m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                                //列出IP通道 List the IP channel
                                iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                                Marshal.FreeHGlobal(ptrChanInfoV40);
                                break;
                            default:
                                break;
                        }

                        if (m_struChanInfo.byIPID != 0 && m_struChanInfoV40.byEnable != 0)
                            bOnline[i] = true;
                        else
                            bOnline[i] = false;
                    }
                }

                Marshal.FreeHGlobal(ptrIpParaCfgV40);

                AppLogger.Info()("[{0}] device get ip channel info success. ", Name);

                return true;
            }
        }

        public void Dispose()
        {
            int k = 0;
        }

        public override bool Reset()
        {
            if (task != null || cancellationTokenSource != null)
            {   
                cancellationTokenSource.Cancel();
                task.Wait();

                task = null;
                cancellationTokenSource = null;
            }

            //if (real_play_handle[cctv_index] >= 0)

            foreach(var cctv_handle in real_play_handle)
            {
                if(cctv_handle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopRealPlay(cctv_handle))
                    {
                        var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        //AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopRealPlay() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                        return false;
                    }
                }

            }


            foreach (var playback_handle in playback_handle)
            {
                if (playback_handle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_StopRealPlay(playback_handle))
                    {
                        var iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        //AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopRealPlay() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                        return false;
                    }
                }
            }


            if (userID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(userID);
                userID = -1;
            }

            //CHCNetSDK.NET_DVR_Cleanup();

            AppLogger.Info()("[{0}] device reset success.", Name);
            return true;
        }

        public override bool Shutdown()
        {
            if(!Reset())
            {
                AppLogger.Error()("[{0}] device reset failed.", Name);
                return false;
            }
            foreach(var config in _NVRServerConfig.CCTVConfig)
            {
                PlayStop(config.UI_Index);
            }

            CHCNetSDK.NET_DVR_Cleanup();

            base.Shutdown();

            return true;

        }

        public bool PlayStop(int cctv_index)
        {
            uint iLastErr = 0;
            if (playback_handle[cctv_index] >= 0)
            {
                //Please stop playback if playbacking now.
                if (!CHCNetSDK.NET_DVR_StopPlayBack(playback_handle[cctv_index]))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopPlayBack() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
            }

            playback_handle[cctv_index] = -1;

            if (real_play_handle[cctv_index] >= 0)
            {
                if (!CHCNetSDK.NET_DVR_StopRealPlay(real_play_handle[cctv_index]))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_StopRealPlay() failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
            }
            real_play_handle[cctv_index] = -1;

            AppLogger.Info()("[{0}] device play stop success.", Name);
            return true;
        }

        public bool PlayPause(int cctv_index)
        {
            uint iOutValue = 0;

            uint iLastErr = 0;

            //if (!isPause[cctv_index])
            {
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYPAUSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAYPAUSE) failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
                isPause[cctv_index] = true;
            }
            //else
            //{
            //    if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAYRESTART, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
            //    {
            //        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
            //        AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAYRESTART) failed, error code = [{2}]", Name, cctv_index, iLastErr);
            //        return false;
            //    }
            //    isPause[cctv_index] = false;
            //}
            //AppLogger.Info()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40({2}) success.", Name, cctv_index, isPause[cctv_index] == true ? "NET_DVR_PLAYPAUSE" : "NET_DVR_PLAYRESTART");
            //return isPause[cctv_index];

            return true;
        }

        public bool PlayReverse(int cctv_index)
        {
            uint iLastErr = 0;
            uint iOutValue = 0;
            if (!isReverse)
            {
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAY_REVERSE, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAY_REVERSE) failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
                isReverse = true;
            }
            else
            {
                if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_PLAY_FORWARD, IntPtr.Zero, 0, IntPtr.Zero, ref iOutValue))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAY_FORWARD) failed, error code = [{2}]", Name, cctv_index, iLastErr);
                    return false;
                }
                isReverse = false;
            }

            AppLogger.Info()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40({2}) success.", Name, cctv_index, isReverse == true ? "NET_DVR_PLAY_REVERSE" : "NET_DVR_PLAY_FORWARD");
            return isReverse;
        }
        public DateTime? GetCurrentPlayTime(int cctv_index)
        {
            uint iLastErr = 0;
            uint iOutValue = 0;
            int time = 0;
            IntPtr lpOutBuffer = Marshal.AllocHGlobal(4);

            if (!CHCNetSDK.NET_DVR_PlayBackControl_V40(playback_handle[cctv_index], CHCNetSDK.NET_DVR_GETTOTALTIME, IntPtr.Zero, 0, lpOutBuffer, ref iOutValue))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                AppLogger.Error()("[{0}] device [{1}] cctv index NET_DVR_PlayBackControl_V40(NET_DVR_PLAYGETTIME) failed, error code = [{2}]", Name, cctv_index, iLastErr);
                return null;
            }

            time = (int)Marshal.PtrToStructure(lpOutBuffer, typeof(int));

            var nHour = (time / 3600) % 24;
            var nMinute = (time % 3600) / 60;
            var nSecond = time % 60;

            Marshal.FreeHGlobal(lpOutBuffer);
            return DateTime.Now;
        }
        #endregion

        #region constructors
        public DeviceNVR_HIK()
            : base(DeviceTypes.NVRServer_HIK)
        {
            sdk_initialized = false;

            userID = -1;

            dwAChanTotalNum = 0;
            dwDChanTotalNum = 0;

            _Channel_Infos = new List<Channel_Info>();

            for(int i = 0; i < bOnline.Length; i++)
            {
                bOnline[i] = false;
            }

            for (int i = 0; i < real_play_handle.Length; i++)
            {
                real_play_handle[i] = -1;
            }

            for (int i = 0; i < playback_handle.Length; i++)
            {
                playback_handle[i] = -1;
            }

            for (int i = 0; i < download_handle.Length; i++)
            {
                download_handle[i] = -1;
            }

            sdk_initialized = CHCNetSDK.NET_DVR_Init();

            if (sdk_initialized == false)
            {
                AppLogger.Error()("[{0}] Device NET_DVR_Init() failed.", Name);
            }
            else
                AppLogger.Info()("[{0}] device sdk initialized.", Name);

            isPause = new bool[6];
            for (int i = 0; i < isPause.Length; i++)
                isPause[i] = false;
        }

        #endregion

    }
}
