using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.HW
{

    [DataContract]
    [System.SerializableAttribute]
    public class HWConfigBase : NotifyPropertyChanged
    {
        #region fields
        #endregion

        #region propertise

        #endregion

        #region methods

        #endregion

        #region constructors
        public HWConfigBase()
        {
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class DeviceTypeConfig : HWConfigBase
    {
        #region fields
        protected DeviceTypes _DeviceType;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public DeviceTypes DeviceType
        {
            get
            {
                return _DeviceType;
            }

            set
            {
                if (_DeviceType != value)
                {
                    _DeviceType = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public DeviceTypeConfig()
        {
            _DeviceType = DeviceTypes.NotUsed;
        }

        public DeviceTypeConfig(DeviceTypes DeviceType)
        {
            _DeviceType = DeviceType;
        }
        #endregion
    }
    [DataContract]
    [System.SerializableAttribute]
    public class NetworkConfig : DeviceTypeConfig
    {
        #region Fields
        private CommunicationTypes _CommunicationType = CommunicationTypes.NotUsed;

        // Ethernet
        private String _TcpAddress = "localhost";
        private Int32 _TcpPort = 0;

        //Serial Communication
        private string _SerialPort = "COM0";
        private int _BaudRate = 9600;
        private System.IO.Ports.Parity _Parity = System.IO.Ports.Parity.None;
        private int _DataBits = 8;
        private System.IO.Ports.StopBits _StopBits = System.IO.Ports.StopBits.None;

        #endregion

        #region Constructors
        public NetworkConfig()
            : base(DeviceTypes.NotUsed)
        {
            this._CommunicationType = CommunicationTypes.NotUsed;

            this._TcpAddress = "localhost";
            this._TcpPort = 0;

            this._SerialPort = "COM0";
            this._BaudRate = 9600;
            this._Parity = System.IO.Ports.Parity.None;
            this._DataBits = 8;
            this._StopBits = System.IO.Ports.StopBits.None;

            this._DeviceType = DeviceTypes.NotUsed;
            //this._DeviceType = "NotUsed";
        }

        //public HWConfigBase(String TcpAddress = "localhost", Int32 TcpPort = 0, string DeviceType = "NotUsed")
        public NetworkConfig(String TcpAddress = "localhost", Int32 TcpPort = 0, DeviceTypes DeviceType = DeviceTypes.NotUsed)
            : base(DeviceType)
        {
            this._CommunicationType = CommunicationTypes.Tcp;

            this._TcpAddress = TcpAddress;
            this._TcpPort = TcpPort;

            // NotUsed
            this._SerialPort = "NotUsed";
            this._BaudRate = 0;
            this._Parity = System.IO.Ports.Parity.None;
            this._DataBits = 0;
            this._StopBits = System.IO.Ports.StopBits.None;
        }

        public NetworkConfig(
            string SerialPort = "COM0",
            int BaudRate = 9600,
            System.IO.Ports.Parity Parity = System.IO.Ports.Parity.None,
            int DataBits = 8,
            System.IO.Ports.StopBits StopBits = System.IO.Ports.StopBits.None,
            DeviceTypes DeviceType = DeviceTypes.NotUsed)
            : base(DeviceType)
        {
            this._CommunicationType = CommunicationTypes.Serial;


            this._SerialPort = SerialPort;
            this._BaudRate = BaudRate;
            this._Parity = Parity;
            this._DataBits = DataBits;
            this._StopBits = StopBits;

            this._DeviceType = DeviceType;

            // NotUsed
            this._TcpAddress = "localhost";
            this._TcpPort = 0;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public CommunicationTypes CommunicationType
        {
            get
            {
                return _CommunicationType;
            }
            set
            {
                if (_CommunicationType != value)
                {
                    _CommunicationType = value;
                    OnPropertyChanged();
                }
            }
        }

        // Ethernet
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public String TcpAddress
        {
            get
            {
                return _TcpAddress;
            }
            set
            {
                if (_TcpAddress != value)
                {
                    _TcpAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 TcpPort
        {
            get
            {
                return _TcpPort;
            }
            set
            {
                if (_TcpPort != value)
                {
                    _TcpPort = value;
                    OnPropertyChanged();
                }
            }
        }

        // Serial Communications
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string SerialPort
        {
            get { return _SerialPort; }
            set
            {
                if (_SerialPort != value)
                {
                    _SerialPort = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int BaudRate
        {
            get { return _BaudRate; }
            set
            {
                if (_BaudRate != value)
                {
                    _BaudRate = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public System.IO.Ports.Parity Parity
        {
            get { return _Parity; }
            set
            {
                if (_Parity != value)
                {
                    _Parity = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int DataBits
        {
            get { return _DataBits; }
            set
            {
                if (_DataBits != value)
                {
                    _DataBits = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public System.IO.Ports.StopBits StopBits
        {
            get { return _StopBits; }
            set
            {
                if (_StopBits != value)
                {
                    _StopBits = value;
                    OnPropertyChanged();
                }
            }
        }


        #endregion

        #region methods
        //public override bool Equals(HWConfigBase config)
        public override bool Equals(object obj)
        {
            NetworkConfig config = obj as NetworkConfig;

            if(_TcpAddress.Equals(config.TcpAddress)    &&
                _TcpPort.Equals(config._TcpPort)        &&
                _SerialPort.Equals(config._SerialPort)  &&
               _BaudRate.Equals(config._BaudRate)       &&
               _Parity.Equals(config._Parity)           &&
               _DataBits.Equals(config._DataBits)       &&
               _StopBits.Equals(config._StopBits)       &&
               _DeviceType.Equals(config._DeviceType))
            {
                return true;
            }

            return false;
        }

        public void Clear()
        {
            this._CommunicationType = CommunicationTypes.NotUsed;

            // Tcp
            this._TcpAddress = TcpAddress;
            this._TcpPort = TcpPort;

            // Serial Communication
            this._SerialPort = "COM0";
            this._BaudRate = 9600;
            this._Parity = System.IO.Ports.Parity.None;
            this._DataBits = 8;
            this._StopBits = System.IO.Ports.StopBits.None;
            this._DeviceType = DeviceTypes.NotUsed;
        }

        public string ToTcpString()
        {
            return string.Format("CommunicationType : {0}, " +
                        "TcpAddress : {1}, " +
                        "TcpPort : {2}, " +
                        "DeviceType : {3} ",
                        _CommunicationType,
                        _TcpAddress,
                        _TcpPort,
                        _DeviceType);
        }

        public string ToSerialString()
        {
            return string.Format("CommunicationType : {0}, " +
                        "SerialPort : {1}, " +
                        "BautRate : {2}, " +
                        "Parity : {3}, " +
                        "DataBits : {4}, " +
                        "StopBits : {5}, " +
                        "DeviceType : {6} ",
                        _CommunicationType,
                        _SerialPort,
                        _BaudRate,
                        _Parity,
                        _DataBits,
                        _StopBits,
                        _DeviceType);
        }

        public override string ToString()
        {
            return string.Format("CommunicationType : {0}, " +
                                    "TcpAddress : {1}, " +
                                    "TcpPort : {2}, " +
                                    "SerialPort : {3}, " +
                                    "BautRate : {4}, " +
                                    "Parity : {5}, " +
                                    "DataBits : {6}, " +
                                    "StopBits : {7}, " +
                                    "DeviceType : {8} ",
                                    _CommunicationType,
                                    _TcpAddress,
                                    _TcpPort,
                                    _SerialPort,
                                    _BaudRate,
                                    _Parity,
                                    _DataBits,
                                    _StopBits,
                                    _DeviceType);
        }
        #endregion
    }
}
