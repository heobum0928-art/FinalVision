using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    // Device Event
    public delegate void DeviceConnected();
    public delegate void DeviceDisconnected();


    // Unit Event
    public delegate void HWInitializationStateChanged(InitializationStates initializationState);

    public delegate void UnitInitializationStateChanged(string _UnitType, InitializationStates _State);
    
    public delegate void UnitStatusChangedEvent(UnitBaseStatus status);



}
