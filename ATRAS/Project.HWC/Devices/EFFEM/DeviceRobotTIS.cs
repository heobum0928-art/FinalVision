using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Project.BaseLib.HW;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System.Collections;
using System.Threading;

namespace Project.HWC
{
    public class TISRobotOutputs : TISSensors
    {
        public TISRobotOutputs(string ioValues, int nbyte)
            : base(ioValues, nbyte)
        {
        }

        #region ch1
        public IOStates InterlockEnable1 { get { return (bits[0][0] ? IOStates.On : IOStates.Off); } }
        public IOStates InterlockEnable2 { get { return (bits[0][1] ? IOStates.On : IOStates.Off); } }
        public IOStates MappingOn { get { return (bits[0][2] ? IOStates.On : IOStates.Off); } }
        public IOStates RAxisVacuumGripSol { get { return (bits[0][3] ? IOStates.On : IOStates.Off); } }

        public IOStates NotUsed1 { get { return (bits[0][4] ? IOStates.On : IOStates.Off); } }
        public IOStates RunningSensor { get { return (bits[0][5] ? IOStates.On : IOStates.Off); } }
        public IOStates OriginCompleteSensor { get { return (bits[0][6] ? IOStates.On : IOStates.Off); } }
        public IOStates AlarmSensor { get { return (bits[0][7] ? IOStates.On : IOStates.Off); } }
        #endregion

        public override string ToString()
        {
            return string.Format("Outputs : InterlockEnable1={0} InterlockEnable2={1} MappingOn={2} RAxisVacuumGripSol={3} RunningSensor={4} OriginCompleteSensor={5} AlarmSensor={6}",
                                            InterlockEnable1, InterlockEnable2, MappingOn, RAxisVacuumGripSol, RunningSensor, OriginCompleteSensor, AlarmSensor);
        }
    }

    public class TISRobotInputs : TISSensors
    {
        public TISRobotInputs(string ioValues, int nbyte)
            : base(ioValues, nbyte)
        {
        }

        #region ch1
        public IOStates MappingSensor { get { return (bits[0][0] ? IOStates.On : IOStates.Off); } }
        public IOStates RAxisVacuumSensor { get { return (bits[0][1] ? IOStates.On : IOStates.Off); } }   // under
        //
        public IOStates LAxisVacuumSensor { get { return (bits[0][4] ? IOStates.On : IOStates.Off); } }   // upper
        #endregion

        #region ch2
        // 0: Auto 1:Manual(TP)
        public IOStates TPAutoMode { get { return (bits[1][0] ? IOStates.On : IOStates.Off); } }
        public IOStates Origin { get { return (bits[1][1] ? IOStates.On : IOStates.Off); } }
        public IOStates ErrorReset { get { return (bits[1][2] ? IOStates.On : IOStates.Off); } }
        public IOStates DoorInterlock { get { return (bits[1][3] ? IOStates.On : IOStates.Off); } }
        #endregion


        public override string ToString()
        {
            return string.Format("Inputs : MappingSensor={0} RAxisVacuumSensor={1} LAxisVacuumSensor={2} TPAutoMode={3} Origin={4} ErrorReset={5} DoorInterlock={6}",
                                            MappingSensor, RAxisVacuumSensor, LAxisVacuumSensor, TPAutoMode, Origin, ErrorReset, DoorInterlock);
        }
    }

    public class DeviceRobotTIS : DeviceTIS, IDeviceRobot
    {
        #region fields

        private List<TISErrors> robotErrorList;

        #endregion

        #region propertise
        public int RobotIndex
        {
            get
            {
                if(Config is WTRConfig)
                {
                    return (Config as WTRConfig).RobotIndex;
                }
                return -1;
            }
        }
        #endregion

        #region methods

        public bool ServoPower(bool on)
        {
            var osr = MessageSend(on ? "ENABLE" : "DISABLE", null, ACTION_TIMEOUT);
            if (osr == false)
            {
                return false;
            }

            return true;
        }

