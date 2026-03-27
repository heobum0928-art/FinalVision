using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using Basler.Pylon;
using OpenCvSharp;
using FinalVisionProject.Utility;
using FinalVisionProject.Setting;

namespace FinalVisionProject.Device {
    public partial class BaslerCamera : VirtualCamera, IDisposable {
        
        public Basler.Pylon.Camera CameraHandle { get; private set; }
        public PixelDataConverter PixelConverter { get; private set; } = new PixelDataConverter();
        
        private Mat TempMat = null;
        private Mat TempMat2 = null;

        //Event 생성        
        public override event StateEvent GuiCameraOpened = null;
        public override event StateEvent GuiCameraClosed = null;
        public override event StateEvent GuiConnectionLost = null;
        public override event StateEvent GuiReadyForDisplay = null;

        //Fps 계산용
        private Stopwatch mStopWatch = new Stopwatch();
        private int FrameDurationTicks;
        private readonly double RENDERFPS = 15; //초당 15frame으로 gui 갱신

        public override TimeSpan ElapsedTime { get { return mStopWatch.Elapsed; } }

        //static member
        private static Dictionary<string, ICameraInfo> DeviceList = new Dictionary<string, ICameraInfo>();
        public static int GetDeviceCount() {
            return DeviceList.Count;
        }
        /// <summary>
        /// Basler 장치 정보를 반환
        /// </summary>
        /// <param name="identifier">식별자</param>
        /// <returns>해당 장치 정보, 없으면 null</returns>
        public static ICameraInfo GetDeviceInfo(string identifier) {
            if (DeviceList.ContainsKey(identifier)) return DeviceList[identifier];

            for(int i = 0; i < DeviceList.Count; i++) {
                if (identifier == GetDeviceUserDefinedName(i)) return DeviceList.ElementAt(i).Value;
                if (identifier == GetDeviceIpAddress(i)) return DeviceList.ElementAt(i).Value;
                if (identifier == GetDeviceFriendlyName(i)) return DeviceList.ElementAt(i).Value;
            }
            return null;
        }
        /// <summary>
        /// identifier 이름의 장치가 존재하는지 확인
        /// </summary>
        /// <param name="identifier">식별자</param>
        /// <returns>존재하면 true, 없으면 false</returns>
        public static bool ContainsDevice(string identifier) {
            if (String.IsNullOrEmpty(identifier)) return false;
            if (GetDeviceInfo(identifier) != null) return true;
            return false;
        }
        /// <summary>
        /// 주어진 index에 해당하는 장치 정보를 반환
        /// </summary>
        /// <param name="index">장치 index</param>
        /// <returns>장치 정보, 없으면 null</returns>
        public static ICameraInfo GetDeviceInfo(int index) {
            if (index > DeviceList.Count - 1) return null;
            return DeviceList.ElementAt(index).Value;
        }

        /// <summary>
        /// 주어진 index에 해당하는 friendly 장치 이름을 반환
        /// </summary>
        /// <param name="index">장치 index</param>
        /// <returns>장치 이름</returns>
        public static string GetDeviceFriendlyName(int index) {
            if (index > DeviceList.Count - 1) return null;
            return DeviceList.ElementAt(index).Value[CameraInfoKey.FriendlyName];
        }

        /// <summary>
        /// 주어진 index에 해당하는 사용자 정의된 장치 이름을 반환
        /// </summary>
        /// <param name="index">장치 index</param>
        /// <returns>장치 이름</returns>
        public static string GetDeviceUserDefinedName(int index) {
            if (index > DeviceList.Count - 1) return null;
            return DeviceList.ElementAt(index).Value[CameraInfoKey.UserDefinedName];
        }

        /// <summary>
        /// 주어진 index에 해당하는 장치의 ip 주소를 반환
        /// </summary>
        /// <param name="index">장치 index</param>
        /// <returns>장치 이름</returns>
        public static string GetDeviceIpAddress(int index) {
            if (index > DeviceList.Count - 1) return null;
            if (DeviceList.ElementAt(index).Value[CameraInfoKey.DeviceType] != DeviceType.GigE) return null;
            return DeviceList.ElementAt(index).Value[CameraInfoKey.DeviceIpAddress];
        }

