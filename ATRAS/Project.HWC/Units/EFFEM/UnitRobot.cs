using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.HW;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;

namespace Project.HWC
{
    public class UnitRobot : UnitTyped
    {
        #region fields

        #endregion

        #region propertise
        public IDeviceRobot Device
        {
            get { return base.BaseDevice as IDeviceRobot; }

            set
            {
                base.BaseDevice = value;
            }
        }

        new public WTRConfig Config
        {
            get
            {
                return (base.Config as WTRConfig);
            }
        }
        #endregion

        #region methods
        public override IDevice CreateDevice()
        {
            IDevice device = null;

            if (DeviceType == DeviceTypes.WTR_TIS)
            {
                device = new DeviceRobotTIS(Config as NetworkConfig);
            }
            else
            {
                var d = new DeviceRobotSim();
                d.Config = Config;
                device = d;
            }

            return device;
        }

        public override bool Initialize()
        {
            if(Device != null)
            {
                var os = Device.HWInitialize();

                //os = Homing();
                if (os == false)
                {
                    AppLogger.Error()($"[{UnitName} unit hw initialize failed.");
                    return false; // new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "Homing failed."));
                }


                os = SetRobotSpeed(Config.RobotSpeed);
                if (os == false)
                {
                    AppLogger.Error()($"[{UnitName} unit set robot speed failed.");
                    return false; // new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "Homing failed."));
                }

                var arm1_presence = PresenceSubstrate(RobotArmIDs.Arm1);
                AppLogger.Info()($"[{UnitName}] unit arm1 substrate is {arm1_presence}");

                var arm2_presence = PresenceSubstrate(RobotArmIDs.Arm2);
                AppLogger.Info()($"[{UnitName}] unit arm2 substrate is {arm2_presence}");


                AppLogger.Info()($"{UnitName} Initialize Success.");
                return true;
            }

            AppLogger.Error()($"[{UnitName}] unit initialize failed.");

            return false;
        }
        public SubstratePresences PresenceSubstrate(RobotArmIDs armID)
        {
            if(Device != null)
            {
                return Device.PresenceSubstrate(armID);
            }

            return SubstratePresences.Unknown;
        }

        public bool SetRobotSpeed(RobotSpeeds speed)
        {
            if(Device != null)
            {
                return Device.SetRobotSpeed(speed);
            }

            return false;
        }

        // Get
        public bool PreparePickSubstrate(RobotArmIDs armID, int station)
        {
            if(Device != null)
            {
                return Device.PreparePickSubstrate(armID, station, Config.WaferSize);
            }

            return false;
        }
        public bool PreparePickSubstrate(RobotArmIDs armID, int portId, int slotId)
        {
            if(Device != null)
            {
                return Device.PreparePickSubstrate(armID, portId, slotId, Config.WaferSize);
            }

            return false;
        }
        public bool PreparePickSubstrate(int arm_number, int portId, int slotId)
        {
            if (Device != null)
            {
                return Device.PreparePickSubstrate(arm_number, portId, slotId, Config.WaferSize);
            }

            return false;
        }

        //
        public bool PickSubstrate(RobotArmIDs armID, int station)
        {
            if(Device != null)
            {
                return Device.PickSubstrate(armID, station, Config.WaferSize);
            }

            return false;
        }
        public bool PickSubstrate(RobotArmIDs armID, int portId, int slotId)
        {
            if (Device != null)
            {
                return Device.PickSubstrate(armID, portId, slotId, Config.WaferSize);
            }

            return false;
        }
        public bool PickSubstrate(int arm_number, int portId, int slotId)
        {
            if (Device != null)
            {
                return Device.PickSubstrate(arm_number, portId, slotId, Config.WaferSize);
            }

            return false;
        }

        // Put
        public bool PreparePlacedSubstrate(RobotArmIDs armID, int station)
        {
            if (Device != null)
            {
                return Device.PreparePlacedSubstrate(armID, station, Config.WaferSize);
            }

            return false;
        }
        public bool PreparePlacedSubstrate(RobotArmIDs armID, int portId, int slotId)
        {

            if (Device != null)
            {
                return Device.PreparePlacedSubstrate(armID, portId, slotId, Config.WaferSize);
            }

            return false;
        }
        public bool PreparePlacedSubstrate(int arm_number, int portId, int slotId)
        {

            if (Device != null)
            {
                return Device.PreparePlacedSubstrate(arm_number, portId, slotId, Config.WaferSize);
            }

            return false;
        }

        public bool PlacedSubstrate(RobotArmIDs armID, int station)
        {
            if(Device != null)
            {
                //var robot_config = (Config as WTRConfig);
                return Device.PlacedSubstrate(armID, station, Config.WaferSize);
            }

            return false;
        }
        public bool PlacedSubstrate(RobotArmIDs armID, int portId, int slotId)
        {
            if (Device != null)
            {
                return Device.PlacedSubstrate(armID, portId, slotId, Config.WaferSize);
            }
            return false;
        }
        public bool PlacedSubstrate(int arm_number, int portId, int slotId)
        {
            if (Device != null)
            {
                return Device.PlacedSubstrate(arm_number, portId, slotId, Config.WaferSize);
            }
            return false;
        }


        //
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
            if(Device != null)
            {
                return Device.UserCommand(command);
            }

            return false;
        }
        #endregion

        #region constructors
        public UnitRobot(UnitIDs unitids)
            : base(unitids)
        {
        }
        #endregion

    }
}
