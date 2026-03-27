using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;

namespace Project.HWC
{
    public interface IDeviceLoadPort : IDevice
    {
        void SetExternalMapper(IDeviceMapper IDevice);

        bool Homing();

        LoadPortInfo GetPortInfo();

        bool GetPresent();

        bool GetPlacement();

        ClampStates GetClampState();
        DockStates GetDockState();
        DoorStates GetDoorState();

        void SetSlotCount(int slotCount);


        bool ClampCarrier();
        bool UnclampCarrier();
        bool DockCarrier();
        bool UndockCarrier();
        bool OpenDoor();
        bool CloseDoor();
        SlotMapResult MapCarrier();

        bool HWInitialize();

        void SetLightStates(List<LoadPortLightState> loadPortLightsMapping);


        bool UserCommand(string command);

    }
}
