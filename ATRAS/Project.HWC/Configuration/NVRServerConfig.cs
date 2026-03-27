using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project.HWC
{
    [DataContract]
    [System.SerializableAttribute]
    public class CCTVConfig
    {
        #region fields
        protected string _IPAddress;
        protected int _PortNumber;
        protected string _UserName;
        protected string _Password;

        protected int _UI_Index;

        protected int _NVR_Channel;

        protected int _Equipment_Number;

        protected string _DeviceName;


        protected IntPtr _PictureBoxHandle;

        protected UIElement _GroupBox;
        #endregion

        #region propertise
        [System.Xml.Serialization.XmlIgnore]
        public IntPtr PictureBoxHandle
        {
            get
            {
                return _PictureBoxHandle;
            }

            set
            {
                _PictureBoxHandle = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string IPAddress
        {
            get { return _IPAddress; }

            set 
            { 
                _IPAddress = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int PortNumber
        {
            get { return _PortNumber; }

            set 
            {
                _PortNumber = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string UserName
        {
            get { return _UserName; }

            set 
            { 
                _UserName = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string Password
        {
            get { return _Password; }

            set 
            { 
                _Password = value;
            }
        }


        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string DeviceName
        {
            get { return _DeviceName; }

            set
            {
                _DeviceName = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int UI_Index
        {
            get { return _UI_Index; }

            set
            {
                _UI_Index = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int NVR_Channel
        {
            get { return _NVR_Channel; }

            set
            {
                _NVR_Channel = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int Equipment_Number
        {
            get { return _Equipment_Number; }

            set
            {
                _Equipment_Number = value;
            }
        }


        #endregion

        #region methods

        #endregion

        #region constructors
        public CCTVConfig()
        {
            _IPAddress = string.Empty;
            _PortNumber = 0;
            _UserName = string.Empty;
            _Password = string.Empty;

            _DeviceName = string.Empty;

            _UI_Index = 0;
            _NVR_Channel = 0;
            _Equipment_Number = 0;
        }
        #endregion

    }

    [DataContract]
    [System.SerializableAttribute]
    public class NVRServerConfig : DeviceTypeConfig
    {
        #region fields

        protected string _IPAddress;

        protected int _PortNumber;

        protected string _UserName;

        protected string _Password;

        protected string _EventVideoDownloadPath;

        protected int _Index;

        protected List<CCTVConfig> _CCTVConfig;

        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string IPAddress
        {
            get { return _IPAddress; }

            set
            {
                _IPAddress = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int PortNumber
        {
            get { return _PortNumber; }

            set
            {
                _PortNumber = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string EventVideoDownloadPath
        {
            get { return _EventVideoDownloadPath; }

            set
            {
                _EventVideoDownloadPath = value;
                OnPropertyChanged();
            }
        }


        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string UserName
        {
            get { return _UserName; }

            set
            {
                _UserName = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string Password
        {
            get { return _Password; }

            set
            {
                _Password = value;
            }
        }

        //[DataMember]
        //[System.Xml.Serialization.XmlAttribute]
        //public int Index
        //{
        //    get
        //    {
        //        return _Index;
        //    }

        //    set
        //    {
        //        _Index = value;
        //    }
        //}


        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public List<CCTVConfig> CCTVConfig
        {
            get { return _CCTVConfig; }

            set
            {
                _CCTVConfig = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        //public void Copy(NVRServerConfig Config)
        //{
        //    //Config.CCTVConfig.(this.CCTVConfig.ToArray());
        //    CCTVConfig = Config.CCTVConfig.GetRange(0, Config.CCTVConfig.Count);
        //}



        public void Copy(NVRServerConfig copy_elem)
        {
            copy_elem.IPAddress = IPAddress;
            copy_elem.PortNumber = PortNumber;

            copy_elem.UserName = UserName;
            copy_elem.Password = Password;

            copy_elem.CCTVConfig = CCTVConfig.GetRange(0, CCTVConfig.Count);

        }
        public NVRServerConfig Copy()
        {
            NVRServerConfig copy_elem = new NVRServerConfig();
            Copy(copy_elem);
            return copy_elem;
        }

        #endregion

        #region constructors
        public NVRServerConfig()
        {
            _DeviceType = DeviceTypes.NotUsed;

            _IPAddress = string.Empty;
            _PortNumber = 0;

            _UserName = string.Empty;
            _Password = string.Empty;

            _Index = 0;

            _EventVideoDownloadPath = string.Empty;
            _CCTVConfig = new List<CCTVConfig>();
        }
        #endregion
    }
}
