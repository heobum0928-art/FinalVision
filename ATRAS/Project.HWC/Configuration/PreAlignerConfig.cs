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
    public class PreAlignerConfig : NetworkConfig
    {
        #region fields
        protected int _RobotStationNumber;

        protected double _X_Limit_mm;
        protected double _Y_Limit_mm;
        protected double _Angle_Limit_degree;

        #endregion

        #region propertise

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
        public double X_Limit_mm
        {
            get
            {
                return _X_Limit_mm;
            }

            set
            {
                _X_Limit_mm = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Y_Limit_mm
        {
            get
            {
                return _Y_Limit_mm;
            }

            set
            {
                _Y_Limit_mm = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Angle_Limit_degree
        {
            get
            {
                return _Angle_Limit_degree;
            }

            set
            {
                _Angle_Limit_degree = value;
                OnPropertyChanged();
            }
        }
                       
        #endregion

        #region Simulation Time
        protected int _AlignTime;

        protected int _InitAlignTime;

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int AlignTime
        {
            get
            {
                return _AlignTime;
            }

            set
            {
                _AlignTime = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int InitAlignTime
        {
            get
            {
                return _InitAlignTime;
            }

            set
            {
                _InitAlignTime = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public PreAlignerConfig()
        {
            _RobotStationNumber = 0;
            _X_Limit_mm = 0;
            _Y_Limit_mm = 0;
            _Angle_Limit_degree = 0;

            _AlignTime = 800;

            _InitAlignTime = 1000;
        }
        #endregion
    }
}