        public bool GoHome()
        {
            var oss = MessageSend("HOME", null, ACTION_TIMEOUT);
            if (oss == false)
            {
                return false;
            }

            return true;
        }

        public override bool HWInitialize()
        {
            var os = base.HWInitialize();
            if(os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device hw initialize failed.");
                return false;
            }

            os = Initialize();
            if(os == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device Initialize failed.");
                return false;
            }
            //os = GoHome();
            //if (os == false)
            //{
            //    AppLogger.Error()($"{Name}-{Device_Index} device homing failed.");
            //    return false;
            //}

            return true;
        }

        public override bool Initialize()
        {
            var input = GetInputStatus();
            if (input == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetInputStatus() failed.");
                return false;
                //return new OperationStatus(new FICHardwareFailureError(input.ErrorInfo, "GetInputStatus failed."));
            }

            //
            var output = GetOutputStatus();
            if (output == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetOutputStatus() failed.");
                return false;
                //return new OperationStatus(new FICHardwareFailureError(output.ErrorInfo, "GetOutputStatus failed."));
            }

            //var power = GetServoStatus();
            //if (power.IsError)
            //{
            //    return new OperationStatus(new FICHardwareFailureError(output.ErrorInfo, "GetServoStatus failed."));
            //}

            // if not origin or servo power off
            //if (output.OriginCompleteSensor != IOStates.On)
            //{
                var os = ServoPower(true);
                if (os == false)
                {
                    AppLogger.Error()($"{Name}-{Device_Index} device servo power on failed.");
                    return false;
                    //return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "Servo power on failed."));
                }

                os = Homing();
                if (os == false)
                {
                    AppLogger.Error()($"{Name}-{Device_Index} device homing failed.");
                    return false;
                    //return new OperationStatus(new FICHardwareFailureError(os.ErrorInfo, "Homing failed."));
                }

                GetOutputStatus();
            //}

            return true;
        }