        /// <summary>
        /// 주어진 index에 해당하는 장치 이름을 반환
        /// </summary>
        /// <param name="index">장치 index</param>
        /// <returns>장치 이름</returns>
        public static string GetDeviceName(int index) {
            string name = GetDeviceUserDefinedName(index);
            if (String.IsNullOrEmpty(name)) name = GetDeviceIpAddress(index);
            if (String.IsNullOrEmpty(name)) name = GetDeviceFriendlyName(index);
            if (String.IsNullOrEmpty(name)) name = GetDeviceUserDefinedName(index);
            return name;
        }

        /// <summary>
        /// 시스템에 연결된 모든 basler 타입의 장치를 조회
        /// </summary>
        /// <param name="identifiers">찾고자 하는 장치 식별자(ip주소 또는 장치 이름, null이면 모두 찾음)</param>
        /// <returns>찾은 장치 개수</returns>
        public static int EnumerateDevice(params string [] identifiers) {
            string devType = null;
            if ((identifiers != null) && (identifiers.Length > 0)) {
                devType = identifiers[0];
            }

            try {
                DeviceList.Clear();

                List<ICameraInfo> camList = null;
                if (devType != null) camList = CameraFinder.Enumerate(devType);
                else camList = CameraFinder.Enumerate();

                foreach(ICameraInfo camInfo in camList) {
                    //1. user define name
                    string camName = camInfo[CameraInfoKey.UserDefinedName];
                    //2. ip address
                    if (camInfo[CameraInfoKey.DeviceType] == DeviceType.GigE) {
                        if (String.IsNullOrEmpty(camName)) {
                            camName = camInfo[CameraInfoKey.DeviceIpAddress];
                        }
                    }
                    //3. friendly name
                    if (String.IsNullOrEmpty(camName)) {
                        camName = camInfo[CameraInfoKey.FriendlyName];
                    }
                    DeviceList.Add(camName, camInfo);
                }
            }
            catch (Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] Enumerate Device, Ientifier : {0}, ({1})", identifiers.ToString(), e.Message);
            }
            return DeviceList.Count;
        }
        public static bool ContainsDevice(ICameraInfo info) {
            return DeviceList.ContainsValue(info);
        }
        

        /// <summary>
        /// 개별 장치에 대한 생성자
        /// </summary>
        /// <param name="config">설정 정보</param>
        /// <param name="imageType">이미지 타입</param>
        /// <param name="devName">장치 이름</param>
        public BaslerCamera(DisplayConfig config, DeviceInfo info) : base(config, info, ECameraType.Basler) {
            Properties = new BaslerCameraProperty(this);

            mStopWatch = new Stopwatch();
            // Calculate the number of stopwatch ticks for every frame at the given frame rate.
            double frametime = 1 / RENDERFPS;
            FrameDurationTicks = (int)(Stopwatch.Frequency * frametime);
        }
        
        ~BaslerCamera() {
            Dispose();
        }

