using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using FinalVisionProject.Device;
using FinalVisionProject.Sequence;
using FinalVisionProject;

namespace FinalVisionProject.Device {
    /// <summary>
    /// 장치 ID
    /// </summary>
    public class DeviceInfo {
        public ECameraType CamType;
        public ECaptureImageType ImageType;
        public ETriggerSource TriggerSource;
        public string Identifier;

        public int Width;
        public int Height;

        public bool ReverseX;
        public bool ReverseY;

        public ERotateAngleType RotateAngle = 0;

        public DeviceInfo(ECameraType type, ECaptureImageType imageType, ETriggerSource triggerSource, string id, int width, int height, bool reverseX, bool reverseY, ERotateAngleType rotateAngle = ERotateAngleType._0) {
            CamType = type;
            ImageType = imageType;
            Identifier = id;

            Width = width;
            Height = height;

            ReverseX = reverseX;
            ReverseY = reverseY;
            RotateAngle = rotateAngle;
        }
    }
    
    /// <summary>
    /// 이미지 제공자의 Initialize() 처리 결과
    /// </summary>
    [Flags]
    public enum EInitializeResult {
        Success = 0,                                  //성공
        NoCamera = 1 << 0,                       //연결된 카메라 없음
        NotEnoughCamera = 1 << 1,            //연결된 카메라 개수 부족
        WrongCameraConnected = 1 << 2,   //잘못된 카메라 연결됨
        OpenFail = 1 << 3,                        //장치 열기 실패
        Unknown = 1 << 4,                        //알수없는 에러
    }

    public sealed partial class DeviceHandler : IDisposable {
        public static DeviceHandler Handle { get; } = new DeviceHandler();

        private List<DeviceInfo> IDList = new List<DeviceInfo>();

        private Dictionary<string, VirtualCamera> Devices = new Dictionary<string, VirtualCamera>();

        public DisplayConfig Config { get; private set; } = new DisplayConfig();

        private DeviceHandler() {
            RegisterRequiredDevices();
        }

        /// <summary>
        /// SystemHandler의 Initialize 시점에 호출됨
        /// 카메라 자원을 초기화
        /// </summary>
        /// <returns></returns>
        public EInitializeResult Initialize() {
            EInitializeResult result = EInitializeResult.Success;

            //enum hik device
            int hikConnectedCount = 0;
            if(IDList.Select(id => id.CamType == ECameraType.HIK) != null) {
                hikConnectedCount = HikCamera.EnumerateDevice();
            }
            
            int hikCamIndex = 0;

            //open all enumerated devices
            for (int i = 0; i < IDList.Count; i++) {
                DeviceInfo id = IDList[i];
                ECaptureImageType imageType = IDList[i].ImageType;
                ETriggerSource triggerSource = IDList[i].TriggerSource;
                string devName = IDList[i].Identifier;

                switch (IDList[i].CamType) {
                    case ECameraType.Virtual: {
                        if (devName == null) {
                            int devIndex = Devices.Count + 1;
                            id.Identifier = "VirtualCamera" + devIndex.ToString();
                        }
                        AddVirtualCamera(id);
                    }
                    break;
                    case ECameraType.HIK: {
                        if (GetCount(ECameraType.HIK) == GetRequiredCameraCount(ECameraType.HIK)) continue;

                        if (HikCamera.ContainsDevice(devName) == false) {
                            result &= ~EInitializeResult.Success; //success 제거
                            result |= EInitializeResult.OpenFail;
#if SIMUL_MODE
                                AddVirtualCamera(id);
#endif
                            continue;
                        }
                        if (devName == null) {
                            devName = HikCamera.GetDeviceName(hikCamIndex);
                            if (devName == null) {
                                result &= ~EInitializeResult.Success; //success 제거
                                result |= EInitializeResult.OpenFail;
#if SIMUL_MODE
                                AddVirtualCamera(id);
#endif
                                continue;
                            }
                            else if ((!String.IsNullOrEmpty(id.Identifier)) && (!HikCamera.ContainsDevice(id.Identifier))) {
                                result &= ~EInitializeResult.Success; //success 제거
                                result |= EInitializeResult.OpenFail;
#if SIMUL_MODE
                                AddVirtualCamera(id);
#endif
                                continue;
                            }
                        }

                        //요구된 카메라를 생성한다. (이후 open되지 않더라도, 요구된 장치 이므로)
                        HikCamera newCam = new HikCamera(Config, id);

                        //이름이 지정된 경우
                        if ((!String.IsNullOrEmpty(devName)) && (HikCamera.ContainsDevice(devName))) {
                            if (!newCam.Open(devName)) {
                                result &= ~EInitializeResult.Success; //success 제거
                                result |= EInitializeResult.OpenFail;
                            }
                        }
                        //이름이 지정되지 않은 경우(조회된 장치를 순차적으로 오픈)
                        else if (hikCamIndex <= HikCamera.GetDeviceCount()) { //연결된 카메라 모두 오픈   
                            if (!newCam.Open(hikCamIndex)) {
                                result &= ~EInitializeResult.Success; //success 제거
                                result |= EInitializeResult.OpenFail;
                            }
                        }

                        //Add to List
                        Devices.Add(devName, newCam);
                        hikCamIndex++;
                    }
                    break;
                    
                }
            }
            IDList.Clear();

            return result;
        }

