using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    public class SerialChannel : NotifyPropertyChanged, ICommunication
    {
        #region Field
        public event ClientConnected OnClientConnected;
        public event ClientDisconnected OnClientDisconnected;
        public event MessageReceived OnMessageReceived;

        private SerialPort comport = null;

        private string portName = string.Empty;
        private int bautRate = 9600;
        private Parity parity = Parity.None;
        private int databits = 8;
        private StopBits stopbits = StopBits.None;

        private bool isConnected = false;
        #endregion

        #region Properties
        public bool Connected
        {
            //set
            //{
            //    if(isConnected != value)
            //    {
            //        isConnected = value;
            //        OnPropertyChanged();
            //    }
            //}
            get
            {
                //return isConnected;

                if (comport == null)
                    return false;

                return comport.IsOpen;
            }
        }
        #endregion

        #region methods
        // Common
        public bool Connect()
        {
            if (comport == null)
            {
                Exception e = new Exception("SerialPort Object not alloced.");
                throw e;
            }

            if (Connected)
            {
                Disconnect();
            }

            comport.Open();

            if (comport.IsOpen == true)
            {
                if (OnClientConnected != null)
                    OnClientConnected();

                StartReceiver();
            }
            return Connected;
        }
        public void Dispose()
        {
            if (Connected)
                Disconnect();

            if (comport != null)
                comport.Dispose();

            comport = null;
        }
        private void StartReceiver()
        {
            Thread receiverThread = new Thread(ReceiverThread);
            receiverThread.Priority = ThreadPriority.Highest;
            receiverThread.Start();
        }
        private void ReceiverThread()
        {
            Byte[] buffer = new Byte[comport.ReadBufferSize];
            int receivedLength = 0;

            while (Connected)
            {
                try
                {
                    receivedLength = comport.Read(buffer, 0, buffer.Length);

                    if (OnMessageReceived != null)
                        OnMessageReceived(buffer, receivedLength);
                }
                catch (Exception e)
                {
                    AppLogger.Error()(e.Message);

                }
            }

            if (OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }
        }
        public void RagisterEvent(SerialDataReceivedEventHandler DataReceived = null, SerialErrorReceivedEventHandler ErrorReceived = null, SerialPinChangedEventHandler PinChanged = null)
        {
            if (DataReceived != null)
                comport.DataReceived += DataReceived;

            if (ErrorReceived != null)
                comport.ErrorReceived += ErrorReceived;

            if (PinChanged != null)
                comport.PinChanged += PinChanged;
        }
        public virtual bool Disconnect()
        {
            if (comport == null)
            {
                return true;
            }   

            if(Connected)
            {
                comport.Close();
                Thread.Sleep(100);
            }

            if(OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }

            return true;
        }



        // send & receive
        public virtual int Send(byte[] buffer, int length)
        {
            if(comport != null)
            {
                if(Connected)
                {
                    comport.Write(buffer, 0, buffer.Length);
                    return length;
                }
            }
            return 0;
        }
        public virtual int Send(string message)
        {
            byte[] enbyte = ASCIIEncoding.ASCII.GetBytes(message);
            return Send(enbyte, message.Length);
        }





        #endregion

        #region Constructor
        private SerialChannel() { }

        public SerialChannel(
            string portName = "COM0",
            int baudRate = 9600,
            Parity parity = Parity.None,
            int databits = 8,
            StopBits stopbits = StopBits.One,
            ClientConnected clientConnected = null,
            ClientDisconnected clientDisconnected = null,
            MessageReceived messageReceived = null)
        {
            this.portName = portName;
            this.bautRate = baudRate;
            this.parity = parity;
            this.databits = databits;
            this.stopbits = stopbits;

            comport = new SerialPort();
            comport.PortName = portName;
            comport.BaudRate = bautRate;
            comport.Parity = parity;
            comport.DataBits = databits;
            comport.StopBits = stopbits;

            var size = comport.ReadBufferSize;

            if (clientConnected != null)
            {
                OnClientConnected += clientConnected;
            }

            if (clientDisconnected != null)
            {
                OnClientDisconnected += clientDisconnected;
            }

            if (messageReceived != null)
            {
                OnMessageReceived += messageReceived;
            }
        }
        #endregion
    }
}
