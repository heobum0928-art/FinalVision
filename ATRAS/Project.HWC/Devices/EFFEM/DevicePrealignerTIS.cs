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
    public class TISPrealignerInputs : TISSensors
    {
        //
        public TISPrealignerInputs(string ioValues, int nbyte)
            : base(ioValues, nbyte)
        {
        }

        #region ch0
        public IOStates VacuumSensor { get { return (bits[0][7] ? IOStates.On : IOStates.Off); } }
        #endregion

        public override string ToString()
        {
            return string.Format("Input : Vacuum={0}", VacuumSensor);
        }
    }

    public class TISAlignerResults
    {
        public double r = 0;
        public double x = 0;
        public double y = 0;
    }


    public class DevicePrealignerTIS : DeviceTIS, IDevicePrealigner
    {
        #region fields
        private List<TISErrors> prealignerErrorList;

        private bool needResetPosition = false;
        private double alignRotation = 0.0;
        private TISAlignerResults orgAlignerResult = null;
        #endregion

        #region propertise

        #endregion

        #region methods

        public override bool HWInitialize()
        {
            var r =  base.HWInitialize();
            if(r == false)
            {
                return false;
            }

            r = Initialize();
            if (r == false)
            {
                return false;
            }

            r = HomePosition();
            if (r == false)
            {
                return false;
            }

            PresenceSubstrate();

            return true;

        }

        public bool Initialize()
        {
            needResetPosition = true;

            if(_InitializationState == InitializationStates.NotReady)
            {
                return false;
            }

            if (_InitializationState == InitializationStates.Ready)
            {
                return Homing();
            }

            return true;

        }


        public PrealignerResult Align()
        {
            if (needResetPosition == true)
            {
                //return new OperationStatus<PrealignerResult>(null, new FICSoftwareFailureError("Need to Aligner Reset"));
                AppLogger.Error()($"{Name} device need to aligner reset.");
                return null;
            }

            //
            needResetPosition = true;
            string angle = string.Format("0,0,{0:0.00}", (int)(alignRotation * 100.0));    // 1 = 0.01 degree

            var osr = MessageSend("ALS", angle, SEND_TIMEOUT);
            if (osr == false)
            {
                // if error code "215" or "221"  ? retry align.
                //return new OperationStatus<PrealignerResult>(null, osr.ErrorInfo);
                AppLogger.Error()($"{Name} device Align(ALS) failed.");

                return null;
            }
            PrealignerResult result = GetAlignResult();

            AppLogger.Info()($"{Name} device Align. Result is {result.ToString()}");
            return result;
        }

        public PrealignerResult GetAlignResult()
        {
            TISAlignerResults osr = GetAlignerPosition();

            if (osr == null)
            {
                AppLogger.Error()($"{Name} GetAlignResult() failed.");

                return null;
            }

            //
            double angle = osr.r;
            double xd = (double)(orgAlignerResult.x - osr.x);    // X micron
            double yd = (double)(orgAlignerResult.y - osr.y);    // Y micron
            double distance = Math.Sqrt((xd * xd) + (yd * yd)); // Eccentricity Magnitude

            //
            PrealignerResult result = new PrealignerResult();
            //
            result.Result1 = xd;           // X distance
            result.Result2 = yd;           // Y distance
            result.EccentricityMagnitude = distance;     // distance from Center of wafer.

            return result;
        }

        public bool HomePosition()
        {
            return ResetPosition();

            AppLogger.Info()($"{Name} device set home position.");
            return true;
        }

        //public bool Homing()
        //{
        //    AppLogger.Info()($"{Name} device homing.");
        //    return true;
        //}

        public bool InitAngle(SubstrateOrientations direction, double angleOffset)
        {
            //OperationStatus os = HomePosition();
            //if (os.IsError)
            //{
            //    return os;
            //}

            switch (direction)
            {
                case SubstrateOrientations.Down: alignRotation = angleOffset; break;
                case SubstrateOrientations.Right: alignRotation = 90.0 + angleOffset; break;
                case SubstrateOrientations.Up: alignRotation = 180.0 + angleOffset; break;
                case SubstrateOrientations.Left: alignRotation = 270.0 + angleOffset; break;
            }

            AppLogger.Info()($"{Name} device InitAngle : Orientation : {direction}, offset : {angleOffset}");
            return true;
        }

        public SubstratePresences PresenceSubstrate()
        {
            var osr = MessageSend("WCH", null, SEND_TIMEOUT);
            if (osr == false)
            {
                AppLogger.Error()($"{Name} PresenceSubstrate(WCH) failed.");

                return SubstratePresences.NoPresence;
            }

            // if("V01=nn")
            if (string.Compare(ResponseParameters, 0, "V01=", 0, 4) == 0)
            {
                if (ResponseParameters[5] == '1')
                {
                    return SubstratePresences.Presence;
                }
                else if (ResponseParameters[5] == '0')
                {
                    return SubstratePresences.NoPresence;
                }
            }

            AppLogger.Error()("Have to received data is 1 or 0 [Received Message={0}]", osr);
            return SubstratePresences.Unknown;
        }

        public bool ResetPosition()
        {
            var os = MessageSend("RST", null, SEND_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name} ResetPosition(RST) failed.");

                return false;
            }

            needResetPosition = false;
            //
            // save origin Position;
            TISAlignerResults osr = GetAlignerPosition();
            if (osr == null)
            {
                return false;
            }

            orgAlignerResult = osr;

            //
            //GetOutputStatus();
            //GetInputStatus();
            return true;
        }

        public bool SetWaferType(SubstrateSizes size, ContourType type)
        {
            int changeSize = 0;

            switch (size)
            {
                case SubstrateSizes.ThreeHundredMM: changeSize = 300; break;
                case SubstrateSizes.TwoHundredMM: changeSize = 200; break;
            }

            string parameters = string.Format("{0}", changeSize);
            var os = MessageSend("WFS", parameters, SEND_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name} SetWaferType(WFS) failed.");

                return false;
            }

            // WFT 1<CR><LF> : notch type
            // WFT 2<CR><LF> : flat type
            parameters = string.Format("{0}", (type == ContourType.Notch) ? 1 : 2);
            os = MessageSend("WFT", parameters, SEND_TIMEOUT);
            if (os == false)
            {
                AppLogger.Error()($"{Name} SetWaferType(WFT) failed.");

                return false;
            }

            AppLogger.Info()($"{Name} device SetWafer Type : {size} / {type}");
            return true;
        }

        private void CreateErrorList()
        {
            prealignerErrorList = new List<TISErrors>();

            prealignerErrorList.Add(new TISErrors("E000", "none error"));
            prealignerErrorList.Add(new TISErrors("E002", "not comeback origion position"));
            prealignerErrorList.Add(new TISErrors("E003", "timeout during wafer existence checking"));
            prealignerErrorList.Add(new TISErrors("E004", "abnormal e-stop input"));
            prealignerErrorList.Add(new TISErrors("E008", "over-run status"));


            prealignerErrorList.Add(new TISErrors("E009", "barrier check"));
            prealignerErrorList.Add(new TISErrors("E011", "driver is not ready state"));
            prealignerErrorList.Add(new TISErrors("E012", "driver is alarm state"));
            prealignerErrorList.Add(new TISErrors("E020", "motor error occurred"));
            prealignerErrorList.Add(new TISErrors("E157", "command parameter is not currect"));



            prealignerErrorList.Add(new TISErrors("E211", "wafer disapper align working"));
            prealignerErrorList.Add(new TISErrors("E213", "wafer is not on align stage"));
            prealignerErrorList.Add(new TISErrors("E215", "wafer sensor out of work range"));
            prealignerErrorList.Add(new TISErrors("E216", "wafer sampling data not enough"));
            prealignerErrorList.Add(new TISErrors("E217", "doesn't align working"));


            prealignerErrorList.Add(new TISErrors("E218", "pre-aligner is not initialize state"));
            prealignerErrorList.Add(new TISErrors("E219", "z-axis out of working range"));
            prealignerErrorList.Add(new TISErrors("E220", "command error"));
            prealignerErrorList.Add(new TISErrors("E221", "the input value of the line sensor is abnormal"));
            prealignerErrorList.Add(new TISErrors("E222", "initialization cannot be normally done."));
        }

        public TISAlignerResults GetAlignerPosition()
        {
            //
            var osr = MessageSend("APS", null, SEND_TIMEOUT);

            if (osr == false)
            {
                AppLogger.Error()($"{Name} GetAlignerPosition(APS) failed.");

                return null;
            }

            string[] datas = ResponseParameters.Split(',');

            //
            TISAlignerResults result = new TISAlignerResults();

            // mm to micron
            result.r = Double.Parse(datas[0]) * 0.0018 * 1000.0;     // Rotation 1pulse = 0.0018 to micro degree
            result.x = Double.Parse(datas[1]) * 1000.0;              // X   0.01mm = 10um
            result.y = Double.Parse(datas[2]) * 1000.0;              // Y

            AppLogger.Error()("** r={0} x={1} y={2}", result.r, result.x, result.y);

            return result;
        }

        public override TISErrors ParseErrorMessage(string command, string parameter)
        {
            // if query of error ? true is ignore
            if (command == "ERR")
            {
                return null;
            }

            var error = prealignerErrorList.Find(s => s.ErrorCode == parameter);
            if (error != null)
            {
                return error;
            }

            return base.ParseErrorMessage(command, parameter);
        }

        #endregion

        #region constructors
        public DevicePrealignerTIS(NetworkConfig config) 
            : base(DeviceTypes.PreAligner_TIS, config)
        {
            CreateErrorList();
        }
        #endregion
    }
}
