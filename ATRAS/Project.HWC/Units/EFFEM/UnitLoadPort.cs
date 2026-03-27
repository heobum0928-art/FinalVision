using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Project.BaseLib.HW;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using Project.BaseLib.DataStructures;
using System.Collections.ObjectModel;

namespace Project.HWC
{
    public class UnitLoadPort : UnitTyped
    {
        #region fields
        #endregion

        #region propertise
        public IDeviceLoadPort Device
        {
            get { return base.BaseDevice as IDeviceLoadPort; }

            set
            {
                base.BaseDevice = value;
            }
        }

        new public LoadPortConfig Config
        {
            get
            {
                return (base.Config as LoadPortConfig);
            }
        }
        #endregion

        #region methods

        public override IDevice CreateDevice()
        {
            IDevice device = null;

            if (DeviceType == DeviceTypes.LoadPort_TIS)
            {
                device = new DeviceLoadPortTIS(Config as NetworkConfig);

            }
            else
            {
                var d = new DeviceLoadPortSim();
                d.Config = Config;

                LoadPortInfo lpip = new LoadPortInfo(Config.LoadPortIndex, false, false, ClampStates.Undefined, DockStates.Undefined, DoorStates.Undefined);
                d.SetLoadPortInfo(lpip);
                device = d;
            }

            return device;
        }

        public override bool Initialize()
        {
            //CreateExternalMapper();

            var r = Device.HWInitialize();
            if(r == false)
            {
                AppLogger.Error()($"[{UnitName}] unit hw initialize failed.");
                return false;
            }

            //
            LoadPortInfo osl = GetLoadPortInfo();
            if (osl == null)
            {
                //return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "HWInitialize failed"));
                AppLogger.Error()($"[{UnitName}] unit hw initialize failed.");
                return false;
            }

            //
            //os = Device.SetSlotCount(NumberOfSlots);
            //if (os.IsError)
            //{
            //    return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "Set slot count failed"));
            //}

            // safe initialize
            if (!osl.LoadCompleted && !osl.UnloadCompleted)
            {
                if (osl.DockState == DockStates.Docked)
                {
                    var r1 = UndockCarrier();
                    if (r1 == false)
                    {
                        AppLogger.Error()($"[{UnitName}] unit hw initialize failed.");
                        return false;
                        //return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "HWInitialize failed"));
                    }
                }

                if (osl.ClampState == ClampStates.Clamped)
                {
                    var r2 = UnclampCarrier();
                    if (r2 == false)
                    {
                        AppLogger.Error()($"[{UnitName}] unit hw initialize failed.");
                        return false;
                        //return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "HWInitialize failed"));
                    }
                }
            }

            return true;
            AppLogger.Info()($"{UnitName} unit Initialize() is empty.");
            AppLogger.Info()($"{UnitName} unit, {Config.DeviceType} device type initialize success.");
            return true;
        }

        public bool ClampCarrier()
        {
            if (Device != null)
            {
                return Device.ClampCarrier();
            }

            return false;
        }
        public bool UnclampCarrier()
        {
            if (Device != null)
            {
                return Device.UnclampCarrier();
            }

            return false;
        }

        public bool OpenDoor()
        {
            if (Device != null)
            {
                return Device.OpenDoor();
            }

            return false;
        }
        public bool CloseDoor()
        {
            if (Device != null)
            {
                return Device.CloseDoor();
            }

            return false;
        }

        public bool DockCarrier()
        {
            if (Device != null)
            {
                return Device.DockCarrier();
            }

            return false;
        }
        public bool UndockCarrier()
        {
            if (Device != null)
            {
                return Device.UndockCarrier();
            }

            return false;
        }

        public bool GetPlacement()
        {
            if (Device != null)
            {
                return Device.GetPlacement();
            }

            return false;
        }
        public bool GetPresent()
        {
            if (Device != null)
            {
                return Device.GetPresent();
            }

            return false;
        }

