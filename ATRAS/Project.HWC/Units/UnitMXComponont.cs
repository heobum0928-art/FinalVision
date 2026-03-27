using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public delegate void OnMxcomponentDownloaded(int LogicalStationNumber, in short[] data);

    public abstract class UnitMXComponent : UnitTyped
    {
        #region fields
        protected OnMxcomponentDownloaded DownloadCompleted;


        protected short[] _read_data;
        protected short[] _write_data;
        #endregion

        #region propertise
        public IDeviceMxComponentX64 Device
        {
            get { return base.BaseDevice as IDeviceMxComponentX64; }
            set
            {
                base.BaseDevice = value;
            }
        }


        public new MXComponentConfig Config
        {
            get
            {
                return _Config as MXComponentConfig;
            }
            
        }
        #endregion

        #region methods
        public void RegisterMxcomponentDownloadEvent(OnMxcomponentDownloaded DownloadCompleted)
        {
            this.DownloadCompleted += DownloadCompleted;
        }

        protected void OnDownloadCompleted()
        {
            //TimetoPLC();

            //bool reSULT = TimetoPLC();
            if (DownloadCompleted != null)
            {
                DownloadCompleted(Config.LogicalStationNumber, in _read_data);
            }
        }
        public override IDevice CreateDevice()
        {
            if(DeviceType == DeviceTypes.MXComponent_X64)
            {
                //var device = DevicesRepository.Instance.GetDevice<DeviceMXComponentX64>();
                var device = new DeviceMXComponentX64();
                device.LogicalStationNumber = (Config as MXComponentConfig).LogicalStationNumber;
                return device;
                //var device = DevicesRepository.Instance.GetDevice<DeviceMXComponentX64>();

                //return device;
            }
            else if(DeviceType == DeviceTypes.HWS)
            {
                return DevicesRepository.Instance.GetDevice<DeviceMXComponentXSim>();
            }

            return null;
        }
        public override bool Initialize()
        {

            var init = InitializationState;
            return true;
        }
        public bool TimetoPLC()
        {

            DateTime now = DateTime.Now;

            int trigger = 6; 
            int yearIndex = 0;    // D95020 TIME YEAR
            int monthIndex = 1;   // D95021 TIME MONTH
            int dayIndex = 2;     // D95022 TIME DAY
            int hourIndex = 3;    // D95023 TIME HOUR
            int minuteIndex = 4;  // D95024 TIME MINUTE
            int secondIndex = 5;  // D95025 TIME SECOND

            _write_data = new short[7];
            _write_data[yearIndex] = (short)now.Year;
            _write_data[monthIndex] = (short)now.Month;
            _write_data[dayIndex] = (short)now.Day;
            _write_data[hourIndex] = (short)now.Hour;
            _write_data[minuteIndex] = (short)now.Minute;
            _write_data[secondIndex] = (short)now.Second;
            

            // Optionally, you can log the current time values
            AppLogger.Info()($"Current Time Set: {_write_data[yearIndex]}-{_write_data[monthIndex]}-{_write_data[dayIndex]} {_write_data[hourIndex]}:{_write_data[minuteIndex]}:{_write_data[secondIndex]}");
            //List<int> logicalStationNumbers = Config.LogicalStationNumber;
            
                Config.LogicalStationNumber = 3;
                // Here you would call the BSend method to send _write_data to the PLC
            if (!BSend("D95020", _write_data.Length, ref _write_data))
            {
                    AppLogger.Error()("Failed to send data to PLC.");
                    return false;
            }

            
            if(!BSend("D95011", 1, ref _write_data))
            {
                AppLogger.Error()(" GOOD");
            }
            OnDownloadCompleted();
            return true;
        }
        public abstract bool Download();
        public abstract bool Upload();

        public bool BReceive(int nStart, int size, ref short[] data)
        {
            if (Device == null)
            {
                data = null;

                AppLogger.Error()("MXComponent device is Null. BReceive is failed.");
                return false;
            }

            return Device.BReceive(nStart, size, ref data);
        }
        public bool BReceive(string device, int size, ref short[] data)
        {
            if (Device == null)
            {
                data = null;
                AppLogger.Error()("MXComponent device is Null. BReceive is failed.");

                return false;
            }

            return Device.BReceive(device, size, ref data);
        }
        public bool BSend(int nstartadd, int size, ref short[] data)
        {
            if (Device == null)
            {
                AppLogger.Error()("MXComponent device is Null. BSend is failed.");
                return false;
            }


            return true;
        }
        public bool BSend(string device, int size, ref short[] data)
        {
            if (Device == null)
            {
                AppLogger.Error()("MXComponent device is Null. BSend is failed.");
                return false;
            }

            return true;
        }
        #endregion

        #region constructors
        //public UnitMXComponent() 
        //    : base(UnitIDs.UnitMxcomponent)
        //{

        //}


        public UnitMXComponent()
            : base(UnitIDs.UnitMxcomponent)
        {

        }
        #endregion
    }
}
