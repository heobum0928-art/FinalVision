using Project.BaseLib.Communication;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{
    public class CommandDoneEvent
    {
        private ManualResetEvent commandDoneEvent = new ManualResetEvent(false);
        private bool isReady = false;
        private bool signaled = false;
        public bool Wait(int waitTime)
        {
            bool result = commandDoneEvent.WaitOne(waitTime);
            isReady = false;
            return signaled;
        }
        public bool IsBusy()
        {
            return isReady;
        }
        public void Set()
        {
            signaled = true;
            commandDoneEvent.Set();
        }
        public void Reset()
        {
            signaled = false;
            isReady = false;
            commandDoneEvent.Reset();
        }
        public void Ready()
        {
            signaled = false;
            isReady = true;
            commandDoneEvent.Reset();
        }
    }
    public class DeviceCom : DeviceBase
    {
        #region Field
        private bool reconnecting = false;
        private bool shutdown = false;

        protected CommandDoneEvent _CommandDoneEvent = new CommandDoneEvent();

        protected ManualResetEvent commandFinishedEvent = null;

        protected ICommunication channel;

        protected const byte delemeter = 0x0d;

        protected string recvMessage = string.Empty;
        protected string recvData = string.Empty;

        private NetworkConfig config;
        #endregion

        #region properties
        public NetworkConfig Config
        {
            get { return config; }
            set
            {
                config = value;
            }
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
        public void SetConfiguration(NetworkConfig config)
        {
            this.config = config;
        }

        public void PacketReceived(byte[] message, int length)
        {
            MessageReceived(message, length);
        }
        public virtual void MessageReceived(byte[] message, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (message[i] != delemeter)
                {
                    recvData += Convert.ToChar(message[i]);
                    //recvMessage += Convert.ToChar(message[i]);
                }
                else
                {
                    recvMessage = recvData;
                    AppLogger.Debug()("<< {0}", recvMessage);

                    ParseMessage(recvMessage, false);
                    recvData = "";

                    //
                    if (commandFinishedEvent != null)
                    {
                        commandFinishedEvent.Set();
                    }
                }
            }
        }
        public void OnCommunicationConnected()
        {
            AppLogger.Info()("Connected.");
            OnDeviceConnected();
        }
        public void OnCommunicationDisconnected()
        {
            AppLogger.Info()("Disconnected.");
            OnDeviceDisconnected();
        }
        public override bool Connect()
        {
            try
            {

                if (channel != null)
                {
                    if (channel.Connected)
                    {
                        AppLogger.Debug()("Already Connected.");
                        return true;
                    }
                }
                shutdown = false;
                string logMessage = string.Empty;
                if (Config.CommunicationType == CommunicationTypes.Tcp)
                {
                    channel = new TcpChannel(Config.TcpAddress, Config.TcpPort,
                        OnCommunicationConnected,
                        OnCommunicationDisconnected,
                        PacketReceived);

                    logMessage = Config.ToTcpString();
                }
                else
                {
                    channel = new SerialChannel(Config.SerialPort,
                        Config.BaudRate, Config.Parity, Config.DataBits, Config.StopBits,
                        OnCommunicationConnected,
                        OnCommunicationDisconnected,
                        PacketReceived);

                    logMessage = Config.ToSerialString();
                }
                AppLogger.Debug()("Name : {0}, {1} Connecting.", Name, logMessage);
                return channel.Connect();
            }
            catch (Exception e)
            {
                AppLogger.Error(e, string.Format("Name : {0}, {1} is not Connected.", Name, Config.ToString()));

                Dispose();
                return false;
            }
        }
        public override bool Disconnect()
        {
            if (channel != null)
            {
                channel.Dispose();
                channel = null;
                OnDeviceDisconnected();
            }
            return true;
        }
        public override bool Shutdown()
        {
            base.Shutdown();
            shutdown = true;

            Disconnect();

            return true;
        }
        public virtual bool MessageSend(byte[] message, int length)
        {
            if (channel != null)
            {
                if (channel.Connected)
                {
                    try
                    {
                        int result = channel.Send(message, length);
                        if (result != length)
                        {
                            AppLogger.Debug()("MessageSend Result is not matched(Length:{0} / Result:{1}", length, result);
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        AppLogger.Error(e, "MessageSend Exception Error!");
                        return false;
                    }
                }
            }

            AppLogger.Error()("{0} channel is not connected.", Name);
            return false;
        }
        public virtual bool MessageSend(string message)
        {
            byte[] enbyte = ASCIIEncoding.ASCII.GetBytes(message);
            return MessageSend(enbyte, enbyte.Length);
        }

        public virtual string MessageSendSync(string message, int WAIT_TIME = 5000)
        {
            try
            {
                recvMessage = "";
                commandFinishedEvent = new ManualResetEvent(false);

                AppLogger.Debug()(">> {0}", message);

                bool ret = MessageSend(message + Convert.ToChar(delemeter));
                if (!ret)
                {
                    commandFinishedEvent.Close();
                    commandFinishedEvent = null;
                    return "";
                }

                var waitResult = commandFinishedEvent.WaitOne(WAIT_TIME);
                commandFinishedEvent.Close();
                commandFinishedEvent = null;
                if (!waitResult)
                {
                    AppLogger.Error()("MessageSendSync Timeout! ({0} ms)", WAIT_TIME);
                    return "";
                }

                return recvMessage;
            }
            catch (Exception e)
            {
                AppLogger.Error(e, string.Format("MessageSendSync Exception Error!"));
                return "";
            }
        }

        public virtual bool ParseMessage(string message, bool isLog)
        {
            if(isLog == true)
                AppLogger.Debug()("Received Message by the DeviceCom : {0}", message);
            return true;
        }
        public void Dispose()
        {
            if (channel != null)
                channel.Dispose();

            channel = null;
        }
        #endregion

        #region constructor
        public DeviceCom(DeviceTypes deviceType, NetworkConfig config)
            : base(deviceType)
        {
            this.config = config;
        }
        #endregion
    }
}
