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
    public class ModelFindData : NotifyPropertyChanged
    {
        #region fields
        protected double _XPos;
        protected double _YPos;

        protected double _XWorldPos;
        protected double _YWorldPos;

        protected double _Score;
        protected double _Angle;
        protected double _Scale;

        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double XPos
        {
            get
            {
                return _XPos;
            }
            set
            {
                _XPos = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double YPos
        {
            get
            {
                return _YPos;
            }
            set
            {
                _YPos = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double XWorldPos
        {
            get
            {
                return _XWorldPos;
            }
            set
            {
                _XWorldPos = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double YWorldPos
        {
            get
            {
                return _YWorldPos;
            }
            set
            {
                _YWorldPos = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Score
        {
            get
            {
                return _Score;
            }
            set
            {
                _Score = value;
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

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public double Scale
        {
            get
            {
                return _Scale;
            }
            set
            {
                _Scale = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region methods
        public ModelFindData Duplicate()
        {
            ModelFindData data = new ModelFindData();

            data.XPos = this.XPos;
            data.YPos = this.YPos;
            data.XWorldPos = this.XWorldPos;
            data.YWorldPos = this.YWorldPos;

            data.Score = this.Score;
            data.Angle = this.Angle;
            data.Scale = this.Scale;

            return data;
        }


        public override string ToString()
        {
            return string.Format($"XPos: {XPos}, YPos: {YPos}, XWorldPos: {XWorldPos}, YWorldPos: {YWorldPos}, " +
                                $"Score: {Score}, Angle: {Angle}, Scale: {Scale}.");
        }
        #endregion

        #region constructors
        public ModelFindData()
        {
            _XPos = 0.0;
            _YPos = 0.0;

            _XWorldPos = 0.0;
            _YWorldPos = 0.0;

            _Score = 0.0;
            _Angle = 0.0;
            _Scale = 0.0;
        }
        #endregion
    }
}
