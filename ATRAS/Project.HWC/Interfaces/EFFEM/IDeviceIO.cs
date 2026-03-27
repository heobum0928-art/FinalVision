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
    public interface IDeviceIO : IDevice
    {
        byte [] GetDigitalInputs(int ch);

        IOStates GetDigitalInput(int ch, int no);

        byte [] GetDigitalOutputs(int ch);

        IOStates GetDigitalOutput(int ch, int no);

        bool SetDigitalOut(int ch, int no, IOStates state);

        string SetUserCommand(string command);

        byte [] GetAllDigitalInput();

        byte[] GetAllDigitalOutput();
    }
}
