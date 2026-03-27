using Matrox.MatroxImagingLibrary;
using Project.BaseLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class Matrox_PCI : MatroxGrabber
    {
        #region fields
        protected ConcurrentDictionary<int, MatroxController> _matroxControllers;

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

                _MilGrabBuffers = new ConcurrentDictionary<int, MIL_ID[]>();

                // Mil System Allocation
                foreach (var cam_info in CameraInfos)
                {
                    int board_id = cam_info.Board_ID;

                    string dcf_file = cam_info.InfoFilePathName;

                    if (_MilSystems[board_id] != MIL.M_NULL)
                    {
                        AppLogger.Error()("[{0}] camera, [{1}] board is not null. please check configuration.", cam_info.CamNumber, board_id);
                        return false;
                    }

                    MIL.MsysAlloc(MIL.M_DEFAULT, "M_DEFAULT", board_id, MIL.M_DEFAULT, ref _MilSystems[board_id]);

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

                    _matroxControllers[cam_number] = new MatroxController(cam_number, cam_info.ComPortName);
                    if (!_matroxControllers[cam_number].Connect())
                    {
                        logger.Error()("[{0}] camera is not connected.", cam_number);
                        return false;
                    }


                }
            }
            catch (Exception e)
            {
                logger.Error()("[{0}] Allocated failed. Message : {1}", Name, e.Message);

                return false;
            }

            logger.Info()("Maxtrox_PCI Grabber Allocation Success.");
            return true;
        }
        protected override bool ApplyParam()
        {
            foreach (var cam_info in CameraInfos)
            {
                string command = string.Empty;
                int board_id = cam_info.Board_ID;
                int cam_number = cam_info.CamNumber;
                int cam_id = cam_info.Camera_ID;

                TriggerModes tri_mode = cam_info.TriggerMode;
                ExposureModes exp_mode = cam_info.ExposureMode;
                int exp_time = cam_info.ExposureTime;
                bool reverse_x = cam_info.ReverseX;

                bool command_result = false;

                if(tri_mode == TriggerModes.External)
                {

                    // External Trigger On
                    command = string.Format("stm 1");
                    command_result = _matroxControllers[cam_number].MessageSendSync(command);

                    if(!command_result)
                    {
                        AppLogger.Error()("[{0}] camera set trigger mode(stm 1) failed.", cam_number);
                        return false;
                    }
                    int mode = 0;


                    // Exposure Mode(Timed or TriggerWidth) Setting
                    mode = exp_mode == ExposureModes.Timed ? 0 : 1;

                    command = string.Format("sem {0}", mode);

                    command_result = _matroxControllers[cam_number].MessageSendSync(command);

                    if (!command_result)
                    {
                        AppLogger.Error()("[{0}] camera set exposure mode(stm {1}) failed.", cam_number, mode);
                        return false;
                    }
                }
                else
                {
                    // Exposure Mode Timed Setting
                    command = string.Format("sem 0");
                    command_result = _matroxControllers[cam_number].MessageSendSync(command);

                    if (!command_result)
                    {
                        AppLogger.Error()("[{0}] camera set exposure mode(stm 0) failed.", cam_number);
                        return false;
                    }

                    // External Trigger Off
                    command = string.Format("stm 0");
                    command_result = _matroxControllers[cam_number].MessageSendSync(command);

                    if (!command_result)
                    {
                        AppLogger.Error()("[{0}] camera set trigger mode(stm 0) failed.", cam_number);
                        return false;
                    }
                }


                // Set Exposure time setting
                command = string.Format("set {0}", exp_time);
                command_result = _matroxControllers[cam_number].MessageSendSync(command);

                if (!command_result)
                {
                    AppLogger.Error()("[{0}] camera set exposure time(set {0}) failed.", cam_number, exp_time);
                    return false;
                }

                // Set Mirror mode setting
                command = string.Format("smm {0}", reverse_x == true ? 1 : 0);
                command_result = _matroxControllers[cam_number].MessageSendSync(command);

                if (!command_result)
                {
                    AppLogger.Error()("[{0}] camera set mirror mode(smm {0}) failed.", cam_number, reverse_x);
                    return false;
                }

                // Set LineRate
                int line_rate = cam_info.LineRate;
                command = string.Format("ssf {0}", line_rate);
                command_result = _matroxControllers[cam_number].MessageSendSync("line_rate");
                if (!command_result)
                {
                    logger.Error()("[{0}] camera set line rate[{1}] command is failed.", cam_number, line_rate);
                    return false;
                }

                command_result = _matroxControllers[cam_number].MessageSendSync("gcp");
                if (!command_result)
                {
                    logger.Error()("[{0}] camera gcp command is failed.", cam_number);
                    return false;
                }

                AppLogger.Info()("[{0}] camera : TriggerMode = {1}, ExposureMode = {2}, ExposureTime = {3}, Mirror = {4}",
                    cam_number, tri_mode, exp_mode, exp_time, reverse_x);
            }

            return true;
        }
        public override bool Reset()
        {
            if(base.Reset())
            {
                //matrox controller free
                if (_matroxControllers != null)
                {
                    foreach (var controller in _matroxControllers.Values)
                    {
                        controller.Dispose();
                    }
                }

                return true;
            }

            return false;
        }

        #endregion

        #region constructors
        public Matrox_PCI()
            : base(GrabberTypes.Matrox_PCI)
        {
            _matroxControllers = new ConcurrentDictionary<int, MatroxController>();

        }
        #endregion
    }
}