        public VirtualCamera this[string name] {
            get {
                if (name == null) return null;
                if (Devices.ContainsKey(name) == false) return null;
                return Devices[name];
            }
        }

        public VirtualCamera this[int index] {
            get {
                if (index >= Devices.Count) return null;
                return Devices.ElementAt(index).Value;
            }
        }

        public int Count { get => Devices.Count; }

        public int GetCount(ECameraType type) {
            return Devices.Count(d => d.Value.CamType == type);
        }

        public int IndexOf(string name) {
            for(int i = 0; i < Count; i++) {
                if (this[i].Name == name) return i;
            }
            return -1;
        }

        public void StopStreamAll() {
            for(int i = 0; i < Devices.Count; i++) {
                Devices.ElementAt(i).Value.StopStream();
            }
        }

        public void SetRequiredDevice(ECameraType type, ECaptureImageType imageType, ETriggerSource triggerSource, string identifier, int width, int height, bool reverseX, bool reverseY, ERotateAngleType rotateAngle = 0) {
            DeviceInfo newID = new DeviceInfo(type, imageType, triggerSource, identifier, width, height, reverseX, reverseY, rotateAngle);
            IDList.Add(newID);
        }

        private int GetRequiredCameraCount(ECameraType type) {
            return IDList.Count(d => d.CamType == type);
        }

        private void AddVirtualCamera(DeviceInfo id) {
            if (String.IsNullOrEmpty(id.Identifier)) id.Identifier = string.Format("Virtual Camera {0}", GetCount(ECameraType.Virtual) + 1);
            VirtualCamera vCam = new VirtualCamera(Config, id);
            vCam.Open(id.Width, id.Height);
            Devices.Add(id.Identifier, vCam);
        }
        
        /// <summary>
        /// 지정된 개수만큼 모든 장치가 오픈되었는지 확인한다.
        /// </summary>
        /// <returns>요구된 개수보다 연결된 개수가 적은 경우 false 반환</returns>
        public bool IsAllOpen() {
            if (GetRequiredCameraCount(ECameraType.HIK) > GetCount(ECameraType.HIK)) return false;

            return true;
        }

        public bool ApplyProperty(ICameraParam param) {
            VirtualCamera cam = this[param.DeviceName];
            if (cam == null) return false;
            if (cam.Properties == null) return false;
            return cam.Properties.ApplyFromParam(param);
        }

        public Mat GrabImage(ICameraParam param) {
            VirtualCamera cam = this[param.DeviceName];
            if (cam == null) return null;
            if (cam.Properties == null) return null;
            if (!cam.Properties.ApplyFromParam(param)) return null;
            // VirtualCamera: BackgroundImagePath가 없으면 SimulImagePath 즉시 적용
            // → 레시피 리로드 없이 Settings에서 경로 변경해도 바로 반영됨
            if (cam.CamType == ECameraType.Virtual && cam.BackgroundImagePath == null) {
                string simulPath = SystemHandler.Handle.Setting.SimulImagePath;
                if (!string.IsNullOrEmpty(simulPath))
                    cam.BackgroundImagePath = simulPath;
            }
            return cam.GrabImage();
        }
        

        public void Dispose() {
            //Config.Save();

            for (int i = 0; i < Devices.Values.Count; i++) {
                VirtualCamera cam = Devices.Values.ElementAt(i);
                cam.Close();
                cam = null;
            }
            Devices.Clear();
        }
        
    }
}
