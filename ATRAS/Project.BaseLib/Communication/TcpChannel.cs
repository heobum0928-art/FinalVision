using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using Project.BaseLib.Extension;
using Project.BaseLib.Utils;

namespace Project.BaseLib.Communication
{

    public class TcpChannel : NotifyPropertyChanged, ICommunication, IDisposable
    {
        private Socket socket;
        private Socket listener;
        private string connectHostName;
        private int connectPort;
        private bool disposed = false;

        public event ClientConnected OnClientConnected;
        public event ClientDisconnected OnClientDisconnected;
        public event MessageReceived OnMessageReceived;

        public bool Connected
        {
            get
            {
                if (socket != null)
                    return socket.Connected;
                else
                    return false;
            }
        }

        static public bool IsLocalIP(string IP)
        {
            return string.IsNullOrEmpty(IP) || NetworkExtensions.IsLocalAddress(IP);
        }

        // server...
        public TcpChannel(int port, ClientConnected clientConnected = null,
                                    ClientDisconnected clientDisconnected = null,
                                    MessageReceived messageReceived = null              )
        {
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

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(1);

            //
            Task.Run(() =>
            {
                //
                while (!disposed)
                {
                    if (Connected)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    try
                    {
                        socket = listener.Accept();

                        //
                        // socket.ExclusiveAddressUse = false;
                        socket.LingerState = new LingerOption(false, 0);
                        socket.NoDelay = true;
                        //
                        socket.ReceiveBufferSize = 8192;
                        socket.ReceiveTimeout = 0;
                        socket.SendBufferSize = 0;
                        socket.SendTimeout = 0;
                        //
                        socket.Ttl = 128;
                    }
                    catch (System.ObjectDisposedException e)
                    {
                        break;
                    }
                    catch (Exception e)
                    {
                        continue;
                    }

                    if (OnClientConnected != null)
                        OnClientConnected();

                    StartReceiver();
                }
            });
        }

        // client
        public TcpChannel(string hostname, int port, ClientConnected clientConnected = null,
                                                     ClientDisconnected clientDisconnected = null,
                                                     MessageReceived messageReceived = null)
        {
            if (clientConnected != null)
            {
                OnClientConnected += clientConnected;
            }

            if (clientDisconnected != null)
            {
                OnClientDisconnected += clientDisconnected;
            }

            //
            if (messageReceived != null)
            {
                OnMessageReceived += messageReceived;
            }

            //
            disposed = false;
            connectHostName = hostname;
            connectPort = port;
        }

        public void Dispose()
        {
            DisposeClient();
            if (listener != null)
            {
                listener.Dispose();
            }
            disposed = true;
        }

        public void DisposeClient()
        {
            if (socket != null && !disposed)
            {
                socket.Dispose();
                socket = null;
            }
        }

        // for Server
        public string ClientIPAddress
        {
            get
            {
                return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            }
        }

        // for client
        public bool Connect()
        {
            try
            {
                if (IsLocalIP(connectHostName)) connectHostName = "127.0.0.1";

                var ep = new IPEndPoint(IPAddress.Parse(connectHostName), connectPort);
                socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.ExclusiveAddressUse = false;
                socket.LingerState = new LingerOption(false, 0);
                socket.NoDelay = true;

                //
                socket.ReceiveBufferSize = 8192;
                socket.ReceiveTimeout = 0;
                socket.SendBufferSize = 0;
                socket.SendTimeout = 0;
                //

                socket.Ttl = 128;

                socket.Connect(ep);

                if (OnClientConnected != null)
                    OnClientConnected();

                StartReceiver();

                return true;
            }
            catch (Exception e)
            {

                string message = e.ToString();

                DisposeClient();

                if (OnClientDisconnected != null)
                {
                    OnClientDisconnected();
                }

                throw;
            }
            return false;
        }


        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        // Common
        private void StartReceiver()
        {
            Thread receiverThread = new Thread(ReceiverThread);
            receiverThread.Priority = ThreadPriority.Highest;
            receiverThread.Start();
        }

        private void ReceiverThread()
        {
            Byte[] buffer = new Byte[socket.ReceiveBufferSize];
            int receivedLength = 0;

            while (Connected)
            {
                try
                {
                    receivedLength = socket.Receive(buffer);
                    // deviceLogger.Format("received={0}", receivedLength);

                    if (receivedLength == 0) break;

                    if (OnMessageReceived != null)
                        OnMessageReceived(buffer, receivedLength);
                }
                catch (Exception ee)
                {
                    // throw (ee);

                    int k = 0;
                    
                }
            }

            DisposeClient();

            if (OnClientDisconnected != null)
            {
                OnClientDisconnected();
            }
        }

        public int Send(byte[] buffer, int length)
        {
            if (socket != null && socket.Connected)
            {
                int sendLength = socket.Send(buffer, length, SocketFlags.None);
                return length;
            }

            return 0;
        }

        public int Send(string message)
        {
            byte[] enbyte = ASCIIEncoding.ASCII.GetBytes(message);
            return Send(enbyte, message.Length);
        }

    }
}