        public void Dispose() {
            if (CameraHandle != null) {
                CameraHandle.ConnectionLost -= OnConnectionLost;
                CameraHandle.CameraOpened -= OnCameraOpened;
                CameraHandle.CameraClosed -= OnCameraClosed;
                if (CameraHandle.StreamGrabber != null) {
                    CameraHandle.StreamGrabber.GrabStarted -= OnGrabStarted;
                    CameraHandle.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
                    CameraHandle.StreamGrabber.GrabStopped -= OnGrabStopped;
                }
            }
            Close();
            
            PixelConverter.Dispose();

        }
        /// <summary>
        /// 해당 장치를 연다.
        /// </summary>
        /// <param name="info">장치 정보</param>
        /// <returns>성공이면 true, 실패이면 false</returns>
        public bool Open(ICameraInfo info) {
            try {
                this.CameraHandle = new Camera(info);
                CameraHandle.CameraOpened += Configuration.SoftwareTrigger;
                CameraHandle.ConnectionLost += this.OnConnectionLost;
                CameraHandle.CameraOpened += this.OnCameraOpened;
                CameraHandle.CameraClosed += this.OnCameraClosed;
                CameraHandle.StreamGrabber.GrabStarted += this.OnGrabStarted;
                CameraHandle.StreamGrabber.ImageGrabbed += this.OnImageGrabbed;
                CameraHandle.StreamGrabber.GrabStopped += this.OnGrabStopped;

                CameraHandle.Open();

                //강제 속성 고정
                //developer custom
                switch (Info.ImageType) {
                    case ECaptureImageType.Color24:
                        CameraHandle.Parameters[PLCamera.PixelFormat].SetValue(PLCamera.PixelFormat.BayerGB8);
                        break;
                    case ECaptureImageType.Gray8:
                        CameraHandle.Parameters[PLCamera.PixelFormat].SetValue(PLCamera.PixelFormat.Mono8);
                        break;
                }
                //set width
                if (CameraHandle.Parameters[PLCamera.Width].IsWritable) {
                    CameraHandle.Parameters[PLCamera.Width].SetValue(Info.Width);
                }
                else {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set Width to {1} failed!", Info.Identifier, Info.Width);
                }
                int width = (int)CameraHandle.Parameters[PLCamera.Width].GetValue();
                if(width != Info.Width) {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set Width to {1} failed!", Info.Identifier, Info.Width);
                }
                else {
                    Properties.Width = width;
                }

                //set height
                if (CameraHandle.Parameters[PLCamera.Height].IsWritable) {
                    CameraHandle.Parameters[PLCamera.Height].SetValue(Info.Height);
                }
                else {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set Height to {1} failed!", Info.Identifier, Info.Height);
                }
                int height = (int)CameraHandle.Parameters[PLCamera.Height].GetValue();
                if(height != Info.Height) {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set Height to {1} failed!", Info.Identifier, Info.Height);
                }
                else {
                    Properties.Height = height;
                }
                

                if (CameraHandle.Parameters[PLCamera.ReverseX].IsWritable) {
                    CameraHandle.Parameters[PLCamera.ReverseX].SetValue(Info.ReverseX);
                }
                else {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set ReverseX to {1} failed!", Info.Identifier, Info.ReverseX);
                }

                if (CameraHandle.Parameters[PLCamera.ReverseY].IsWritable) {
                    CameraHandle.Parameters[PLCamera.ReverseY].SetValue(Info.ReverseY);
                }
                else {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Set ReverseY to {1} failed!", Info.Identifier, Info.ReverseY);
                }

                //default
                if (CameraHandle.Parameters[PLCamera.ExposureAuto].IsWritable) {
                    CameraHandle.Parameters[PLCamera.ExposureAuto].SetValue(PLCamera.ExposureAuto.Off);
                }
                if (CameraHandle.Parameters[PLCamera.GainAuto].IsWritable) {
                    CameraHandle.Parameters[PLCamera.GainAuto].SetValue(PLCamera.GainAuto.Off);
                }
                if (CameraHandle.Parameters[PLCamera.GainSelector].IsWritable) {
                    CameraHandle.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.All);
                }
                
                //Gamma Enable
                if (CameraHandle.Parameters[PLCamera.GammaEnable].IsWritable) {
                    CameraHandle.Parameters[PLCamera.GammaEnable].SetValue(true);
                    //CameraHandle.Parameters[PLCamera.GammaSelector].SetValue(PLCamera.GammaSelector.sRGB);
                }
                /*
                //Balance White AUto
                if (CameraHandle.Parameters[PLCamera.BalanceWhiteAuto].IsWritable) {
                    CameraHandle.Parameters[PLCamera.BalanceWhiteAuto].SetValue(PLCamera.BalanceWhiteAuto.Once);
                }
               */
                
                //parameter
                Properties.Update();

                //rotate90, 270 경우 width, height가 반대
                if ((Info.RotateAngle == ERotateAngleType._90) || (Info.RotateAngle == ERotateAngleType._270)) {
                    int temp = Properties.Height;
                    Properties.Height = Properties.Width;
                    Properties.Width = temp;
                }

                CaptureMode = ECaptureModeType.Stop;
                //CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
            catch (Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] Camera {0} Open Fail, ({1})", Info.Identifier, e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// index에 해당되는 장치를 연다.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool Open(int index) {
            if (index > DeviceList.Count - 1) return false;
            return Open(GetDeviceInfo(index));
        }