        private void CreateErrorList()
        {
            robotErrorList = new List<TISErrors>();

            robotErrorList.Add(new TISErrors("2", "ERR_NOT_HOMED"));
            robotErrorList.Add(new TISErrors("4", "ERR_EMERGENCY"));

            robotErrorList.Add(new TISErrors("5", "ERR_POWER_NOT_ENABLE"));
            robotErrorList.Add(new TISErrors("12", "ERR_MOTOR_ERROR"));
            robotErrorList.Add(new TISErrors("194", "ERR_INTERLOCK"));
            robotErrorList.Add(new TISErrors("202", "ERR_WAFER_BEFORE_GET"));
            robotErrorList.Add(new TISErrors("203", "ERR_NO_WAFER_BEFORE_PUT"));
            robotErrorList.Add(new TISErrors("204", "ERR_NO_WAFER_AFTER_GET"));

            robotErrorList.Add(new TISErrors("205", "ERR_WAFER_AFTER_PUT"));
            robotErrorList.Add(new TISErrors("206", "ERR_NO_WAFER_DURING_GET"));
            robotErrorList.Add(new TISErrors("207", "ERR_WAFER_CURING_PUT"));
            robotErrorList.Add(new TISErrors("208", "ERR_NOT_HOME"));
            robotErrorList.Add(new TISErrors("209", "ERR_NO_WAFER_AFTER_GET"));

            robotErrorList.Add(new TISErrors("251", "ERR_MAPPING_IS_NOT_PERFORMED"));
            robotErrorList.Add(new TISErrors("252", "ERR_NO_MAPPING_DATA"));

            robotErrorList.Add(new TISErrors("1001", "ERR_INVALID_COMMAND"));

            robotErrorList.Add(new TISErrors("1011", "ERR_INVALID_DATA"));
            robotErrorList.Add(new TISErrors("1012", "ERR_INVALID_STATION"));
            robotErrorList.Add(new TISErrors("1013", "ERR_INVALID_HAND"));
            robotErrorList.Add(new TISErrors("1014", "ERR_INVALID_SLOT"));
            robotErrorList.Add(new TISErrors("1015", "ERR_INVALID_TEACHING_INDEX"));
            robotErrorList.Add(new TISErrors("1016", "ERR_INVALID_PD_INDEX"));
            robotErrorList.Add(new TISErrors("1017", "ERR_INVALID_DOUBLE_ERROR"));
            robotErrorList.Add(new TISErrors("1018", "ERR_INVALID_NOEXIT_ERROR"));

            robotErrorList.Add(new TISErrors("1021", "ERR_INVALID_COORDINATE_TYPE"));

            robotErrorList.Add(new TISErrors("1031", "ERR_INVALID_ARGUMENT"));
            robotErrorList.Add(new TISErrors("1033", "ERR_INVALID_FORMAT"));
            robotErrorList.Add(new TISErrors("1034", "ERR_INVALID_LOCATION_FORMAT"));
            robotErrorList.Add(new TISErrors("1035", "ERR_INVALID_PROFILE_FORMAT"));

            robotErrorList.Add(new TISErrors("1041", "ERR_WRONG_PD_COMMAND"));
            robotErrorList.Add(new TISErrors("1042", "ERR_WRONG_AWC_DATA"));
            robotErrorList.Add(new TISErrors("1043", "ERR_NO_AWC_DATA"));

            robotErrorList.Add(new TISErrors("1051", "ERR_NO_DATA"));
            robotErrorList.Add(new TISErrors("1052", "ERR_NOT_HOME"));
            robotErrorList.Add(new TISErrors("1053", "ERR_CANNOT_RETRACT_ARM"));
            robotErrorList.Add(new TISErrors("1054", "ERR_VACUUM_DETECTING_ERROR"));
            robotErrorList.Add(new TISErrors("1055", "ERR_NO_WAFER"));
            robotErrorList.Add(new TISErrors("1056", "ERR_UPGRIP"));
            robotErrorList.Add(new TISErrors("1057", "ERR_DOUBLE_WAFER_CHECK"));

            robotErrorList.Add(new TISErrors("1060", "ERR_NOT_SUPPLY_AIR"));

            robotErrorList.Add(new TISErrors("1999", "USER_STOP_REQUEST"));

            robotErrorList.Add(new TISErrors("2000", "ERR_RECEIVEBUF_FULL"));
            robotErrorList.Add(new TISErrors("2001", "ERR_SENDBUF_FULL"));









            //robotErrorList = new List<TISErrors>();

            //robotErrorList.Add(new TISErrors("0002", "ERR_NOT_HOMED"));
            //robotErrorList.Add(new TISErrors("0004", "ERR_EMERGENCY"));

            //robotErrorList.Add(new TISErrors("0005", "ERR_POWER_NOT_ENABLE"));
            //robotErrorList.Add(new TISErrors("0012", "ERR_MOTOR_ERROR"));
            //robotErrorList.Add(new TISErrors("0194", "ERR_INTERLOCK"));
            //robotErrorList.Add(new TISErrors("0202", "ERR_WAFER_BEFORE_GET"));
            //robotErrorList.Add(new TISErrors("0203", "ERR_NO_WAFER_BEFORE_PUT"));
            //robotErrorList.Add(new TISErrors("0204", "ERR_NO_WAFER_AFTER_GET"));

            //robotErrorList.Add(new TISErrors("0205", "ERR_WAFER_AFTER_PUT"));
            //robotErrorList.Add(new TISErrors("0206", "ERR_NO_WAFER_DURING_GET"));
            //robotErrorList.Add(new TISErrors("0207", "ERR_WAFER_CURING_PUT"));
            //robotErrorList.Add(new TISErrors("0208", "ERR_NOT_HOME"));
            //robotErrorList.Add(new TISErrors("0209", "ERR_NO_WAFER_AFTER_GET"));

            //robotErrorList.Add(new TISErrors("0251", "ERR_MAPPING_IS_NOT_PERFORMED"));
            //robotErrorList.Add(new TISErrors("0252", "ERR_NO_MAPPING_DATA"));

            //robotErrorList.Add(new TISErrors("1001", "ERR_INVALID_COMMAND"));

            //robotErrorList.Add(new TISErrors("1011", "ERR_INVALID_DATA"));
            //robotErrorList.Add(new TISErrors("1012", "ERR_INVALID_STATION"));
            //robotErrorList.Add(new TISErrors("1013", "ERR_INVALID_HAND"));
            //robotErrorList.Add(new TISErrors("1014", "ERR_INVALID_SLOT"));
            //robotErrorList.Add(new TISErrors("1015", "ERR_INVALID_TEACHING_INDEX"));
            //robotErrorList.Add(new TISErrors("1016", "ERR_INVALID_PD_INDEX"));
            //robotErrorList.Add(new TISErrors("1017", "ERR_INVALID_DOUBLE_ERROR"));
            //robotErrorList.Add(new TISErrors("1018", "ERR_INVALID_NOEXIT_ERROR"));

            //robotErrorList.Add(new TISErrors("1021", "ERR_INVALID_COORDINATE_TYPE"));

            //robotErrorList.Add(new TISErrors("1031", "ERR_INVALID_ARGUMENT"));
            //robotErrorList.Add(new TISErrors("1033", "ERR_INVALID_FORMAT"));
            //robotErrorList.Add(new TISErrors("1034", "ERR_INVALID_LOCATION_FORMAT"));
            //robotErrorList.Add(new TISErrors("1035", "ERR_INVALID_PROFILE_FORMAT"));

            //robotErrorList.Add(new TISErrors("1041", "ERR_WRONG_PD_COMMAND"));
            //robotErrorList.Add(new TISErrors("1042", "ERR_WRONG_AWC_DATA"));
            //robotErrorList.Add(new TISErrors("1043", "ERR_NO_AWC_DATA"));

            //robotErrorList.Add(new TISErrors("1051", "ERR_NO_DATA"));
            //robotErrorList.Add(new TISErrors("1052", "ERR_NOT_HOME"));
            //robotErrorList.Add(new TISErrors("1053", "ERR_CANNOT_RETRACT_ARM"));
            //robotErrorList.Add(new TISErrors("1054", "ERR_VACUUM_DETECTING_ERROR"));
            //robotErrorList.Add(new TISErrors("1055", "ERR_NO_WAFER"));
            //robotErrorList.Add(new TISErrors("1056", "ERR_UPGRIP"));
            //robotErrorList.Add(new TISErrors("1057", "ERR_DOUBLE_WAFER_CHECK"));

            //robotErrorList.Add(new TISErrors("1060", "ERR_NOT_SUPPLY_AIR"));

            //robotErrorList.Add(new TISErrors("1999", "USER_STOP_REQUEST"));

            //robotErrorList.Add(new TISErrors("2000", "ERR_RECEIVEBUF_FULL"));
            //robotErrorList.Add(new TISErrors("2001", "ERR_SENDBUF_FULL"));
        }

