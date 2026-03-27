using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


using Project.BaseLib.Utils;

namespace Project.HWC
{
    [DataContract]
    [System.SerializableAttribute]
    public class BufferConfig : NotifyPropertyChanged
    {
        #region fields
        protected int _RobotStationNumber;

        protected int _SlotCount;

        protected int _InputSlotCount;
        protected int _OutputSlotCount;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int InputSlotCount
        {
            get
            {
                return _InputSlotCount;
            }

            set
            {
                _InputSlotCount = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int OutputSlotCount
        {
            get
            {
                return _OutputSlotCount;
            }

            set
            {
                _OutputSlotCount = value;
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


        [System.Xml.Serialization.XmlIgnore]
        public int BufferSlotCount
        {
            get
            {
                return _InputSlotCount + _OutputSlotCount;
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public BufferConfig()
        {
            _RobotStationNumber = 0;
        }
        #endregion

    }
}
