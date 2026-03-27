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
    public class LoadPortConfig : NetworkConfig
    {
        #region fields
        protected int _RobotStationNumber;
        protected int _LoadPortIndex;

        protected int _MaxSlotCount;

        protected string _DefaultName;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public string DefaultName
        {
            get
            {
                return _DefaultName;
            }

            set
            {
                _DefaultName = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int LoadPortIndex
        {
            get
            {
                return _LoadPortIndex;
            }

            set
            {
                _LoadPortIndex = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int RobotStationNumber
        {
            get
            {
                return _RobotStationNumber;
            }

            set
            {
                _RobotStationNumber = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int MaxSlotCount
        {
            get
            {
                return _MaxSlotCount;
            }

            set
            {
                _MaxSlotCount = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Simulation Time
        protected int _MappingTime;

        protected int _DockTime;

        protected int _ClampTime;

        protected int _DoorOpenCloseTime;

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int MappingTime
        {
            get
            {
                return _MappingTime;
            }

            set
            {
                _MappingTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int DockTime
        {
            get
            {
                return _DockTime;
            }

            set
            {
                _DockTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int ClampTime
        {
            get
            {
                return _ClampTime;
            }

            set
            {
                _ClampTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int DoorOpenCloseTime
        {
            get
            {
                return _DoorOpenCloseTime;
            }

            set
            {
                _DoorOpenCloseTime = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public LoadPortConfig()
        {
            _RobotStationNumber = 0;

            _LoadPortIndex = 0;

            _MaxSlotCount = 25;

            _DefaultName = "Unknown";


            _MappingTime = 1000;

            _DockTime = 500;

            _ClampTime = 200;

            _DoorOpenCloseTime = 1000;

        }
        #endregion

    }
}
