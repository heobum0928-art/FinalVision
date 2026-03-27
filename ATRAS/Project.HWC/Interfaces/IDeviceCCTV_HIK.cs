using Project.BaseLib.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    public interface IDeviceNVRServer : IDevice

    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods

        bool Login();
        bool Logout();

        bool Real_Play(int cctv_index);
        bool PlayFast(int cctv_index);
        bool PlaySlow(int cctv_index);
        bool PlayPause(int cctv_index);
        bool PlayReverse(int cctv_index);

        bool FramePlay(int cctv_index);

        bool PlayBack(int cctv_index, DateTime start, DateTime end);
        bool Download_start(int cctv_index, DateTime start, DateTime end, string path, string filename);
        bool Download_stop(int cctv_index);

        bool PlayStop(int cctv_index);

        bool CaptureBMP(int cctv_index);
        bool CaptureJPEG(int cctv_index);
        bool Record(int cctv_index);


        DateTime? GetCurrentPlayTime(int cctv_index);

        #endregion

        #region constructors

        #endregion
    }
}
