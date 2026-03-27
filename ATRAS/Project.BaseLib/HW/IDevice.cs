using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public interface IDevice
    {
        #region propertise
        string Name { get; }
        #endregion

        #region methods
        UnitOnline Online();
        void RegisterConnetedEvent(DeviceConnected deviceConnected, DeviceDisconnected deviceDisconnected);
        void ClearRegiterEvents();
        bool Initialize();
        bool Connect();
        bool Disconnect();
        bool Reset();
        bool Abort();
        bool Shutdown();


        
        #endregion
    }
}
