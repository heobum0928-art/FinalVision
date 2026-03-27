using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public interface IDevicePrealigner : IDevice
    {
        //
        bool Homing();

        //
        bool SetWaferType(SubstrateSizes size, ContourType type);
        bool InitAngle(SubstrateOrientations direction, double angleOffset);

        //
        bool HomePosition();
        bool ResetPosition();
        PrealignerResult Align();
        PrealignerResult GetAlignResult();
        SubstratePresences PresenceSubstrate();


        bool UserCommand(string command);
    }
}
