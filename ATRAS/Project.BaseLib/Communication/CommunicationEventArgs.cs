using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    [DataContract]
    public class CommunicationEventArgs : EventArgs
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string IpAddress { get; set; }

        [DataMember]
        public bool Connected { get; set; }

        [DataMember]
        public Type ServiceType { get; set; }

        public CommunicationEventArgs(string name, bool connected, Type serviceType, string ipAddress)
        {
            this.Name = name;
            this.Connected = connected;
            this.ServiceType = serviceType;
            this.IpAddress = ipAddress;
        }
    }
}