        /// <summary>
        /// 주어진 ip 또는 이름에 해당되면 open한다.
        /// </summary>
        /// <param name="param">ip주소 또는 장치 이름</param>
        /// <returns></returns>
        public override bool Open(params object[] param) {
            string camIpOrName = null;
            if ((param != null) && (param.Length > 0)) {
                camIpOrName = param[0] as string;
            }
            ICameraInfo camInfo = null;
            if (IPAddress.TryParse(camIpOrName, out _)) {
                camInfo = GetDeviceInfo(camIpOrName);
                if(camInfo == null) {
                    return false;
                }
                
                //camInfo = IpConfigurator.AnnounceRemoteDevice(camIpOrName);
                //IP 주소로 찾아낸 카메라 정보가 장치 목록에 없는 경우, 추가한다.
                if (!ContainsDevice(camInfo)) {
                    string camName = camInfo[CameraInfoKey.UserDefinedName];
                    if (String.IsNullOrEmpty(camName)) camName = camInfo[CameraInfoKey.FriendlyName];
                    DeviceList.Add(camName, camInfo);
                    Name = camName;
                }
                return this.Open(camInfo);
            }

            camInfo = GetDeviceInfo(camIpOrName);
            return this.Open(camInfo);
        }

        //장치가 open된 경우 호출
        private void OnCameraOpened(object sender, EventArgs args) {
            IsOpen = true;
            if(GuiCameraOpened != null) {
                GuiCameraOpened(Name);
            }
        }

        //장치가 close 된 경우 호출
        private void OnCameraClosed(object sender, EventArgs args) {
            IsOpen = false;
            if(GuiCameraClosed != null) {
                GuiCameraClosed(Name);
            }
        }

        // grab이 시작된 경우 호출
        private void OnGrabStarted(object sender, EventArgs args) {
            IsGrabbing = true;
        }

        // grab이 완료된 경우 호출
        private void OnGrabStopped(object sender, EventArgs args) {
            IsGrabbing = false;
        }

