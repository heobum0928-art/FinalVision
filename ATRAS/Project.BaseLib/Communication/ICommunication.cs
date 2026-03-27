using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    public delegate void ClientConnected();
    public delegate void ClientDisconnected();
    public delegate void MessageReceived(byte[] message, int length);

    public interface ICommunication : IDisposable
    {
        event ClientConnected OnClientConnected;
        event ClientDisconnected OnClientDisconnected;
        event MessageReceived OnMessageReceived;

        bool Connected { get; }
        
        int Send(byte[] buffer, int length);
        int Send(string message);

        bool Connect();

        bool Disconnect();
    }
}
