using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public abstract class UnitBase : IUnit
    {
        #region fields
        public event UnitInitializationStateChanged _UnitInitializationStateChanged;

        public event UnitStatusChangedEvent _UnitStatusChangedEvent;

        //protected ILogger logger = null;

        protected string _UnitName = "Unknown";

        protected HWConfigBase _Config = null;

        protected UnitBaseStatus basestatus;

        private IDevice basedevice = null;

        protected bool abort = false;

        private UnitIDs unitID = UnitIDs.Unknown;

        #endregion

        #region propertise

        public UnitIDs UnitID
        {
            get { return unitID; }
            set { unitID = value; }
        }
        public HWConfigBase Config
        {
            get
            {
                return _Config;
            }
            set
            {
                _Config = value;
            }
        }
        public IDevice BaseDevice
        {
            get 
            { 
                return basedevice; 
            }

            set { basedevice = value; }
        }
        public InitializationStates InitializationState
        {
            get { return basestatus.InitializationState; }

            set { SetHWInitializationState(value); }
        }
        public UnitBaseStatus BaseStatus
        {
            get
            {
                if(basestatus == null)
                {
                    basestatus = new UnitBaseStatus();
                }

                if(BaseDevice == null)
                {
                    basestatus.UnitOnline = UnitOnline.Offline;
                }
                else
                {
                    basestatus.UnitOnline = BaseDevice.Online();
                }

                return basestatus;
            }

            set
            {
                basestatus = value;
            }
        }
        public UnitOnline Online
        {
            get
            {
                if (BaseDevice == null)
                {
                    return UnitOnline.Offline;
                }

                BaseStatus.UnitOnline = BaseDevice.Online();
                return basestatus.UnitOnline;
            }
            set
            {
                if (BaseStatus.UnitOnline != value)
                {
                    BaseStatus.UnitOnline = value;
                    NotifyUnitStatus();
                }
            }
        }
        public string UnitName
        {
            get { return unitID.ToString(); }
        }
        public bool Initialized
        {
            get { return (InitializationState == InitializationStates.Initialized); }
        }

        public bool IsAbort
        {
            get
            {
                if (abort)
                {
                    abort = false;
                    return true;
                }
                return false;
            }
            set
            {
                abort = value;
            }
        }
        #endregion

        #region methods
        public abstract IDevice CreateDevice();
        public virtual async Task<bool> InitDevice()
        {
            return await Task.Run(() =>
            {
                return Initialize();
            });
        }
        public abstract bool Initialize();
        public virtual void OnDeviceConnected()
        {
            if (InitializationState == InitializationStates.NotReady)
            {
                AppLogger.Info()("{0} Device Connected", UnitName);
                Online = UnitOnline.Online;
                InitializationState = InitializationStates.Ready;
            }
        }
        public virtual void OnDeviceDisconnected()
        {
            if (InitializationState != InitializationStates.NotReady)
            {
                AppLogger.Error()("{0} Device Disconnected", UnitName);
                Online = UnitOnline.Offline;
                InitializationState = InitializationStates.NotReady;
            }
        }
        public virtual void RegisterDeviceEvents()
        {
            BaseDevice.RegisterConnetedEvent(OnDeviceConnected, OnDeviceDisconnected);
        }
        public virtual void RegisterEvent(UnitInitializationStateChanged _UnitInitializationStateChanged)
        {
            this._UnitInitializationStateChanged += _UnitInitializationStateChanged;
        }
        public virtual void RegisterEvent(UnitStatusChangedEvent unitStatusChangedEvent)
        {
            this._UnitStatusChangedEvent += _UnitStatusChangedEvent;
        }
        public virtual void SetHWInitializationState(InitializationStates value)
        {
            if (BaseStatus.InitializationState != value)
            {
                // logger.Info()("InitializationState = " + value.ToString());
                BaseStatus.InitializationState = value;

                if (_UnitInitializationStateChanged != null)
                {
                    _UnitInitializationStateChanged(UnitName, BaseStatus.InitializationState);
                }
            }
        }
        public void NotifyUnitStatus()
        {
            if (_UnitStatusChangedEvent != null)
            {
                _UnitStatusChangedEvent(BaseStatus);
            }
        }
        public virtual void Abort()
        {
            if(BaseDevice != null)
            {
                BaseDevice.Abort();
            }

            abort = true;
        }
        public virtual void Reset()
        {
            
        }
        public virtual void Shutdown()
        {
            if(BaseDevice != null)
            {
                BaseDevice.ClearRegiterEvents();
                BaseDevice.Shutdown();
            }
            AppLogger.Debug()("Shutdown {0}", unitID.ToString());
        }

        public abstract Task<bool> SWInitialize(HWConfigBase hwConfigBase);
        public abstract Task<bool> HWInitialize(bool forceInitialization);



        public bool WaitForConnection(int waitTime)
        {
            int endTicks = (System.Environment.TickCount + waitTime);
            while (true)
            {
                if (Online == UnitOnline.Online) break;

                System.Threading.Thread.Sleep(1000);
                if (endTicks < System.Environment.TickCount) break;
            }

            if (Online != UnitOnline.Online)
            {
                AppLogger.Error()("[{0}] Unit time out [{1}] ms", unitID, waitTime);
                return false;
            }

            return true;
        }
        public virtual bool ConnectDevice()
        {
            return BaseDevice.Connect();
        }
        public bool Connect()
        {
            return ConnectDevice();
        }
        #endregion

        #region constructors
        public UnitBase(UnitIDs unitID)
        {
            this.unitID = unitID;
        }
        #endregion
    }

}