        //이미지 그랩 이벤트
        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs args) {
            if ((CaptureMode != ECaptureModeType.Streaming) && (CaptureMode != ECaptureModeType.Trigger)) return;
            try {
                IGrabResult grabResult = args.GrabResult;
                if (grabResult.GrabSucceeded) {
                    ProcessGrabResultToCvMat(grabResult);
                }
                else {
                    Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Grab Fail, ErrorCode:{1}, Description:{2}", Name, grabResult.ErrorCode, grabResult.ErrorDescription);
                    Interlocked.Increment(ref errorCount);
                }
            }
            catch(Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Grab Error, Description:{1}", Name, e.Message);
            }
        }

        // 연결이 끊어진 경우 호출
        private void OnConnectionLost(object sender, EventArgs args) {
            if (GuiConnectionLost != null) GuiConnectionLost(Name);
        }

        public override bool SetTriggerMode(ETriggerSource source, bool forcing=false, bool threading = false) {
            if (CameraHandle == null) return false;
            if (TriggerSource == source && forcing == false) return true; // Source가 같으면 Skip

            try {
                ResetGrabCount();
                IEnumParameter triggerMode = CameraHandle.Parameters[PLCamera.TriggerMode];
                IEnumParameter triggerSource = CameraHandle.Parameters[PLCamera.TriggerSource];

                string triggerModeState = triggerMode.GetValue();
                if(triggerModeState != PLCamera.TriggerMode.On) {
                    if(triggerMode.TrySetValue(PLCamera.TriggerMode.On) == false) {
                        throw new Exception("Fail to Trigger Mode Set.");
                    }
                }
                string triggerSourceStr = "";
                switch (source) {
                    case ETriggerSource.Hardware_Line0:
                        triggerSourceStr = PLCamera.TriggerSource.Line1;
                        break;
                    case ETriggerSource.Hardware_Line1:
                        triggerSourceStr = PLCamera.TriggerSource.Line2;
                        break;
                    case ETriggerSource.Hardware_Line2:
                        triggerSourceStr = PLCamera.TriggerSource.Line3;
                        break;
                    case ETriggerSource.Hardware_Line3:
                        triggerSourceStr = PLCamera.TriggerSource.Line4;
                        break;
                    case ETriggerSource.Software:
                        triggerSourceStr = PLCamera.TriggerSource.Software;
                        break;
                }
                if(triggerSource.TrySetValue(triggerSourceStr) == false) {
                    throw new Exception("Fail to Trigger Source set " + source.ToString());
                }
                
                if (threading) CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                else CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByUser);

                if (CameraHandle.CanWaitForFrameTriggerReady) {
                    if (!CameraHandle.WaitForFrameTriggerReady(pConfig.TriggerModeTimeOut, TimeoutHandling.Return)) return false;
                }
                CaptureMode = ECaptureModeType.Trigger;
                TriggerSource = source;
            }
            catch (Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} SetSoftwareTrigger. ({1})", Name, e.Message);
            }
            return true;
        }

        /// <summary>
        /// 소프트웨어 트리거 모드로 변경한다.
        /// </summary>
        /// <param name="threading">쓰레드로 처리하는 경우 true</param>
        /// <returns>성공이면 true, 실패면 false</returns>
        public override bool SetSoftwareTriggerMode(bool threading = false) {
            if (CameraHandle == null) return false;
            if (CaptureMode == ECaptureModeType.Trigger) return true;

            try {
                ResetGrabCount();
                Configuration.SoftwareTrigger(CameraHandle, null);
                if (threading) CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                else CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByUser);

                if (CameraHandle.CanWaitForFrameTriggerReady) {
                    if (!CameraHandle.WaitForFrameTriggerReady(pConfig.TriggerModeTimeOut, TimeoutHandling.Return)) return false;
                }
                CaptureMode = ECaptureModeType.Trigger;
                TriggerSource = ETriggerSource.Software;
            }
            catch(Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} SetSoftwareTrigger. ({1})", Name, e.Message);
            }
            return true;
        }

        /// <summary>
        /// 소프트웨어 트리거를 수행한다.
        /// </summary>
        /// <returns>성공이면 true, 실패면 false</returns>
        public override bool ExecuteSoftwareTrigger() {
            if (CameraHandle == null) return false;
            try {
                prevImageCount = imageCount;
                CameraHandle.ExecuteSoftwareTrigger();
            }
            catch(Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} ExecSoftwareTrigger. ({1})", Name, e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 그랩이 성공적으로 되었는지 확인한다.
        /// 마지막 검사 이후로, 캡쳐가 성공적으로 수행되었는지 확인하는 용도로 사용할 수 있다.
        /// </summary>
        /// <returns>성공이면 true, 실패면 false</returns>
        public override bool IsGrabbed() {
            if (prevImageCount != imageCount) return true;
            return false;
        }

        /// <summary>
        /// 장치를 close 한다.
        /// </summary>
        public override void Close() {
            try {
                StopStream();
                if (CameraHandle != null) {
                    CameraHandle.Close();
                    CameraHandle.Dispose();
                    CameraHandle = null;
                }
            }catch(Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} Close. ({1})", Name, e.Message);
            }
        }
       
        //grabResult를 mat으로 처리하는 함수
        private void ProcessGrabResultToCvMat(IGrabResult grabResult, bool callEvent = true) {
            Interlocked.Increment(ref imageCount);

            //if(!mStopWatch.IsRunning || mStopWatch.ElapsedTicks >= FrameDurationTicks) {
            PixelType pixelType = grabResult.PixelTypeValue;
            byte[] buffer = grabResult.PixelData as byte[];
            long destSize = 0;
            lock (Interlock) {
                switch (Info.ImageType) {
                    case ECaptureImageType.Color24:
                        if (TempMat == null) {
                            PixelConverter.OutputPixelFormat = PixelType.BGR8packed;
                            TempMat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC3);
                        }
                        //TempMat.SetArray(buffer);
                        
                        destSize = PixelConverter.GetBufferSizeForConversion(grabResult);
                        PixelConverter.Convert(TempMat.Data, destSize, grabResult);

                        //Cv2.CvtColor(TempMat, TempMat, ColorConversionCodes.BayerBG2BGR);
                        break;
                    case ECaptureImageType.Gray8:
                        if (TempMat == null) {
                            //PixelConverter.OutputPixelFormat = PixelType.Mono8;
                            //TempMat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC1);
                            PixelConverter.OutputPixelFormat = PixelType.BGR8packed;
                            TempMat = new Mat(grabResult.Height, grabResult.Width, MatType.CV_8UC3);
                        }
                        //TempMat.SetArray(buffer);
                        //ptr = TempMat.Data;
                        //Marshal.Copy(buffer, 0, ptr, (int)grabResult.PayloadSize);

                        destSize = PixelConverter.GetBufferSizeForConversion(grabResult);
                        PixelConverter.Convert(TempMat.Data, destSize, grabResult);

                        //Cv2.CvtColor(TempMat, TempMat, ColorConversionCodes.BayerBG2GRAY);
                        break;
                }

                if (Info.RotateAngle == ERotateAngleType._0) {
                    LastGrabImage = TempMat;
                }
                else {
                    if (TempMat2 == null) {
                        TempMat2 = new Mat();
                    }
                    else if((TempMat2.Width != TempMat.Height) || (TempMat2.Height != TempMat.Width)) {
                        TempMat2 = new Mat();
                    }
                    if (Info.RotateAngle == ERotateAngleType._90) Cv2.Rotate(TempMat, TempMat2, RotateFlags.Rotate90Clockwise);
                    if (Info.RotateAngle == ERotateAngleType._180) Cv2.Rotate(TempMat, TempMat2, RotateFlags.Rotate180);
                    if (Info.RotateAngle == ERotateAngleType._270) Cv2.Rotate(TempMat, TempMat2, RotateFlags.Rotate90Counterclockwise);

                    LastGrabImage = TempMat2;
                }
            }
            //trigger the frame captured event 
            if (callEvent && (GuiReadyForDisplay != null)) {
                GuiReadyForDisplay(Name);
            }
            //}
        }
    
        /// <summary>
        /// TriggerMode로 이미지 그랩을 수행한다.
        /// </summary>
        /// <returns>성공이면 true, 실패면 false</returns>
        public override Mat GrabImage() {
            if ((CaptureMode == ECaptureModeType.Streaming)) return null;

            mStopWatch.Restart();

            if (!SetSoftwareTriggerMode()) return null;

            prevImageCount = imageCount;
            ExecuteSoftwareTrigger();
            
            try {
                IGrabResult grabResult = CameraHandle.StreamGrabber.RetrieveResult(pConfig.GrabTimeOut, TimeoutHandling.Return);
                if (grabResult == null) return null;
                using (grabResult) {
                    if (grabResult.GrabSucceeded) {
                        //ProcessGrabResultToBitmp(grabResult, false);
                        ProcessGrabResultToCvMat(grabResult, false);
                        //Logging.PrintLog((int)ELogType.Camera, "[GRAB] {0} GrabImage. ImageCount:{1}", Name, ImageCount);
                        return LastImage;
                    }
                    else {
                        Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} GrabImage. ErrorCode:{1} ({2})", Name, grabResult.ErrorCode, grabResult.ErrorDescription);
                        Interlocked.Increment(ref errorCount);
                    }
                }
            }
            catch (Exception e) {
                Trace.Write(e.Message);
                Interlocked.Increment(ref errorCount);
            }
            return null;
        }

        /// <summary>
        /// trigger mode를 해제하고 stream을 수행한다.
        /// </summary>
        /// <returns>성공이면 true, 실패면 false</returns>
        public override bool StartStream() {
            if (CameraHandle == null) return false;

            if (CaptureMode == ECaptureModeType.Streaming) return true;
            if (CaptureMode == ECaptureModeType.Trigger) StopStream();

            try {
                ResetGrabCount();
                Configuration.AcquireContinuous(CameraHandle, null);

                prevImageCount = imageCount;
                CameraHandle.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);

                CaptureMode = ECaptureModeType.Streaming;
                mStopWatch.Restart();
            }
            catch (Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} StartStream. ({1})", Name, e.Message);
            }
            return true;
        }

        /// <summary>
        /// stream을 정지한다.
        /// </summary>
        public override void StopStream() {
            if (CaptureMode == ECaptureModeType.Stop) return;
            try {
                CameraHandle.StreamGrabber.Stop();
                if (mStopWatch.IsRunning) {
                    mStopWatch.Stop();
                }
                IsGrabbing = false;
                CaptureMode = ECaptureModeType.Stop;
            }
            catch(Exception e) {
                Logging.PrintLog((int)ELogType.Camera, "[ERROR] {0} StopStream. ({2})", Name, e.Message);
            }

            base.StopStream();
        }

        public override void ClearLastFrame() {
            base.ClearLastFrame();
            if(TempMat != null) {
                if(TempMat.IsDisposed == false) {
                    TempMat.Dispose();
                }
            }
            TempMat = null;
        }

        public override Mat WaitForTrigger(bool clone = true, int timeOut = 3000) {
            lock (Interlock) {
                prevImageCount = imageCount;
                mStopWatch.Restart();
            }

            while (true) {
                if (mStopWatch.ElapsedMilliseconds > timeOut) return null;
                else if (IsGrabbed()) break;
            }
            if ((LastImage != null) && clone) return LastImage.Clone();
            return LastImage;
        }

    }
}
