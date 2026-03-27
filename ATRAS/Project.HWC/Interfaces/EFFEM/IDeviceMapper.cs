using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.DataStructures;
using Project.BaseLib.HW;

namespace Project.HWC
{
    public interface IDeviceMapper : IDevice
    {
        SlotMapResult MapCarrier(int portNumber);
    }
}
