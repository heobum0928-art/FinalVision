using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class CalibrationData : NotifyPropertyChanged
    {
        #region fields
        protected double _DistanceX;

        protected double _DistanceY;

        protected double _Angle;

        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double DistanceX
        {
            get
            {
                return _DistanceX;
            }
            set
            {
                _DistanceX = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double DistanceY
        {
            get
            {
                return _DistanceY;
            }
            set
            {
                _DistanceY = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Angle
        {
            get
            {
                return _Angle;
            }
            set
            {
                _Angle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public CalibrationData()
        {
            _DistanceX = 0.0;

            _DistanceY = 0.0;

            _Angle = 0.0;
        }
        #endregion

    }
}
