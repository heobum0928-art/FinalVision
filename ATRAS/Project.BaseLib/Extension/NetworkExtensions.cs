using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace Project.BaseLib.Extension
{
  public static class NetworkExtensions
  {
    public static bool TryResolve(string name, out IPAddress ipAddress)
    {
      if (IPAddress.TryParse(name, out ipAddress))
        return true;

      try
      {
        IPHostEntry entry = Dns.GetHostEntry(name);
        ipAddress = entry.AddressList[0];
        for(int index = 1; index < entry.AddressList.Length; ++index)
        {
          if(entry.AddressList[index].AddressFamily == AddressFamily.InterNetwork)
          {
            ipAddress = entry.AddressList[index];
            break;
          }
        }
        return true;
      }
      catch(SocketException socketException)
      {
        if(socketException.ErrorCode == 11001) // host not found
        {
          ipAddress = null;
          return false;
        }
        throw;
      }
    }

    public static string GetHostNameFromIpAddress(string ipString)
    {
      IPAddress ipAddress;
      if (!IPAddress.TryParse(ipString, out ipAddress))
        return ipString;
      else
        return Dns.GetHostEntry(ipAddress).HostName;
    }

    public static IPAddress Resolve(string nameOrAddress)
    {
      IPAddress ipAddress;
      return TryResolve(nameOrAddress, out ipAddress) ? ipAddress : null;
    }

    public static bool IsLocalAddress(string nameOrAddress)
    {
      try
      {
        IPAddress[] hostIPs = Dns.GetHostAddresses(nameOrAddress);
        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

        foreach(IPAddress hostIP in hostIPs)
        {
          if(IPAddress.IsLoopback(hostIP)) // loopback address (127.0.0.1)
            return true;

          foreach(IPAddress ipAddress in localIPs)
          {
            if (hostIP.Equals(ipAddress))
              return true;
          }
        }
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e);
      }
      return false;
    }

    public static bool IsLocalAddress(IEnumerable<IPAddress> hostIPs)
    {
      try
      {
        IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

        foreach(IPAddress hostIP in hostIPs)
        {
          if (localIPs.Contains(hostIP))
            return true;
        }
      }
      catch(Exception e)
      {
        System.Diagnostics.Debug.WriteLine(e);
      }
      return false;
    }
  }
}
