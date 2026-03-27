using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public abstract class UnitTyped : UnitBase
    {
        #region fields

        #endregion

        #region propertise
        public HWModes HWMode
        {
            get
            {
                if (Config.DeviceType == DeviceTypes.HWS)
                {
                    return HWModes.Simulator;
                }
                else
                {
                    return HWModes.Real;
                }
            }
        }



        public new DeviceTypeConfig Config 
        {
            get
            {
                return (_Config as DeviceTypeConfig);
            }
            set
            {
                _Config = value;
            }
        }
        public DeviceTypes DeviceType
        {
            get
            {
                if (_Config == null)
                {
                    return DeviceTypes.NotUsed;
                }
                else
                {

                    return (_Config as DeviceTypeConfig).DeviceType;
                }
            }
        }
        #endregion

        #region methods
        public override async Task<bool> SWInitialize(HWConfigBase hwConfigBase)
        {
            _Config = hwConfigBase as DeviceTypeConfig;

            if (Config.DeviceType == DeviceTypes.NotUsed)
            {
                AppLogger.Info()("Initialize(SW) is skip. [{0}] device is NotUsed.", UnitID);
                return true;
            }


            var forceInit = !Config.Equals(hwConfigBase);

            BaseStatus.DeviceType = Config.DeviceType;

            BaseDevice = CreateDevice();
            if (BaseDevice == null)
            {
                AppLogger.Error()("SetHWUnitConfiguration Failed {0} (Device is null)", UnitName);
                return false;
            }
            //
            InitializationState = InitializationStates.Ready;

            var result = await HWInitialize(forceInit);

            if (result)
            {
                AppLogger.Info()("Initialize(SW) Completed {0} ({1}:{2})", UnitID, HWMode, BaseDevice.Name);
                return true;
            }

            AppLogger.Error()("Initialize(SW) failed {0} ({1}:{2})", UnitID, HWMode, BaseDevice.Name);
            return false;
        }
        public override async Task<bool> HWInitialize(bool forceInitialization)
        {
            if(Config == null)
            {
                AppLogger.Error()($"Initialize(HW) {UnitName}'s Device is not alloced. SWInitialize is not doing.");
                InitializationState = InitializationStates.NotReady;
                return false;
            }
            if (Config.DeviceType == DeviceTypes.NotUsed)
            {
                AppLogger.Info()("Initialize(HW) " + BaseDevice.Name + " Skip (NotUsed)");
                //
                InitializationState = InitializationStates.Initialized;

                return true;
            }

            if (InitializationState == InitializationStates.Initialized && !forceInitialization)
            {
                AppLogger.Info()("HWInitialize already done. {0} ({1} : {2})", UnitID, HWMode, BaseDevice.Name);
                return true;
            }

            BaseStatus.InitializationState = InitializationStates.Ready;

            AppLogger.Debug()("Initialize(HW) Starting of {0} ({1} : {2})", UnitID, HWMode, BaseDevice.Name);
            var os = Connect();
            if (os == false)
            {
                InitializationState = InitializationStates.NotReady;
                AppLogger.Error()("Initialize(HW) Failed {0}", UnitID);
                return false;
            }

            os = await InitDevice();
            if (os == false)
            {
                InitializationState = InitializationStates.NotReady;

                AppLogger.Error()("Initialize(HW) Failed {0}", UnitID);
                return false;
            }

            //
            InitializationState = InitializationStates.Initialized;

            //
            AppLogger.Info()("Initialize(HW) Completed {0}", UnitID);

            return true;
        }


        #endregion

        #region constructors
        public UnitTyped(UnitIDs unitID)
            : base(unitID)
        {

        }
        #endregion

    }
}
