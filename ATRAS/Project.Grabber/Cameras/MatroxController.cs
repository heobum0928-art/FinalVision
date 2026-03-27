using Project.BaseLib.Communication;
using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class MatroxController : SerialChannel
    {
        #region fields
        protected int camera_number = 0;

        protected CommandDoneEvent _CommandDoneEvent = new CommandDoneEvent();

        protected ManualResetEvent commandFinishedEvent = null;

        protected const byte delemeter = 0x0d;

        protected string recvData = string.Empty;

        protected object obj_lock = new object();

        protected bool isSuccess = false;
        #endregion

        #region propertise
        public bool IsCommunicationSuccess
        {
            get { return isSuccess; }
        }
        protected bool IsCommandBusy
        {
            get { return _CommandDoneEvent.IsBusy(); }

            set
            {
                if (value == true)
                {
                    _CommandDoneEvent.Ready();
                }
                else
                {
                    _CommandDoneEvent.Reset();
                }
            }
        }
        protected bool CommandFinished
        {
            get
            {
                if (IsCommandBusy) return false;
                return true;
            }
            set
            {
                if (value == true)
                {
                    _CommandDoneEvent.Set();
                }
            }
        }
        #endregion

        #region methods
        protected void PacketReceived(byte[] message, int length)
        {
            for (int i = 0; i < length; i++)
            {
                recvData += Convert.ToChar(message[i]);

                if(recvData.Contains("USER>"))
                {
                    var list = recvData.Split("\r\n".ToArray());

                    foreach(var param in list)
                    {
                        if (param == "")
                            continue;

                        ParseMessage(param);
                    }

                    if (recvData.Contains("???"))
                        isSuccess = false;
                    else
                        isSuccess = true;

                    recvData = "";

                    if (commandFinishedEvent != null)
                    {
                        commandFinishedEvent.Set();
                    }
                }
            }
        }
        public void ParseMessage(string message)
        {
            AppLogger.Debug()("<< [{0}] camera {1}", camera_number, message);
        }
        protected void OnCommunicationConnected()
        {
            AppLogger.Info()("[{0}] camera connected.", camera_number);
        }
        protected void OnCommunicationDisconnected()
        {
            AppLogger.Info()("[{0}] camera disconnected.", camera_number);
        }
        public bool Connect()
        {
            try
            {
                if (Connected)
                {
                    AppLogger.Debug()("[{0}] camera Already Connected.");
                    return true;
                }

                string logMessage = string.Empty;

                AppLogger.Debug()("[{0}] camera connecting.", camera_number);
                return base.Connect();
            }
            catch (Exception e)
            {
                AppLogger.Error()(string.Format("[{0}] camera is not Connected.", camera_number));

                Dispose();
                return false;
            }
        }
        public bool Shutdown()
        {
            return Disconnect();
        }
        public bool MessageSend(byte[] message, int length)
        {
            try
            {
                int result = Send(message, length);
                if (result != length)
                {
                    AppLogger.Debug()("MessageSend Result is not matched(Length:{0} / Result:{1}", length, result);
                }
                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()("MessageSend Exception Error!");
                return false;
            }
     
            return false;
        }
        public bool MessageSend(string message)
        {
            byte[] enbyte = ASCIIEncoding.ASCII.GetBytes(message);
            return MessageSend(enbyte, enbyte.Length);
        }
        public bool MessageSendSync(string message, int WAIT_TIME = 5000)
        {
            try
            {
                isSuccess = false;
                commandFinishedEvent = new ManualResetEvent(false);

                AppLogger.Debug()(">> {0}", message);

                bool ret = MessageSend(message + Convert.ToChar(delemeter));
                if (!ret)
                {
                    commandFinishedEvent.Close();
                    commandFinishedEvent = null;
                    return false;
                }

                var waitResult = commandFinishedEvent.WaitOne(WAIT_TIME);
                commandFinishedEvent.Close();
                commandFinishedEvent = null;
                if (!waitResult)
                {
                    AppLogger.Error()("MessageSendSync Timeout! ({0} ms)", WAIT_TIME);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                AppLogger.Error()(string.Format("MessageSendSync Exception Error!"));
                return false;
            }
        }

        #endregion

        #region constructors
        public MatroxController(
            int camera_number = 0,
            string portName = "COM0",
            int baudRate = 9600,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            ClientConnected clientConnected = null,
            ClientDisconnected clientDisconnected = null,
            MessageReceived messageReceived = null)
            : base(portName,
                  baudRate,
                  parity,
                  databits,
                  stopbits,
                  clientConnected,
                  clientDisconnected,
                  messageReceived)
        {
            this.camera_number = camera_number;
            this.OnClientConnected += OnCommunicationConnected;
            this.OnClientDisconnected += OnCommunicationDisconnected;
            this.OnMessageReceived += PacketReceived;
        }
        #endregion


    }
}
