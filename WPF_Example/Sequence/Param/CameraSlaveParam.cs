using System;
using System.Collections.Generic;
using System.Linq;
using OpenCvSharp;
using PropertyTools;
using PropertyTools.DataAnnotations;
using FinalVisionProject.Define;
using FinalVisionProject.Device;
using FinalVisionProject.Utility;

namespace FinalVisionProject.Sequence {

    public class CameraSlaveParam : ParamBase, ICameraParam {
        [Browsable(false)]
        private DeviceHandler pDev;

        [Browsable(false)]
        private LightHandler pLight;


        [Category("General|AOI")]
        // 레거시 필드 제거  //260327 hbk
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }


        [Category("Device|Light")]
        [ReadOnly(true)]
        public string LightGroupName {
            get {
                return _LightGroupName;
            }
            set {
                if (value == null) return;
                _LightGroupName = value;
            }
        }
        private string _LightGroupName;

        public int LightLevel { get; set; }

        /*
        // 03.06 Insert Start
        // 03.07 주석 처리 : CameraSlaveParam.cs 에서 이렇게 프로퍼티를 설정하면, Wafer 시퀀스이외의
        // Front, Bottom 시퀀스의 Device 탭에도 해당 프로퍼티가 표시되기 때문에 주석 처리함.
        [Category("Device|WAFER OUTER Light")]
        [DisplayName("Outer Lightlevel")]
        public int OuterLevel{ get; set; }
        // 03.06 Insert End
        */

        [Category("Device|Camera")]
        [ReadOnly(true)]
        public string DeviceName {
            get {
                return _DeviceName;
            }
            set {
                if (value == null) return;

                _DeviceName = value;

                //선택한 장치의 현재 property 를 가져온다.
                if (pDev == null) return;
                var selectedDev = pDev[value];
                if (selectedDev == null) return;
                this.PasteFromCamera(selectedDev);
            }
        }
        private string _DeviceName;


        [Browsable(false)]
        public string[] PropertyNameList { get; }


        [Category("Device|Camera")]
        [HeaderPlacement(HeaderPlacement.Collapsed)]
        public PropertyItem[] PropertyArray { get; set; }


        public CameraSlaveParam(object owner) :base(owner) {
            pDev = SystemHandler.Handle.Devices;
            pLight = SystemHandler.Handle.Lights;

            this.PropertyNameList = Enum.GetNames(typeof(ECameraPropertyType));
            this.PropertyArray = new PropertyItem[PropertyNameList.Length];
            for (int i = 0; i < this.PropertyArray.Length; i++) {
                this.PropertyArray[i] = new PropertyItem(PropertyNameList[i]);
            }
        }

        // 레거시 픽셀변환 메서드 제거  //260327 hbk


        private PropertyItem SearchProperty(ECameraPropertyType type) {
            for(int i = 0; i< PropertyArray.Length; i++) {
                if (PropertyArray[i].GetPropertyType() == type) return PropertyArray[i];
            }
            return null;
        }

        public void PasteFromCamera(VirtualCamera camera) {
            if(camera.Properties == null) {
                //error occurs
                return;
            }
            for(int i = 0; i < camera.Properties.Count; i++) {
                ECameraPropertyType type = camera.Properties.GetPropType(i);
                decimal value = camera.Properties[type];
                PropertyItem item = SearchProperty(type);
                if (item == null) continue;
                item.Value = (double)value;
            }
        }

        [Browsable(false)]
        public string SequenceName {
            get {
                return Parent.Name;
            }
        }

        [Browsable(false)]
        public string ActionName
        {
            get
            {
                if (Owner is ActionBase)
                {
                    return (Owner as ActionBase).Name;
                }
                return null;
            }
        }

        public void CopyToCamera(VirtualCamera camera) {
            for(int i = 0; i < PropertyArray.Length; i++) {
                ECameraPropertyType type = PropertyArray[i].GetPropertyType();
                camera.Properties[type] = (decimal)PropertyArray[i].Value;
            }
        }

        [Browsable(false)]
        public decimal this[int idx] {
            get {
                if (idx >= PropertyArray.Length) return 0;
                return (decimal)PropertyArray.ElementAt(idx).Value;
            }
        }

        [Browsable(false)]
        public decimal this[string propName] {
            get {
                ECameraPropertyType type = (ECameraPropertyType)Enum.Parse(typeof(ECameraPropertyType), propName);
                foreach(PropertyItem info in PropertyArray) {
                    if(info.GetPropertyType() == type) return (decimal)info.Value;
                }
                return 0;
            }
        }

        public override bool Save(IniFile saveFile, string groupName) {
            return base.Save(saveFile, groupName);
        }

        public virtual void PutImage(Mat image) {

        }

        public override bool CopyTo(ParamBase param) {
            if (param is CameraMasterParam) {
                CameraMasterParam masterParam = param as CameraMasterParam;
                return false;
            }
            else if (param is CameraSlaveParam) {
                CameraSlaveParam slaveParam = param as CameraSlaveParam;
                //slaveParam.DeviceName = this.DeviceName;
                //slaveParam.LightGroupName = this.LightGroupName;
                slaveParam.LightLevel = this.LightLevel;

                for (int i = 0; i < this.PropertyNameList.Length; i++) {
                    if (this.PropertyNameList[i] == slaveParam.PropertyNameList[i]) {
                        slaveParam.PropertyArray[i].Value = this.PropertyArray[i].Value;
                    }
                }

                // 레거시 필드 대입 제거  //260327 hbk
                slaveParam.FrameWidth = this.FrameWidth;
                slaveParam.FrameHeight = this.FrameHeight;
                return true;
            }
            else if (param is CameraParam) {
                CameraParam camParam = param as CameraParam;
                //camParam.DeviceName = this.DeviceName;
                //camParam.LightGroupName = this.LightGroupName;
                camParam.LightLevel = this.LightLevel;

                for (int i = 0; i < this.PropertyNameList.Length; i++) {
                    if (this.PropertyNameList[i] == camParam.PropertyNameList[i]) {
                        camParam.PropertyArray[i].Value = this.PropertyArray[i].Value;
                    }
                }

                // 레거시 필드 대입 제거  //260327 hbk
                camParam.FrameWidth = this.FrameWidth;
                camParam.FrameHeight = this.FrameHeight;
                return true;
            }
            return false;
        }
    }
}
