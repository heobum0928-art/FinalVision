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
    public class OrientData : NotifyPropertyChanged
    {
        #region fields
        protected double _Orientation;
        protected double _Score;
        #endregion

        #region propertise
        public double Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                _Orientation = value;
                OnPropertyChanged();
            }
        }
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
        #endregion

        #region methods

        #endregion

        #region constructors
        public OrientData(double _Orientation = 0.0, double _Score = 0.0)
        {
            this._Orientation = _Orientation;
            this._Score = _Score;
        }

        #endregion
    }
}
