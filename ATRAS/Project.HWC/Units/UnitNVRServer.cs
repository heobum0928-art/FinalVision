using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class UnitNVRServer : UnitTyped
    {
        #region fields

        #endregion

        #region propertise
        public IDeviceNVRServer Device
        {
            get { return base.BaseDevice as IDeviceNVRServer; }
            set
            {
                base.BaseDevice = value;
            }
        }
        #endregion

        #region methods
        public override IDevice CreateDevice()
        {
            var config = (Config as NVRServerConfig);

            IDevice device = null;

            if (config.DeviceType == DeviceTypes.NVRServer_HIK)
            {

                if (BaseDevice != null && BaseDevice is DeviceNVR_HIK)
                {
                    var old_device = BaseDevice as DeviceNVR_HIK;


                    if(!old_device.Reset())
                    {
                        AppLogger.Error()("[{0}] device reset false.", UnitName);
                        return null;
                    }

                    old_device = null;
                }



                var hik_device = new DeviceNVR_HIK();
                var nvr_config = (Config as NVRServerConfig);
                //hik_device.SetLoginData(nvr_config.TcpAddress, nvr_config.TcpPort, nvr_config.UserName, nvr_config.Password);
                hik_device.SetNVRServerConfig(nvr_config);

                device = hik_device;
            }
            else if(config.DeviceType == DeviceTypes.HWS)
            {
                
            }
            BaseDevice = null;

            return device;
        }

        public override bool Initialize()
        {
            if (!Device.Login())
            {
                AppLogger.Error()("Device Login() failed.");
                return false;
            }

            AppLogger.Info()("[{0}] unit initialize success!", UnitName);
            return true;
        }

        public bool PlayStop(int cctv_idx)
        {
            return Device.PlayStop(cctv_idx);
        }

        public bool CaptureBmp(int cctv_idx)
        {
            return Device.CaptureBMP(cctv_idx);
        }

        public async Task<bool> PlayBackAsync(int cctv_idx, DateTime start, DateTime end)
        {
            return await Task.Run(() =>
            {
                return Device.PlayBack(cctv_idx, start, end);
            });
        }

        public bool PlayPause(int cctv_index)
        {
            return Device.PlayPause(cctv_index);
        }

        public async Task<bool> PlayPauseAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.PlayPause(cctv_idx);
            });
        }

        public async Task<bool> PlayReverseAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.PlayReverse(cctv_idx);
            });
        }

        public bool Download(int cctv_idx, DateTime start, DateTime end, string path, string filename)
        {
            return Device.Download_start(cctv_idx, start, end, path,  filename);
        }

        public async Task<bool> DownloadAsync(int cctv_idx, DateTime start, DateTime end, string path, string filename)
        {
            return await Task.Run(() =>
            {
                return Device.Download_start(cctv_idx, start, end, path, filename);
            });
        }

        public async Task<bool> PlayFastAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.PlayFast(cctv_idx);
            });
        }

        public async Task<bool> PlaySlowAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.PlaySlow(cctv_idx);
            });
        }

        public async Task<bool> FramePlayAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.FramePlay(cctv_idx);
            });

        }

        public async Task<DateTime?> GetCurrentPlayTimeAsync(int cctv_idx)
        {
            return await Task.Run(() =>
            {
                return Device.GetCurrentPlayTime(cctv_idx);
            });
        }

        #endregion

        #region constructors
        public UnitNVRServer()
        : base(UnitIDs.UnitNVRServer)
            {

            }
        #endregion
    }
}