        public TISRobotInputs GetInputStatus(int ch)
        {
            string parameters = string.Format($"{ch}");
            var oss = MessageSend("IDI", parameters, REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetInputStatus({ch}) failed.");
                return null;
                //return new OperationStatus<TISLoadportInputs>(null, oss.ErrorInfo);
            }

            return new TISRobotInputs(ResponseParameters, 8);
        }


        public TISRobotOutputs GetOutputStatus(int ch)
        {
            string parameters = string.Format($"{ch}");
            var oss = MessageSend("IDO", parameters, REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device GetOutputStatus({ch}) failed.");
                return null;
                //return new OperationStatus<TISLoadportInputs>(null, oss.ErrorInfo);
            }



            return new TISRobotOutputs(ResponseParameters, 8);
        }

        public TISRobotInputs GetInputStatus()
        {
            var oss = MessageSend("IDI", "0,2", REQUEST_TIMEOUT);
            if (oss == false)
            {
                return null;
            }

            TISRobotInputs inputs = new TISRobotInputs(ResponseParameters, 2);

            AppLogger.Debug()(inputs.ToString());

            return inputs;



            //TISRobotInputs i1 = GetInputStatus(0);
            //string r1 = ResponseParameters;
            //TISRobotInputs i2 = GetInputStatus(1);
            //string r2 = ResponseParameters;
            //TISRobotInputs i3 = GetInputStatus(2);
            //string r3 = ResponseParameters;

            //if (i1 != null && i2 != null && i3 != null)
            //{
            //    var t1 = r1.Split(',');
            //    var t2 = r2.Split(',');
            //    var t3 = r3.Split(',');


            //    string data = string.Format($"{t1[1]},{t2[1]},{t3[1]}");
            //    return new TISRobotInputs(data, 8);

            //}

            //return null;





            TISRobotInputs i1 = GetInputStatus(0);
            string r1 = ResponseParameters;
            TISRobotInputs i2 = GetInputStatus(1);
            string r2 = ResponseParameters;

            if (i1 != null && i2 != null)
            {
                var t1 = r1.Split(',');
                var t2 = r2.Split(',');
                if (t1.Length != 2 || t2.Length != 2)
                    return null;


                string data = string.Format($"{t1[1]},{t2[1]}");
                return new TISRobotInputs(data, 8);

            }
            AppLogger.Error()($"{Name}-{Device_Index} device GetInputStatus() failed.");
            return null;
        }

