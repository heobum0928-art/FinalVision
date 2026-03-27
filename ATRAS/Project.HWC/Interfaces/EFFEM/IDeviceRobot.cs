using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;

namespace Project.HWC
{
    public interface IDeviceRobot : IDevice
    {

        //void RegisterEvent(NotifyCarrierEvent notifyRobot);

        //
        bool Homing();

        //
        SubstratePresences PresenceSubstrate(RobotArmIDs armID);
        SubstratePresences PresenceSubstrate(int arm_number);

        int GetStationNumber(RobotStations station, SubstrateSizes waferSize);

        //
        bool PreparePickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize);
        bool PreparePickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize);
        bool PreparePickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize);

        bool PickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize);
        bool PickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize);
        bool PickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize);

        //
        bool PreparePlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize);
        bool PreparePlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize);
        bool PreparePlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize);

        bool PlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize);
        bool PlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize);
        bool PlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize);




        bool SetRobotSpeed(RobotSpeeds speed);

        bool UserCommand(string command);

        bool HWInitialize();

    }
}
