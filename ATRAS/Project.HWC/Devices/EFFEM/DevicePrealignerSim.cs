using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;

namespace Project.HWC
{
    public class DevicePrealignerSim : DeviceBase, IDevicePrealigner
    {
        #region fields
        private const byte delemeterCR = 0x0d;
        private const byte delemeterLF = 0x0a;
        #endregion

        #region propertise
        public PreAlignerConfig Config { get; set; }
        #endregion

        #region methods
        public PrealignerResult Align()
        {
            int time = Config.AlignTime;
            PrealignerResult result = new PrealignerResult();

            AppLogger.Info()($"{Name} device Align. Result is {result.ToString()}. [{time}] ms");
            return result;
        }

        public PrealignerResult GetAlignResult()
        {
            PrealignerResult result = new PrealignerResult();


            AppLogger.Info()($"{Name} device get align result : {result.ToString()}");
            return result;
        }

        public bool HomePosition()
        {
            AppLogger.Info()($"{Name} device set home position.");
            return true;
        }

        public bool Homing()
        {
            AppLogger.Info()($"{Name} device homing.");

            Thread.Sleep(1000);
            return true;
        }

        public bool InitAngle(SubstrateOrientations direction, double angleOffset)
        {
            int time = Config.AlignTime;
            AppLogger.Info()($"{Name} device InitAngle : Orientation : {direction}, offset : {angleOffset}");
            return true;
        }

        public SubstratePresences PresenceSubstrate()
        {
            SubstratePresences sp = SubstratePresences.Unknown;
            AppLogger.Info()($"{Name} device PresenceSubstrate() : {sp}");

            return sp;
        }

        public bool ResetPosition()
        {
            AppLogger.Info()($"{Name} device ResetPosition()");
            return true;
        }

        public bool SetWaferType(SubstrateSizes size, ContourType type)
        {
            AppLogger.Info()($"{Name} device SetWafer Type : {size} / {type}");

            return true;
        }

        public override bool Connect()
        {
            AppLogger.Info()($"Connect() - [{Name} device is Connect");
            return true;
        }

        public override bool Disconnect()
        {
            AppLogger.Info()($"Disconnect() - [{Name} device is Disconnect");
            return true;
        }

        public bool UserCommand(string command)
        {
            AppLogger.Debug()($"[{Name}] device user command : >> {command + Convert.ToChar(delemeterCR) + Convert.ToChar(delemeterLF)}");
            return true;

        }
        #endregion

        #region constructors
        public DevicePrealignerSim() : base(DeviceTypes.HWS)
        {
        }


        #endregion


    }
}
