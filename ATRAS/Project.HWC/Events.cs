using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    // all unit
    public delegate void NotifyUnitStatusChanged(UnitBaseStatus status);

    // Carreir
    public delegate void NotifyBrooksEvent(int deviceCode, string[] commands);
    //
    public delegate void NotifyDeviceEvent(int portId, EventCodes code, object data);

    ////CarrierData
    //public delegate void ChangedLoadPortCarrierData(LoadPortCarrierInfo info);

    // Substrate
    public delegate void NotifySubstrateLocations(SubstratePresences robotArmA, SubstratePresences robotArmB, SubstratePresences prealigner);

}
