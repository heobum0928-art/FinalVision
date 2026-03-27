using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.HWC
{
    public class TISErrors
    {
        private string errorCode;
        public string ErrorCode
        {
            get
            {
                return errorCode;
            }
        }

        private string description;
        private string recoveryCommand;
        public TISErrors(string error, string desc, string rcommand = null)
        {
            errorCode = error;
            description = desc;
            recoveryCommand = rcommand == null ? string.Empty : rcommand;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(recoveryCommand))
                return string.Format("{0}-{1}", errorCode, description);

            return string.Format("{0}-{1} [{2}]", errorCode, description, recoveryCommand);
        }
    }

    public class TISSensors
    {
        private string rawString = null;
        protected BitArray[] bits = null;
        //
        public TISSensors()
        {
        }

        public TISSensors(string ioValues, int nbyte)
        {
            SetIOValues(ioValues, nbyte);
        }

        public void SetIOValues(string ioValues, int nbyte)
        {
            rawString = ioValues;
            bits = new BitArray[nbyte];

            if (ioValues != null)
            {
                if (bits == null) bits = new BitArray[nbyte];
                string[] hexValuesSplit = ioValues.Split(',');
                int index = 0;
                foreach (string hex in hexValuesSplit)
                {
                    int value = Convert.ToInt32(hex, 16);
                    bits[index++] = new BitArray(BitConverter.GetBytes(value));
                }
            }
            else
            {
                for (int i = 0; i < nbyte; i++)
                {
                    bits[i] = new BitArray(8);
                }
            }
        }

        public override string ToString()
        {
            if (rawString != null) return rawString;

            if (bits == null) return "null";

            var ivalue = new int[1];
            string result = string.Empty;
            foreach (var v in bits)
            {
                v.CopyTo(ivalue, 0);
                result += string.Format("{0:X2},", ivalue[0]);
            }
            return result.TrimEnd(',');
        }

        public BitArray[] Bits
        {
            get
            {
                return bits;
            }
        }
    }

    public class DeviceTIS : DeviceCom
    {
        #region fields
        protected InitializationStates _InitializationState;
        protected int SEND_TIMEOUT = 60000;

        protected const int REQUEST_TIMEOUT = 10000;
        protected const int ACTION_TIMEOUT = 120000;

        protected List<TISErrors> commonErrorList;

        //protected List<string> errorCheckCommandList;

        //
        protected ManualResetEvent commandFinishedEvent;
        //
        private string responseCommand;
        protected string responseParameters;
        private string recevingData;

        protected string mapData = string.Empty;

        private const byte delemeterCR = 0x0d;
        private const byte delemeterLF = 0x0a;

        protected bool parse_log = true;
        #endregion

        #region propertise
        public string ResponseParameters
        {
            get
            {
                return responseParameters;
            }
        }

        //public new WTRConfig Config
        //{
        //    get
        //    {
        //        return base.Config as WTRConfig;
        //    }
        //}

        public int Device_Index
        {
            get
            {
                
                if(Config is WTRConfig)
                {
                    return (Config as WTRConfig).RobotIndex;
                }
                else if(Config is LoadPortConfig)
                {
                    return (Config as LoadPortConfig).LoadPortIndex;
                }

                return 0;
            }
        }
        #endregion

        #region methods
        public void InsertErrors()
        {
            commonErrorList = new List<TISErrors>();

            commonErrorList.Add(new TISErrors("E01", "Received not execute command during operating"));
            commonErrorList.Add(new TISErrors("E02", "Received run command when error"));
            commonErrorList.Add(new TISErrors("E03", "Can't received this situation. Please see command list"));
            commonErrorList.Add(new TISErrors("E04", "Again received stop command. Please waiting until stopped"));
            commonErrorList.Add(new TISErrors("E05", "Exceed range of data"));
            commonErrorList.Add(new TISErrors("E06", "Format error"));
            commonErrorList.Add(new TISErrors("E07", "Exceed moving range"));
            commonErrorList.Add(new TISErrors("E08", "Position number error"));
            commonErrorList.Add(new TISErrors("E09", "No data"));
            commonErrorList.Add(new TISErrors("E10", "This command can't perform. please verify C-HOST command"));
            commonErrorList.Add(new TISErrors("E11", "POS range so big"));
            commonErrorList.Add(new TISErrors("E12", "Not exist defined macro"));
            commonErrorList.Add(new TISErrors("E13", "Duplicated macro"));
            commonErrorList.Add(new TISErrors("E99", "Connect error"));
        }

        public virtual bool HWInitialize()
        {
            var os = ClearError();
            if (os == false)
            {
                return false;
            }

            os = Version();
            if (os == false)
            {
                return false;
            }

            return ClearError();
        }

        public virtual bool Homing()
        {
            var osr = MessageSend("ORG", null, ACTION_TIMEOUT);
            if (osr == false)
            {
                AppLogger.Error()($"[{Name}-{Device_Index}] device Homing(ORG) command fail.");
                return false;
            }


            _InitializationState = InitializationStates.Initialized;
            return true;
        }

        public bool Version()
        {
            return MessageSend("VER", null, REQUEST_TIMEOUT);
        }

        public bool Stop()
        {
            var osr = MessageSend("AES", null, ACTION_TIMEOUT);
            if (osr == false)
            {
                AppLogger.Error()($"[{Name}-{Device_Index}] device stop(AES) command fail.");
                return false;
            }

            return true;
        }

        //
        public bool ClearError()
        {
            //var  oss = MessageSend("ERR", null, REQUEST_TIMEOUT);
            //if (oss == false)
            //{
            //    return false;
            //}

            //if (ResponseParameters != "0" &&    // for Robot
            //    ResponseParameters != "000")    // for Loadport
            //{
            //    RequestErrorMessage(ResponseParameters);

            //    // query error..
            //    oss = MessageSend("DRT", null, REQUEST_TIMEOUT);
            //    if (oss == false)
            //    {
            //        return false;
            //    }
            //}


            // query error..
            var oss = MessageSend("DRT", null, REQUEST_TIMEOUT);
            if (oss == false)
            {
                AppLogger.Error()($"[{Name}-{Device_Index}] device ClearError(DRT) command fail.");
                return false;
            }

            return true;
        }

        public override bool ParseMessage(string message, bool isLog)
        {
            if (message == "Encoder error reset 1 : *Protection error*")
                return false;

            if (message == "Reboot encoder")
                return false;
            


            if (isLog == true)
                AppLogger.Debug()($"{Name}-{Device_Index} << {message}");
            return true;
        }

        public void RequestErrorMessage(string errorMsg)
        {
            MessageSend("ERD", null, REQUEST_TIMEOUT);
        }

        public virtual TISErrors ParseErrorMessage(string command, string parameter)
        {
            var error = commonErrorList.Find(s => s.ErrorCode == parameter);
            if (error != null)
            {
                return error;
            }

            return null;
        }

        public override void MessageReceived(byte[] message, int length)
        {
            for (int i = 0; i < length; i++)
            {
                switch (message[i])
                {
                    case delemeterCR:
                        break;
                    case delemeterLF:
                        {
                            if (!ParseMessage(recevingData, parse_log))
                            {
                                recevingData = string.Empty;
                                continue;
                            }

                            int index = recevingData.IndexOf(' ');
                            if (index < 0)
                            {
                                responseCommand = recevingData;
                                responseParameters = string.Empty;
                            }
                            else
                            {

                                responseCommand = recevingData.Substring(0, index);

                                responseParameters = recevingData.Substring(index + 1);


                                if(responseCommand == "MLD1")
                                {
                                    mapData = responseParameters;
                                }
                            }

                            if (commandFinishedEvent != null)
                            {
                                commandFinishedEvent.Set();
                            }
                            else
                            {
                                AppLogger.Debug()("<* {0} (Unknown data,{0})", recevingData);
                            }

                            recevingData = string.Empty;

                            break;
                        }

                    default:
                        {
                            char ch = Convert.ToChar(message[i]);
                            recevingData += ch;
                        }

                        break;
                }
            }
        }

        public virtual bool MessageSend(string command, string parameter, int waitTime, bool islog = true)
        {
            lock (this)
            {
                string message = string.Format("{0}", command);
                if (parameter != null)
                {
                    message += string.Format(" {0}", parameter);
                }

                if (islog)
                {
                    AppLogger.Debug()($"{Name}-{Device_Index} >> {message}");
                }

                if (commandFinishedEvent != null)
                {
                    AppLogger.Error()($"{Name}-{Device_Index} device SendError : CommandBusy.");
                    return false;
                    //return string.Empty;
                    //return new OperationStatus<string>("SendError", new FICDeviceMessageSendError("CommandBusy"));
                }

                //
                string newmessage = message + Convert.ToChar(delemeterCR) + Convert.ToChar(delemeterLF);

                commandFinishedEvent = new ManualResetEvent(false);
                responseCommand = string.Empty;
                responseParameters = string.Empty;
                recevingData = string.Empty;

                var os = base.MessageSend(newmessage);
                // if error ?
                if (os == false)
                {
                    commandFinishedEvent = null;
                    AppLogger.Error()($"{Name}-{Device_Index} device SendError.");
                    return false;
                    //return string.Empty;
                    //return new OperationStatus<string>("SendError", os.ErrorInfo);
                }

                bool waitResult = commandFinishedEvent.WaitOne(waitTime);
                commandFinishedEvent = null;

                //
                if (waitResult)
                {
                    if (command != responseCommand)
                    {
                        AppLogger.Error()($"{Name}-{Device_Index} device response message not match. ResponseCommand : {responseCommand}.");
                        return false;
                        //return string.Empty;
                        //return new OperationStatus<string>(responseCommand, new FICDeviceMessageReceiveError("Response message not match"));
                    }

                    if (responseParameters != "")
                    {
                        string saveParameter = responseParameters;
                        TISErrors rndError = ParseErrorMessage(responseCommand, responseParameters);
                        if (rndError != null)
                        {
                            AppLogger.Error()($"{Name}-{Device_Index} device message receive error. error code : {rndError.ToString()}.");
                            return false;
                            //return string.Empty;
                            //return new OperationStatus<string>(rndError.ErrorCode, new FICDeviceMessageReceiveError(rndError.ToString()));
                        }

                        // for recursive called query "ERR"
                        responseParameters = saveParameter;
                    }

                    return true;
                    //return new OperationStatus<string>(responseParameters, Status.Success);
                }
                AppLogger.Error()($"{Name}-{Device_Index} device time-out error. wait time : {waitTime} ms");
                return false;
                //return new OperationStatus<string>("TimeOut", new FICDeviceMessageTimeoutError());
            }
        }

        public virtual bool UserCommand(string command)
        {
            string[] array = command.Split(' ');

            if (array.Length < 2)
            {
                return MessageSend(command, null, SEND_TIMEOUT);
            }

            string parameter = command.Substring(array[0].Length + 1);
            return MessageSend(array[0], parameter, SEND_TIMEOUT);
        }

        #endregion

        #region constructors
        public DeviceTIS(DeviceTypes type, NetworkConfig config)
            : base(type, config)
        {
            InsertErrors();

            _InitializationState = InitializationStates.NotReady;
        }
        #endregion
    }
}
