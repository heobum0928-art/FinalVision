using Project.BaseLib.Enums;
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
    public class WTRConfig : NetworkConfig
    {
        #region fields
        protected int _RobotIndex;

        protected SubstrateSizes _WaferSize;

        protected RobotSpeeds _RobotSpeed;

        //protected ArmActionTypes _ArmActionType;

        //protected int _RobotStationNumber;
        #endregion

        #region propertise
        //[DataMember]
        //[System.Xml.Serialization.XmlElement]
        //public ArmActionTypes ArmActionType
        //{
        //    get
        //    {
        //        return _ArmActionType;
        //    }

        //    set
        //    {
        //        _ArmActionType = value;
        //        OnPropertyChanged();
        //    }
        //}


        //[DataMember]
        //[System.Xml.Serialization.XmlElement]
        //public int RobotStationNumber
        //{
        //    get
        //    {
        //        return _RobotStationNumber;
        //    }

        //    set
        //    {
        //        _RobotStationNumber = value;
        //        OnPropertyChanged();
        //    }
        //}

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public SubstrateSizes WaferSize
        {
            get
            {
                return _WaferSize;
            }

            set
            {
                _WaferSize = value;
                OnPropertyChanged();
            }
        }
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int RobotIndex
        {
            get
            {
                return _RobotIndex;
            }

            set
            {
                _RobotIndex = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public RobotSpeeds RobotSpeed
        {
            get
            {
                return _RobotSpeed;
            }

            set
            {
                _RobotSpeed = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Simulation Time

        protected int _PrepareTime;

        protected int _GetTime;

        protected int _PutTime;

        protected int _LPMoveTime;

        protected int _PBMoveTime;

        protected int _BCMoveTime;

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int PrepareTime
        {
            get
            {
                return _PrepareTime;
            }

            set
            {
                _PrepareTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int GetTime
        {
            get
            {
                return _GetTime;
            }

            set
            {
                _GetTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int PutTime
        {
            get
            {
                return _PutTime;
            }

            set
            {
                _PutTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int LPMoveTime // loadport <-> prealigner
        {
            get
            {
                return _LPMoveTime;
            }

            set
            {
                _LPMoveTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int PBMoveTime // prealigner <-> buffer
        {
            get
            {
                return _PBMoveTime;
            }

            set
            {
                _PBMoveTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int BCMoveTime // buffer <-> chamber
        {
            get
            {
                return _BCMoveTime;
            }

            set
            {
                _BCMoveTime = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public WTRConfig()
        {
            _RobotIndex = 0;

            _RobotSpeed = RobotSpeeds.Normal;

            _PrepareTime = 500;

            _GetTime = 1000;

            _PutTime = 1000;

            _LPMoveTime = 800;

            _PBMoveTime = 300;

            _BCMoveTime = 1000;

            //_RobotStationNumber = 0;

            //_ArmActionType = ArmActionTypes.Both;
        }
        #endregion

    }
}
