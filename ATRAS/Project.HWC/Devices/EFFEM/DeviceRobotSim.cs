using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;

namespace Project.HWC
{
    public class DeviceRobotSim : DeviceBase, IDeviceRobot
    {
        #region fields
        private const byte delemeterCR = 0x0d;
        private const byte delemeterLF = 0x0a;
        #endregion

        #region propertise
        public WTRConfig Config { get; set; }
        #endregion

        #region methods

        public int GetStationNumber(RobotStations station, SubstrateSizes waferSize)
        {
            switch (waferSize)
            {
                case SubstrateSizes.ThreeHundredMM:
                    switch (station)
                    {
                        case RobotStations.LoadPort1: return 1;
                        case RobotStations.LoadPort2: return 2;
                        case RobotStations.Prealigner1: return 9;
                        case RobotStations.Chamber1: return 10;
                    }
                    break;
                case SubstrateSizes.TwoHundredMM:
                    switch (station)
                    {
                        case RobotStations.LoadPort1: return 3;
                        case RobotStations.LoadPort2: return 4;
                        case RobotStations.Prealigner1: return 11;
                        case RobotStations.Chamber1: return 12;
                    }
                    break;
                case SubstrateSizes.FourHundredFiftyMM:
                    break;
            }

            return 0;

        }

        public bool Homing()
        {
            AppLogger.Info()($"{Name} device robot homing.");

            Thread.Sleep(1000);
            return true;
        }

        public SubstratePresences PresenceSubstrate(RobotArmIDs armID)
        {
            SubstratePresences sp = SubstratePresences.Unknown;

            AppLogger.Info()($"{Name} device presence substrate is {sp}.");
            return sp;
        }

        public SubstratePresences PresenceSubstrate(int arm_number)
        {
            SubstratePresences sp = SubstratePresences.Unknown;

            AppLogger.Info()($"{Name} device presence substrate is {sp}.");
            return sp;
        }
               
        public bool SetRobotSpeed(RobotSpeeds speed)
        {
            AppLogger.Info()($"{Name} device set robot speed. speed is {speed}");
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

        //
        public bool PreparePickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PreparePickSubstrate({armID}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PreparePickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PreparePickSubstrate({armID}, {station}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PreparePickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PreparePickSubstrate({arm_number}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        //
        public bool PickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.GetTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PickSubstrate({armID}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            int time = Config.GetTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PickSubstrate({armID}, {station}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.GetTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PickSubstrate({arm_number}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        //
        public bool PreparePlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);


            AppLogger.Info()($"PreparePlacedSubstrate({armID}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PreparePlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PreparePlacedSubstrate({armID}, {station}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PreparePlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PrepareTime;
            Thread.Sleep(time);


            AppLogger.Info()($"PreparePlacedSubstrate({arm_number}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        //
        public bool PlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PutTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PlacedSubstrate({armID}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            int time = Config.PutTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PlacedSubstrate({armID}, {station}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }

        public bool PlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            int time = Config.PutTime;
            Thread.Sleep(time);

            AppLogger.Info()($"PlacedSubstrate({arm_number}, {portId}, {slotId}, {WaferSize}) - [{Name} device], [{time}] ms");
            return true;
        }
        
        public bool UserCommand(string command)
        {
            AppLogger.Debug()($"[{Name}] device user command : >> {command + Convert.ToChar(delemeterCR) + Convert.ToChar(delemeterLF)}");
            return true;
        }

        public bool HWInitialize()
        {
            AppLogger.Debug()($"[{Name}] device hw initialize success.");
            return true;
        }
        
        #endregion

        #region constructors
        public DeviceRobotSim()
            : base(DeviceTypes.HWS)
        {
        }

        #endregion
    }
}
