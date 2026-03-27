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
    public class CircleData : NotifyPropertyChanged
    {
        #region fields
        protected double _X;
        protected double _Y;
        protected double _R;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double R
        {
            get
            {
                return _R;
            }
            set
            {
                _R = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format($"X={_X}, Y={_Y}, R={_R}");
        }
        #endregion

        #region constructors
        public CircleData(double _X = 0.0, double _Y = 0.0, double _R = 0.0)
        {
            this._X = _X;
            this._Y = _Y;
            this._R = _R;
        }
        #endregion


    }
}
