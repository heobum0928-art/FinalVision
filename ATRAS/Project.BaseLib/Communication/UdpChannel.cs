using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

using System.Threading;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
  public delegate void UdpMessageReceived(byte[] message);
  public class UdpChannel : IDisposable
  {
    private UdpClient client;
    private UdpClient listener;
    private bool disposed = false;
    public event UdpMessageReceived OnMessageReceived;


    // Construct a receive only Channel
    public UdpChannel(int localPort, IPAddress localAddress, UdpMessageReceived messageReceived) : this(0, null, localPort, localAddress, messageReceived)
    {
    }

    //Construct a Send/Receive channel (if localAddress is null than it is a Send only channel)
    public UdpChannel(int remotePort, IPAddress remoteAddress, int localPort = 0, IPAddress localAddress = null, UdpMessageReceived messageReceived = null)
    {
      if(remoteAddress != null)
      {
        client = new UdpClient();
        if (remoteAddress.Equals(IPAddress.Broadcast))
          client.EnableBroadcast = true;

        client.Connect(remoteAddress, remotePort);
      }

      if(localAddress != null)
      {
        listener = new UdpClient();
        listener.ExclusiveAddressUse = false;
        listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        IPEndPoint localEP;
        if (localAddress != null)
        {
          localEP = new IPEndPoint(localAddress, localPort);
        }
        else
          localEP = new IPEndPoint(IPAddress.Any, localPort);

        listener.Client.Bind(localEP);

        if(messageReceived != null)
        {
          StartReceive(messageReceived);
        }
      }
    }
    public int Send(byte[] buffer)
    {
      return client.Send(buffer, buffer.Length);
    }

    public async Task<int> SendAsync(byte[] buffer)
    {
      return await client.SendAsync(buffer, buffer.Length);
    }
    public void StartReceive(UdpMessageReceived messageReceived = null)
    {
      if (messageReceived != null)
        OnMessageReceived += messageReceived;

      listener.BeginReceive(HandleReceive, null);
    }

    private void HandleReceive(IAsyncResult ar)
    {
      if(!disposed)
      {
        var remoteEP = new IPEndPoint(IPAddress.Any, 0);
        byte[] receiveBuffer = listener.EndReceive(ar, ref remoteEP);

        listener.BeginReceive(HandleReceive, null);

        if (OnMessageReceived != null)
          OnMessageReceived(receiveBuffer);
      }
    }

    public void Dispose()
    {
      disposed = true;
      if(client != null)
      {
        client.Close();
      }
      if(listener != null)
      {
        listener.Close();
      }
    }
  }
}
