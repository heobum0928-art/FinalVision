using Project.BaseLib.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public interface IDeviceMxComponentX64 : IDevice
    {
        #region propertise

        #endregion

        #region methods
        bool BReceive(int nStart, int size, ref short[] data);
        bool BReceive(string device, int size, ref short[] data);
        bool BSend(int nstartadd, int size, ref short[] data);
        bool BSend(string device, int size, ref short[] data);

        #endregion

    }
}
