using Project.BaseLib.DataStructures;
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
    public class DeviceEFEMIOSim : DeviceBase, IDeviceEFEMIO
    {
        #region fields
        protected string _IPAddress;

        protected int _Port;
        #endregion

        #region propertise
        public string IPAddress
        {
            get
            {
                return _IPAddress;
            }
        }

        public int Port
        {
            get
            {
                return _Port;
            }
        }
        #endregion

        #region methods
        public override bool Connect()
        {
            AppLogger.Info()($"[{Name}] device is connected. IPAddress : {_IPAddress}, Port : {_Port}");
            return true;
        }

        public override bool Disconnect()
        {
            AppLogger.Info()($"[{Name}] device is disconnected.");
            return true;
        }

        public bool SetDigitalOut(int ch, int no, IOStates state)
        {
            AppLogger.Info()($"[{Name}] device SetDigitalOut({ch}, {no}, {state}");


            return true;
        }

        public string SetUserCommand(string command)
        {
            AppLogger.Info()($"[{Name}] device SetUserCommand(>> {command})");


            return string.Format("<< simulator response"); ;
        }

        public byte[] GetDigitalInputs(int ch)
        {
            byte[] bytes = new byte[2];

            Array.Clear(bytes, 0, 2);

            //AppLogger.Debug()($"[{DeviceType}] device GetDigitalInputs({ch}) is On.");
            return bytes;
        }

        public IOStates GetDigitalInput(int ch, int no)
        {
            //AppLogger.Debug()($"[{DeviceType}] device GetDigitalInput({ch}, {no}) is On.");
            return IOStates.On;
        }

        public byte[] GetDigitalOutputs(int ch)
        {
            byte[] bytes = new byte[2];

            Array.Clear(bytes, 0, 2);

            //AppLogger.Debug()($"[{DeviceType}] device GetDigitalOutputs({ch}) is On.");
            return bytes;
        }

        public IOStates GetDigitalOutput(int ch, int no)
        {
            //AppLogger.Debug()($"[{DeviceType}] device GetDigitalOutput({ch}, {no}) is On.");
            return IOStates.On;
        }

        public byte[] GetAllDigitalInput()
        {
            byte[] bytes = new byte[12];
            Array.Clear(bytes, 0, 12);

            //AppLogger.Debug()($"[{DeviceType}] device GetAllDigitalInput() is On.");
            return bytes;
        }

        public byte[] GetAllDigitalOutput()
        {
            byte[] bytes = new byte[12];
            Array.Clear(bytes, 0, 12);

            //AppLogger.Debug()($"[{DeviceType}] device GetAllDigitalOutput() is On.");
            return bytes;
        }
        #endregion

        #region constructors
        protected DeviceEFEMIOSim()
            : base(DeviceTypes.HWS)
        {

        }
        public DeviceEFEMIOSim(string ipaddress, int port)
            : base(DeviceTypes.HWS)
        {
            _IPAddress = ipaddress;
            _Port = port;
        }
        #endregion

    }
}