        public TISRobotOutputs GetOutputStatus()
        {
            var oss = MessageSend("IDO", "1", REQUEST_TIMEOUT);
            if (oss == false)
            {
                return null;
            }

            //
            TISRobotOutputs outputs = new TISRobotOutputs(ResponseParameters, 1);

            AppLogger.Debug()(outputs.ToString());

            return outputs;




            //TISLoadportOutputs i1 = GetOutputStatus(0);
            //string r1 = ResponseParameters;
            //TISLoadportOutputs i2 = GetOutputStatus(1);
            //string r2 = ResponseParameters;
            //TISLoadportOutputs i3 = GetOutputStatus(2);
            //string r3 = ResponseParameters;

            //if (i1 != null && i2 != null && i3 != null)
            //{
            //    var t1 = r1.Split(',');
            //    var t2 = r2.Split(',');
            //    var t3 = r3.Split(',');


            //    string data = string.Format($"{t1[1]},{t2[1]},{t3[1]}");
            //    return new TISLoadportOutputs(data, 8);

            //}

            //return null;



            TISRobotOutputs i1 = GetOutputStatus(0);
            string r1 = ResponseParameters;
            TISRobotOutputs i2 = GetOutputStatus(1);
            string r2 = ResponseParameters;

            if (i1 != null && i2 != null)
            {
                var t1 = r1.Split(',');
                var t2 = r2.Split(',');

                string data = string.Format($"{t1[1]},{t2[1]}");
                return new TISRobotOutputs(data, 8);
            }

            AppLogger.Error()($"{Name}-{Device_Index} device GetOutputStatus() failed.");
            return null;
        }

