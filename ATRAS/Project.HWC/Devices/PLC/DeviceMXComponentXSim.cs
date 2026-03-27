using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class DeviceMXComponentXSim : DeviceBase, IDeviceMxComponentX64
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        public bool BReceive(int nStart, int size, ref short[] data)
        {
            //AppLogger.Info()("[{0}] device BReceive is empty.", Name);
            data = null;

            return true;
        }
        public bool BReceive(string device, int size, ref short[] data)
        {
            //AppLogger.Info()("[{0}] device BReceive is empty.", Name);
            data = null;

            return true;
        }
        public bool BSend(int nstartadd, int size, ref short[] data)
        {
            AppLogger.Info()("[{0}] device BSend is empty.", Name);
            data = null;

            return true;
        }
        public bool BSend(string device, int size, ref short[] data)
        {
            AppLogger.Info()("[{0}] device BSend is empty.", Name);

            return true;
        }

        public override bool Connect()
        {
            AppLogger.Info()("[{0}] device connect is empty.", Name);

            return true;

        }

        public override bool Disconnect()
        {
            AppLogger.Info()("[{0}] device disconnect is empty.", Name);

            return true;
        }
        #endregion

        #region constructors
        public DeviceMXComponentXSim() 
            : base(DeviceTypes.HWS)
        {

        }


        #endregion

    }
}