        public bool LoadCarrier()
        {
            if (Device != null)
            {

                if(Device is DeviceLoadPortTIS)
                {
                    AppLogger.Debug()($"[{UnitName}] unit LoadCarrier Starting - real");

                    var real_device = Device as DeviceLoadPortTIS;

                    if (!real_device.ClearError())
                    {
                        AppLogger.Error()($"[{UnitName}] unit LoadCarrier failed. ClearError Command failed.");
                        return false;
                    }

                    var command_result = Device.UserCommand("RLMP");
                    if(command_result != true)
                    {
                        AppLogger.Error()($"[{UnitName}] unit LoadCarrier failed. [RLMP] Command failed.");
                        return false;
                    }


                    var info = Device.GetPortInfo();

                    info.ClampState = ClampStates.Clamped;
                    info.DockState = DockStates.Docked;
                    info.DoorState = DoorStates.Opened;

                    var result = Device.MapCarrier();

                    info.SlotMap = new ObservableCollection<CarrierSlot>(result.SlotMap);

                    (Device as DeviceLoadPortTIS).SetLoadPortInfo(info);

                    if (result == null)
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    AppLogger.Debug()($"[{UnitName}] unit LoadCarrier Starting - hws");
                    var info = Device.GetPortInfo();
                    if (info == null)
                    {
                        AppLogger.Error()("LoadCarrier Failed. GetPortInfo() is null.");
                        return false;
                    }


                    if (info.CarrierPlacement != true ||
                        info.CarrierPresence != true)
                    {
                        AppLogger.Error()("Carrier Placement is Error. [{0}]", info.ToShortString());
                        return false;
                    }

                    bool b = false;

                    if (info.ClampState != ClampStates.Clamped)
                    {
                        b = ClampCarrier();

                        if (b == false)
                        {
                            return false;
                        }
                    }

                    if (info.DockState != DockStates.Docked)
                    {
                        b = DockCarrier();
                        if (b == false)
                        {
                            return false;
                        }
                    }

                    if (info.DoorState != DoorStates.Opened)
                    {
                        b = OpenDoor();
                        if (b == false)
                        {
                            return false;
                        }
                    }


                    var result = Device.MapCarrier();


                    info.SlotMap = new ObservableCollection<CarrierSlot>(result.SlotMap);


                    if (result == null)
                    {
                        return false;
                    }


                }
                return true;

            }

            return false;
        }

        public bool UnloadCarrier()
        { 
            if(Device != null)
            {
                if(Device is DeviceLoadPortTIS)
                {

                    var real_device = Device as DeviceLoadPortTIS;

                    if(!real_device.ClearError())
                    {
                        AppLogger.Error()($"[{UnitName}] unit LoadCarrier failed. ClearError Command failed.");
                        return false;
                    }


                    AppLogger.Debug()($"[{UnitName}] unit UnloadCarrier Starting. - real");

                    var command_result = Device.UserCommand("RUNP");
                    if (command_result != true)
                    {
                        AppLogger.Error()($"[{UnitName}] unit LoadCarrier failed. [RUNP] Command failed.");
                        return false;
                    }

                    var info = Device.GetPortInfo();

                    info.ClampState = ClampStates.Unclamped;
                    info.DockState = DockStates.Undocked;
                    info.DoorState = DoorStates.Closed;

                    (Device as DeviceLoadPortTIS).SetLoadPortInfo(info);

                    AppLogger.Info()("UnloadCarrier successfully completed.");
                }
                else
                {

                    AppLogger.Debug()($"[{UnitName}] unit LoadCarrier Starting. - hws");

                    var info = Device.GetPortInfo();
                    if (info == null)
                    {
                        AppLogger.Error()("UnloadCarrier Failed. GetPortInfo() is null.");
                        return false;
                    }

                    bool b = false;
                    if (info.DoorState == DoorStates.Opened)
                    {
                        b = CloseDoor();
                        if (b != true)
                        {
                            return false;
                        }
                    }

                    if (info.DockState == DockStates.Docked)
                    {
                        b = UndockCarrier();
                        if (b != true)
                        {
                            return false;
                        }
                    }

                    if (info.ClampState == ClampStates.Clamped)
                    {
                        b = UnclampCarrier();
                        if (b != true)
                        {
                            return false;
                        }
                    }

                    AppLogger.Info()("UnloadCarrier successfully completed.");


                }
                return true;
            }

            return false;
        }

        public LoadPortInfo GetLoadPortInfo()
        {
            if(Device != null)
            {
                return Device.GetPortInfo();
            }
            return null;
        }


        public TISLoadportInputs GetInputStatus(int ch)
        {
            if (Device != null)
            {
                var d = (Device as DeviceLoadPortTIS);
                return d.GetInputStatus(ch);
            }
            return null;
        }

        public TISLoadportInputs GetInputPorts()
        {
            if(Device != null)
            {
                var d = (Device as DeviceLoadPortTIS);
                return d.GetInputPorts();
            }
            return null;
        }


        public bool Homing()
        {
            if(Device != null)
            {
                return Device.Homing();
            }

            return false;
        }

        public bool UserCommand(string command)
        {
            if (Device != null)
            {
                return Device.UserCommand(command);
            }

            return false;
        }
        #endregion

        #region constructors
        public UnitLoadPort(UnitIDs unitids)
            : base(unitids)
        {
            //lock_obj = new object();

            //_read_size = 1;
            //_write_size = 1;

            //_read_start_address = string.Empty;
            //_write_start_address = string.Empty;
        }

        #endregion

    }
}
