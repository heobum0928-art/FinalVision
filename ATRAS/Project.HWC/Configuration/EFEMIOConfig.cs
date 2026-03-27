using Project.BaseLib.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    [DataContract]
    [System.SerializableAttribute]
    public class EFEMIOConfig : DeviceTypeConfig
    {
        #region fields
        protected string _IpAddress;

        protected int _Port;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string IpAddress
        {
            get
            {
                return _IpAddress;
            }

            set
            {
                _IpAddress = value;
                OnPropertyChanged();
            }
        }
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public EFEMIOConfig()
        {
            _IpAddress = "127.0.0.1";

            _Port = 502;
        }
        #endregion



    }
}