        public override TISErrors ParseErrorMessage(string command, string parameter)
        {
            // no need check
            if (command == "ERR" ||
                command == "ERD" ||
                command == "NIDI" ||
                command == "NIDO" ||
                command == "RPS" ||
                command == "VER")
            {
                return null;
            }

            if (parameter != "E02")     //"Received run command when error")
            {
                TISErrors error = robotErrorList.Find(s => s.ErrorCode == parameter);
                if (error != null)
                {
                    return error;
                }

                error = base.ParseErrorMessage(command, parameter);
                if (error != null)
                {

                    return error;
                }
            }

            //int errorCode;
            //if (!Int32.TryParse(parameter, out errorCode))
            //{
            //    return null;
            //}

            var oss = MessageSend("ERD", null, REQUEST_TIMEOUT);
            if (oss == false)
            {
                return new TISErrors("ERD", ResponseParameters);
            }

            if (ResponseParameters.Length > 0)
            {
                return new TISErrors(parameter, ResponseParameters);
            }

            return null;
        }
        
        public int GetStationNumber(RobotStations station, SubstrateSizes waferSize)
        {
            int number = 0;

            AppLogger.Info()($"{Name}-{Device_Index} device get station number. Robot Station : {station} / WaferSize : {waferSize}");
            return number;
        }

        //private string StationString(int station, SubstrateSizes waferSize)    //, int ArmNumber)
        //{
        //    switch (location)
        //    {
        //        case EquipmentSubstrateLocations.Aligner:
        //            return string.Format("{0},1", GetStationNumber(RobotStations.Prealigner, waferSize));
        //        case EquipmentSubstrateLocations.ChamberA:
        //            return string.Format("{0},1", GetStationNumber(RobotStations.Chamber, waferSize));
        //    }
        //    return string.Empty;
        //}

        //public string StationString(int portId, int slotId, SubstrateSizes waferSize)
        //{
        //    switch (portId)
        //    {
        //        case 1: return string.Format("{0},{1}", GetStationNumber(RobotStations.LoadPortA, waferSize), slotId);
        //        case 2: return string.Format("{0},{1}", GetStationNumber(RobotStations.LoadPortB, waferSize), slotId);
        //    }
        //    return string.Empty;
        //}
               
        public SubstratePresences PresenceSubstrate(RobotArmIDs armID)
        {
            var input = GetInputStatus();
            if(input == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device presence substrate is {SubstratePresences.Unknown}.");
                return SubstratePresences.Unknown;
            }

            SubstratePresences sp = SubstratePresences.Unknown;

            AppLogger.Info()($"{Name}-{Device_Index} device presence substrate is {sp}.");
            return sp;
        }

        public SubstratePresences PresenceSubstrate(int arm_number)
        {
            var input = GetInputStatus();
            if (input == null)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device presence substrate is {SubstratePresences.Unknown}.");
                return SubstratePresences.Unknown;
            }

            SubstratePresences sp = SubstratePresences.Unknown;

            AppLogger.Info()($"{Name}-{Device_Index} device presence substrate is {sp}.");
            return sp;
        }
        public bool SetRobotSpeed(RobotSpeeds speed)
        {
            AppLogger.Info()($"{Name}-{Device_Index} device set robot speed. speed is {speed}");
            //WLOS - Wafer Off speed(1~100%)
            //Command : WLOS v1
            //Factor : INT (%)
            //Function : If robot doesn’t have wafer, set the robot’s speed value (%) of GET/PUT command..
            //This speed is applied to the motion of arm extension and retraction.
            //Return :None
            //WLOS 20
            //WLOS
            var oss = MessageSend("WLOS", "100", REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device SetRobotSpeed(WLOS) failed.");
                return false;
            }

            //WHIS - Wafer On speed(1~100%)
            //Command : WHIS v1
            //Factor : INT (%)
            //Function : If robot has wafer, set the robot speed (%) of GET/PUT command.
            //This speed is applied to the motion of arm extension and retraction.
            //Return : WHIS
            //WHIS 60
            //WHIS

            int onspeed = 0;
            if (speed == RobotSpeeds.Slow)
            {
                onspeed = 80;
            }
            else
            {
                onspeed = 100;
            }

            oss = MessageSend("WHIS", onspeed.ToString(), ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device SetRobotSpeed(WHIS) failed.");
                return false;
            }

            return true;
        }

