using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Project.BaseLib.HW;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using Project.BaseLib.DataStructures;

namespace Project.HWC
{
    public class UnitPrealigner : UnitTyped
    {
        #region fields
        
        #endregion

        #region propertise
        public IDevicePrealigner Device
        {
            get { return base.BaseDevice as IDevicePrealigner; }

            set
            {
                base.BaseDevice = value;
            }
        }

        new public PreAlignerConfig Config
        {
            get
            {
                return (base.Config as PreAlignerConfig);
            }
        }
        #endregion

        #region methods
        public override IDevice CreateDevice()
        {
            IDevice device = null;

            if (DeviceType == DeviceTypes.PreAligner_TIS)
            {
                device = new DevicePrealignerTIS(Config as NetworkConfig);
            }
            else
            {
                var d = new DevicePrealignerSim();
                d.Config = Config;
                device = d;
            }

            return device;
        }

        public override bool Initialize()
        {
            if (Device == null)
            {
                AppLogger.Error()($"[{UnitName}] unit is not alloced. Initialize failed.");
                return false;
            }

            if (Device.Homing() != true)
            {
                AppLogger.Error()($"{UnitName} initialize failed.");
                return false;
            }

            AppLogger.Info()($"{UnitName} initialize Success.");
            return true;
        }

        public bool SetWaferType(SubstrateSizes size, ContourType type, bool force)
        {
            if (Device == null)
            {
                AppLogger.Error()($"[{UnitName}] unit is not alloced. SetWaferType failed.");
                return false;
            }

            return Device.SetWaferType(size, type);
        }

        public bool InitAligner(SubstrateSizes size, ContourType type)
        {
            try
            {
                AppLogger.Debug()("InitAligner starting... {0},{1}", size, type);

                //
                var os = SetWaferType(size, type, false);
                if (os == false)
                {
                    return os;
                }

                //
                os = ResetPosition();
                if (os == false)
                {
                    return os;
                }

                AppLogger.Debug()("InitAlign successfully completed.");

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("InitAlign exception failed. {0}", e.ToString());
                return false;
            }
        }

        public bool InitAngle(SubstrateOrientations direction)
        {
            try
            {

                AppLogger.Debug()("InitAngle starting... {0}:{1}", direction, 0.0);

                var os = Device.InitAngle(direction, 0.0);
                if (os == false)
                {
                    AppLogger.Error()("InitAngle failed.");
                    return false;
                }

                AppLogger.Debug()("InitAngle successfully completed. {0}", os.ToString());

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("InitAngle exception failed. {0}", e.ToString());
                return false;
            }
        }

        public bool ResetPosition()
        {
            try
            {
                if (Device == null)
                {
                    AppLogger.Error()($"[{UnitName}] unit is not alloced. ResetPosition failed.");
                    return false;
                }


                var r = Device.ResetPosition();
                if (r == false)
                {
                    AppLogger.Error()("ResetPosition failed");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("ResetPosition exception failed. {0}", e.ToString());
                return false;
            }
        }

        public PrealignerResult Align()
        {
            try
            {
                AppLogger.Debug()("Align starting...");

                if (Device == null)
                {
                    AppLogger.Error()($"[{UnitName}] unit is not alloced. Align failed.");
                    return null;
                }


                PrealignerResult osr = Device.Align();
                if (osr == null)
                {
                    AppLogger.Error()("Align failed");
                    return null;
                }


                AppLogger.Debug()("Align successfully completed. {0}", osr.ToString());
                return osr;
            }
            catch (Exception e)
            {
                AppLogger.Error()("Align exception failed. {0}", e.ToString());
                return null;
            }
        }

        public bool HomePosition()
        {
            try
            {
                var op = Device.HomePosition();
                if (op == false)
                {
                    AppLogger.Error()("Homing failed");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("HomePosition exception failed. {0}", e.ToString());
                return false;
            }
        }

        public SubstratePresences PresenceSubstrate()
        {
            try
            {
                if (Device == null)
                {
                    AppLogger.Error()($"[{UnitName}] unit is not alloced. PresenceSubstrate failed.");
                    return SubstratePresences.Unknown;
                }
                var r = Device.PresenceSubstrate();
                AppLogger.Debug()($"SubstratePresences : [{r.ToString()}].");
                return r;
            }
            catch (Exception e)
            {
                AppLogger.Error()("PresenceSubstrate exception failed. {0}", e.ToString());
                return SubstratePresences.Unknown;
            }
        }

        public bool Homing()
        {
            if(Device != null)
            {
                return Device.Homing();
            }
            return false;
        }
        #endregion

        #region constructors
        public UnitPrealigner(UnitIDs unitids)
            : base(unitids)
        {

        }

        #endregion

    }
}
