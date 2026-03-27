using Project.BaseLib.Enums;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public abstract class DeviceBase : IDevice
    {
        #region Field
        protected event DeviceConnected deviceConnected;
        protected event DeviceDisconnected deviceDisconnected;

        //protected string name;
        protected DeviceTypes deviceType;

        //
        private bool abort = false;

        #endregion

        #region Properties

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
        public string Name
        {
            get { return deviceType.ToString(); }
        }
        public DeviceTypes DeviceType
        {
            get { return deviceType; }
        }
        #endregion

        #region constructor
        //public DeviceBase(string name)
        //{
        //    this.name = name;
        //    logger = LogManager.GetLogger(name);
        //}

        public DeviceBase(DeviceTypes deviceType)
        {
            this.deviceType = deviceType;
        }
        #endregion

        #region methods

        public abstract bool Connect();

        //public virtual bool Disconnect() { return true; }
        public abstract bool Disconnect();

        //public void SetConfiguration(HWConfigBase config)
        //{
        //    this.config = config;
        //}

        public virtual bool Initialize() { return true; }

        public void RegisterConnetedEvent(DeviceConnected deviceConnected, DeviceDisconnected deviceDisconnected)
        {
            this.deviceConnected += deviceConnected;
            this.deviceDisconnected += deviceDisconnected;
        }
        public virtual void ClearRegiterEvents()
        {
            this.deviceConnected -= deviceConnected;
            this.deviceDisconnected -= deviceDisconnected;
        }

        public virtual void OnDeviceConnected()
        {
            if (deviceConnected != null)
                deviceConnected();
        }

        public virtual void OnDeviceDisconnected()
        {
            if (deviceDisconnected != null)
                deviceDisconnected();
        }

        public virtual UnitOnline Online()
        {
            return UnitOnline.Offline;
        }

        public virtual bool Shutdown()
        {
            ClearRegiterEvents();
            AppLogger.Info()("Shutdown.");
            return true;
        }

        public virtual bool Reset()
        {
            return true;
        }

        public virtual bool Abort()
        {
            IsAbort = true;
            AppLogger.Info()("{0} Device abort!");

            return true;
        }
        #endregion
    }
}