        // 
        public bool PreparePickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},1,{1}", station, armID);

            var oss = MessageSend("GRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PreparePickSubstrate(GRDY) failed. {armID} / {station} / {WaferSize}");

                return false;
            }

            return true;
        }
        public bool PreparePickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, (int)armID);

            var oss = MessageSend("GRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PreparePickSubstrate(GRDY) failed. {armID} / {portId} / {slotId} / {WaferSize}");

                return false;
            }

            return true;
        }
        public bool PreparePickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, (int)arm_number);

            var oss = MessageSend("GRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PreparePickSubstrate(GRDY) failed. {arm_number} / {portId} / {slotId} / {WaferSize}");

                return false;
            }

            return true;
        }

        // Get
        public bool PickSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},1,{1}", station, armID);

            var oss = MessageSend("GET", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PickSubstrate GET() failed. {armID} / {station} / {WaferSize}");
                return false;
            }

            return true;

            // check presence after get
            var presence = PresenceSubstrate(armID);
            if (presence == SubstratePresences.Unknown)
            {
                return false;
            }

            if (presence != SubstratePresences.Presence)
            {
                return false;
            }

            return true;
        }
        public bool PickSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, (int)armID);

            var oss = MessageSend("GET", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PickSubstrate GET() failed. {armID} / {portId} / {slotId} / {WaferSize}");
                return false;
            }

            return true;

            // check presence after get
            var presence = PresenceSubstrate(armID);
            if (presence == SubstratePresences.Unknown)
            {
                return false;
            }

            if (presence != SubstratePresences.Presence)
            {
                return false;
            }

            return true;
        }
        public bool PickSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, (int)arm_number);

            var oss = MessageSend("GET", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PickSubstrate GET() failed. {arm_number} / {portId} / {slotId} / {WaferSize}");
                return false;
            }

            return true;

            // check presence after get
            var presence = PresenceSubstrate(arm_number);
            if (presence == SubstratePresences.Unknown)
            {
                return false;
            }

            if (presence != SubstratePresences.Presence)
            {
                return false;
            }

            return true;
        }


        //
        public bool PreparePlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},1,{1}", station, armID);

            var oss = MessageSend("PRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PreparePlacedSubstrate(PRDY) failed. {armID} / {station} / {WaferSize}");
                return false;
            }

            return true;
        }
        public bool PreparePlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, armID);

            var oss = MessageSend("PRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                return false;
            }

            return true;
        }
        public bool PreparePlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, arm_number);

            var oss = MessageSend("PRDY", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                return false;
            }

            return true;
        }

        // Put
        public bool PlacedSubstrate(RobotArmIDs armID, int station, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},1,{1}", station, armID);

            var oss = MessageSend("PUT", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PlacedSubstrate PUT() failed. {armID} / {station} / {WaferSize}");
                return false;
            }

            return true;
        }
        public bool PlacedSubstrate(RobotArmIDs armID, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, armID);

            var oss = MessageSend("PUT", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PlacedSubstrate PUT() failed. {armID} / {portId} / {slotId} / {WaferSize}");
                return false;
            }

            return true;
        }
        public bool PlacedSubstrate(int arm_number, int portId, int slotId, SubstrateSizes WaferSize)
        {
            string parameters = string.Format("{0},{1},{2}", portId, slotId, arm_number);

            var oss = MessageSend("PUT", parameters, ACTION_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"{Name}-{Device_Index} device PlacedSubstrate PUT() failed. {arm_number} / {portId} / {slotId} / {WaferSize}");
                return false;
            }

            return true;
        }



        #endregion

        #region constructors
        public DeviceRobotTIS(NetworkConfig config)
            : base(DeviceTypes.WTR_TIS, config)
        {
            InsertErrors();
            CreateErrorList();
        }
        #endregion
    }
}
