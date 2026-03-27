using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class Matrox_GigE : MatroxGrabber
    {
        #region fields

        #endregion

        #region propertise

        #endregion

        #region methods
        protected override bool Allocate()
        {

            try
            {
                //MIL.MappControl(_MilApplication, MIL.M_ERROR, MIL.M_THROW_EXCEPTION);
                MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);

                _MilSystems = new MIL_ID[BoardCount];
                _MilDigitizers = new MIL_ID[camera_infos.Count];

                hookDatas = new Matrox_HookData[camera_infos.Count];

                // Mil System Allocation
                foreach (var cam_info in CameraInfos)
                {
                    int board_id = cam_info.Board_ID;

                    string dcf_file = cam_info.InfoFilePathName;

                    if (_MilSystems[board_id] != MIL.M_NULL)
                        continue;

                    MIL.MsysAlloc(MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, MIL.M_DEFAULT, ref _MilSystems[board_id]);

                    if (_MilSystems[board_id] == MIL.M_NULL)
                    {
                        logger.Error()("System id [{0}] allocated failed.", board_id);
                        return false;
                    }
                }

                // Mil Digitizer and buffer Allocation
                foreach (var cam_info in CameraInfos)
                {
                    int board_id = cam_info.Board_ID;
                    int cam_number = cam_info.CamNumber;
                    int cam_id = cam_info.Camera_ID;
                    int frame_buffer_count = cam_info.FrameBufferCount;

                    MIL.MdigAlloc(_MilSystems[board_id], cam_id, cam_info.InfoFilePathName, MIL.M_DEFAULT, ref _MilDigitizers[cam_number]);

                    if (_MilDigitizers[cam_number] == MIL.M_NULL)
                    {
                        logger.Error()("[{0}] digitizer allocated failed.", cam_number);
                        return false;
                    }

                    MIL.MdigControl(_MilDigitizers[cam_number], MIL.M_GRAB_TIMEOUT, MIL.M_INFINITE);

                    _MilGrabBuffers[cam_number] = new MIL_ID[frame_buffer_count];

                    //foreach(var buf in _MilGrabBuffers[cam_number])
                    for (int i = 0; i < frame_buffer_count; i++)
                    {
                        MIL.MbufAlloc2d(_MilSystems[board_id],
                            cam_info.Width, cam_info.Height,
                            8L + MIL.M_UNSIGNED,
                            MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref _MilGrabBuffers[cam_number][i]);

                        if (_MilGrabBuffers[cam_number][i] == MIL.M_NULL)
                        {
                            logger.Error()("[{0}] camera [{1}] mil grab buffer allocation failed.", cam_number, i);
                            return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error()("[{0}] Allocated failed. Message : {1}", Name, e.Message);

                return false;
            }

            logger.Info()("Maxtrox_GigE Grabber Allocation Success.");
            return true;
        }

        protected override bool ApplyParam()
        {
            foreach (var cam_info in CameraInfos)
            {
                int cam_number = cam_info.CamNumber;
                int board_id = cam_info.Board_ID;
                int frame_buffer_count = cam_info.FrameBufferCount;

                long sensorWidth = 0;// camera_infos[i].SensorWidth;

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "SensorWidth", MIL.M_TYPE_INT64, ref sensorWidth);
                Thread.Sleep(1);

                long width = cam_info.Width;

                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "Width", MIL.M_TYPE_INT64, ref width);

                Thread.Sleep(1);

                long offsetX = cam_info.OffsetX;
                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "OffsetX", MIL.M_TYPE_INT64, ref offsetX);
                Thread.Sleep(1);

                long sensorHeight = 0;//  camera_infos[i].SensorHeight;

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "SensorHeight", MIL.M_TYPE_INT64, ref sensorHeight);
                Thread.Sleep(1);

                long height = cam_info.Height;

                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "Height", MIL.M_TYPE_INT64, ref height);
                Thread.Sleep(1);

                long offsetY = cam_info.OffsetY;
                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "OffsetY", MIL.M_TYPE_INT64, ref offsetY);
                Thread.Sleep(1);

                string triggerMode = cam_info.TriggerMode == TriggerModes.External ? "On" : "Off";

                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "TriggerMode", MIL.M_TYPE_STRING, triggerMode);
                Thread.Sleep(1);

                double ExposureTime = cam_info.ExposureTime;
                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ExposureTime);
                Thread.Sleep(1);

                var reverse = cam_info.ReverseX;
                MIL.MdigControlFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "ReverseX", MIL.M_TYPE_BOOLEAN, ref reverse);

                Thread.Sleep(1);

                // ================================================================================================================================

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "Width", MIL.M_TYPE_INT64, ref width);
                Thread.Sleep(1);

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "OffsetX", MIL.M_TYPE_INT64, ref offsetX);
                Thread.Sleep(1);

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "Height", MIL.M_TYPE_INT64, ref height);
                Thread.Sleep(1);

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE,
                    "OffsetY", MIL.M_TYPE_INT64, ref offsetY);
                Thread.Sleep(1);

                //MIL.MdigInquireFeature(_MilDigitizers[i], MIL.M_FEATURE_VALUE, "ExposureMode", MIL.M_TYPE_STRING, btrigMode);
                ////Thread.Sleep(1);

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ExposureTime);
                Thread.Sleep(1);

                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "ReverseX", MIL.M_TYPE_BOOLEAN, ref reverse);

                bool tm = false;
                MIL.MdigInquireFeature(_MilDigitizers[cam_number], MIL.M_FEATURE_VALUE, "TriggerMode", MIL.M_TYPE_BOOLEAN, ref tm);

                logger.Info()("[{0}] camera info : SenserWidth : {1}, SensorHeight : {2}, Width : {3}, Height : {4}, Offset : {5}" +
                    "OffsetY : {6}, ExposureTime : {7}, Reverse X : {8}, UseTriggerMode : {9}", cam_number, sensorWidth, sensorHeight, width, height, offsetX, offsetY, ExposureTime, reverse, tm);
            }
            return true;
        }

        #endregion

        #region constructors
        public Matrox_GigE()
            : base(GrabberTypes.Matrox_GigE)
        {

        }

        #endregion
    }
}
