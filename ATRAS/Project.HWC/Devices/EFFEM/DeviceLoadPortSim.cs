using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using Project.BaseLib.DataStructures;
using System.Threading;

namespace Project.HWC
{
    public class DeviceLoadPortSim : DeviceBase, IDeviceLoadPort
    {
        #region fields
        private const byte delemeterCR = 0x0d;
        private const byte delemeterLF = 0x0a;

        //protected LoadPortConfig _Config;

        protected LoadPortInfo _LoadPortInfo;
        #endregion

        #region propertise

        public LoadPortConfig Config { get; set; }        

        #endregion

        #region methods
        public bool ClampCarrier()
        {
            int time = Config.ClampTime;
            Thread.Sleep(time);

            _LoadPortInfo.ClampState = ClampStates.Clamped;
            AppLogger.Info()($"ClamCarrier() - [{Name} device is ClampCarrier. [{time}] ms");
            return true;
        }

        public bool CloseDoor()
        {
            int time = Config.DoorOpenCloseTime;
            _LoadPortInfo.DoorState = DoorStates.Closing;

            Thread.Sleep(time);

            _LoadPortInfo.DoorState = DoorStates.Closed;
            AppLogger.Info()($"CloseDoor() - [{Name} device is CloseDoor. [{time}] ms");
            return true;
        }

        public bool DockCarrier()
        {
            int time = Config.DockTime;

            Thread.Sleep(time);

            _LoadPortInfo.DockState = DockStates.Docked;
            AppLogger.Info()($"DockCarrier() - [{Name} device is DockCarrier. [{time}] ms");
            return true;
        }

        public ClampStates GetClampState()
        {
            AppLogger.Info()($"GetClampState() - [{Name} device ClampState is {_LoadPortInfo.ClampState}");

            return _LoadPortInfo.ClampState;
        }

        public DockStates GetDockState()
        {
            AppLogger.Info()($"GetDockState() - [{Name} device DockState is {_LoadPortInfo.DockState}");

            return _LoadPortInfo.DockState;
        }

        public DoorStates GetDoorState()
        {
            AppLogger.Info()($"GetDoorState() - [{Name} device DoorState is {_LoadPortInfo.DoorState}");

            return _LoadPortInfo.DoorState;
        }

        public bool GetPlacement()
        {
            AppLogger.Info()($"GetPlacement() - [{Name} device placement is {_LoadPortInfo.CarrierPlacement}");

            return _LoadPortInfo.CarrierPlacement;
        }

        public LoadPortInfo GetPortInfo()
        {
            //AppLogger.Info()($"GetPortInfo() - [{Name} device Port Info is {_LoadPortInfo.ToString()}");

            return _LoadPortInfo;
        }

        public bool GetPresent()
        {
            AppLogger.Info()($"GetPlacement() - [{Name} device present is {_LoadPortInfo.CarrierPresence}");

            return _LoadPortInfo.CarrierPresence;
        }

        public bool Homing()
        {
            AppLogger.Info()($"Homing() - [{Name} device homed");

            return true;
        }

        public SlotMapResult MapCarrier()
        {
            List<CarrierSlot> slots = new List<CarrierSlot>();
            for (int i = 0; i < _LoadPortInfo.SlotMap.Count; i++)
            {
                var slot = _LoadPortInfo.SlotMap[i];
                slots.Add(slot);
            }
            SlotMapResult result = new SlotMapResult(ReadStatus.ReadOK, slots);

            AppLogger.Info()($"MapCarrier() - [{Name} device Map Carrier is {result.ToString()}");

            return result;
        }

        public bool OpenDoor()
        {
            int time = Config.DoorOpenCloseTime;

            _LoadPortInfo.DoorState = DoorStates.Opening;

            Thread.Sleep(time);

            AppLogger.Info()($"OpenDoor() - [{Name} device is OpenDoor. [{time}] ms");


            _LoadPortInfo.DoorState = DoorStates.Opened;
            return true;
        }

        public void SetExternalMapper(IDeviceMapper IDevice)
        {
            AppLogger.Info()($"SetExternalMapper() - [{Name} device is SetExternalMapper");
        }

        public void SetLightStates(List<LoadPortLightState> loadPortLightsMapping)
        {
            throw new NotImplementedException();
        }

        public void SetSlotCount(int slotCount)
        {
            AppLogger.Info()($"{Name} device set slot count : {slotCount}");
        }

        public bool UndockCarrier()
        {
            int time = Config.DockTime;
            Thread.Sleep(time);

            _LoadPortInfo.DockState = DockStates.Undocked;

            AppLogger.Info()($"UndockCarrier() - [{Name} device is UndockCarrier. [{time}] ms");
            return true;
        }

        public bool UnclampCarrier()
        {
            int time = Config.ClampTime;
            Thread.Sleep(time);

            _LoadPortInfo.ClampState = ClampStates.Unclamped;
            AppLogger.Info()($"UnclampCarrier() - [{Name} device is UnclampCarrier. [{time}] ms"); 
            return true;
        }

        public override bool Connect()
        {
            AppLogger.Info()($"Connect() - [{Name} device is Connect");
            return true;
        }

        public override bool Disconnect()
        {
            AppLogger.Info()($"Disconnect() - [{Name} device is Disconnect");
            return true;
        }

        public bool HWInitialize()
        {
            AppLogger.Info()($"HWInitialize() - [{Name} device is HWInitialize");
            return true;
        }

        public bool UserCommand(string command)
        {
            AppLogger.Debug()($"[{Name}] device user command : >> {command + Convert.ToChar(delemeterCR) + Convert.ToChar(delemeterLF)}");
            return true;

        }

        public void SetLoadPortInfo(LoadPortInfo info)
        {
            _LoadPortInfo = info.Duplicate();
        }
        #endregion

        #region constructors
        public DeviceLoadPortSim() : base(DeviceTypes.HWS)
        {
            Config = null;
            _LoadPortInfo = new LoadPortInfo();
        }
        #endregion
    }
}
