using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    public class UnitBaseStatus : NotifyPropertyChanged
    {
        #region fields
        private string _UnitType = "Unknown";
        private DeviceTypes _DeviceType = DeviceTypes.HWS;
        private UnitOnline _UnitOnline = UnitOnline.Offline;
        private InitializationStates _InitializationState = InitializationStates.NotReady;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string UnitType
        {
            get { return _UnitType; }
            set
            {
                if (_UnitType != value)
                {
                    _UnitType = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public DeviceTypes DeviceType
        {
            get { return _DeviceType; }
            set
            {
                if (_DeviceType != value)
                {
                    _DeviceType = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public UnitOnline UnitOnline
        {
            get { return _UnitOnline; }
            set
            {
                if (_UnitOnline != value)
                {
                    _UnitOnline = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public InitializationStates InitializationState
        {
            get { return _InitializationState; }
            set
            {
                if (_InitializationState != value)
                {
                    _InitializationState = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors

        #endregion
    }
}
