using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class DevicesRepository : Singleton<DevicesRepository>
    {
        #region fields
        public Dictionary<Type, IDevice> devices;


        #endregion

        #region propertise
        public Dictionary<Type, IDevice> Devices
        {
            get { return devices; }
        }

        public int Count
        {
            get { return devices.Count; }
        }
        #endregion

        #region methods
        public DeviceBase GetDevice(int nIndex)
        {
            int i = 0;
            foreach (var device in devices.Values)
            {
                if (nIndex == i)
                {
                    return (DeviceBase)device;
                }
                i++;
            }

            return null;
        }
        public T GetDevice<T>() where T : class, IDevice
        {
            Type deviceType = typeof(T);
            if (!devices.ContainsKey(deviceType))
            {
                T value = (T)Activator.CreateInstance(deviceType, true);
                return AddDevice<T>(value);
            }

            return devices[deviceType] as T;
        }
        public T AddDevice<T>(T value) where T : class, IDevice
        {
            Type unitType = typeof(T);
            if (devices.ContainsKey(unitType))
            {
                // throw new ArgumentNullException("Type " + unitType + " already exist");
            }
            else
            {
                devices.Add(unitType, value);
            }

            return value;
        }
        //
        public void CreateDevices()
        {
            devices = new Dictionary<Type, IDevice>();
        }
        //
        public void Initialize()
        {
            CreateDevices();
        }
        public void Cleanup()
        {
        }
        //
        public void Shutdown()
        {
            foreach (var device in devices.Values)
            {
                device.Shutdown();
            }
        }

        public void Abort()
        {
            foreach (var device in devices.Values)
            {
                device.Abort();
            }
        }
        #endregion

        #region constructors
        protected DevicesRepository()
        {
            devices = new Dictionary<Type, IDevice>();
        }
        #endregion
    }
}
