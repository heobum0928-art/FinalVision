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
    public class DeviceMXComponentX64 : DeviceBase, IDeviceMxComponentX64
    {
        #region fields
        protected int _LogicalStationNumber;

        private ActUtlType64Lib.ActUtlType64Class plcCom = null;

        #endregion

        #region propertise
        public int LogicalStationNumber
        {
            get { return _LogicalStationNumber; }

            set { _LogicalStationNumber = value; }
        }

        public bool IsConnected
        {
            get { return plcCom == null ? false : true; }
        }
        #endregion

        #region methods
        public override bool Connect()
        {
            try
            {
                if(IsConnected)
                {
                    if (!Disconnect())
                    {
                        AppLogger.Error()("[{0}] device disconnect failed", Name);
                        return false;
                    }
                }

                plcCom = new ActUtlType64Lib.ActUtlType64Class();
                plcCom.ActLogicalStationNumber = _LogicalStationNumber;
                AppLogger.Info()("[{0}] device LogicalStationNumber is [{1}].", Name, _LogicalStationNumber);

                int return_value = plcCom.Open();
                if(return_value == 0)
                {
                    AppLogger.Info()("[{0}] device open success.", Name);
                    return true;
                }

                plcCom = null;
                AppLogger.Error()("[{0}] device open failed", Name);
                return false;
            }
            catch (Exception ex)
            {
                AppLogger.Error()("[{0}] device open failed. Message : [{1}]", Name, ex.Message);
                return false;
            }


        }
        public override bool Disconnect()
        {
            if(plcCom != null)
            {
                var result = plcCom.Close();
                if(result == 0)
                {
                    plcCom = null;
                    return true;
                }
            }

            return false;
        }
        public bool BReceive(int nStart, int size, ref short [] data)
        {
            string strDevice = "W" + nStart.ToString("x").ToUpper();// string.Format("W%x", nstartadd);

            return BReceive(strDevice, size, ref data);
        }

        public bool BReceive(string device, int size, ref short[] data)
        {
            try
            {
                if (!IsConnected)
                {
                    AppLogger.Error()("[{0}] device BReceive failed. plcCom is Null.", Name);
                    return false;
                }


                if (data == null || (data.Length < size))
                {
                    AppLogger.Error()("[{0}] device BReceive failed. Data is too short. size : {1}, data.Length : {2}", 
                        Name, size, data.Length);
                    return false;
                }

                
                var returnCode = plcCom.ReadDeviceBlock2(device, size, out data[0]);

                if (returnCode != 0)
                {
                    AppLogger.Error()("[{0}] device BReceive failed", Name);
                    return false;
                }

                //AppLogger.Info()("[{0}] device BReceive start : {1}, size : {2},  data : [ {3} ]",
                //    Name, device, size, string.Join(", ", data));

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("[{0}] device BReceive failed. Message : {1}", Name, e.Message);
                return false;
            }
        }
        public bool BSend(int nstartadd, int size, ref short[] data)
        {
            string strDevice = "W" + nstartadd.ToString("x").ToUpper();// string.Format("W%x", nstartadd);

            return BSend(strDevice, size, ref data);
        }
        public bool BSend(string device, int size, ref short[] data)
        {
            try
            {

                if (data == null)// || (data.Length < size))
                {
                    AppLogger.Error()("[{0}] device data is null", Name);
                    return false;
                }

                if (data.Length < size)
                {
                    AppLogger.Error()("[{0}] device data size is too short. data.length : [{1}], size : {2}",
                        Name, data.Length, size);

                    return false;
                }

                var returnCode = plcCom.WriteDeviceBlock2(device,
                                      size,
                                      ref data[0]);

                if (returnCode != 0)
                {
                    AppLogger.Error()("[{0}] device Block Send failed", Name);
                    return false;
                }

                //AppLogger.Info()("[{0}] device data Block Send success.", Name);

                return true;
            }
            catch (Exception e)
            {
                data = null;
                AppLogger.Error()("[{0}] device Block Send failed. Message : {1}", Name, e.Message);

                return false;
            }

        }
        #endregion

        #region constructors
        public DeviceMXComponentX64() 
            : base(DeviceTypes.MXComponent_X64)
        {
 
        }

        #endregion

    }
}
