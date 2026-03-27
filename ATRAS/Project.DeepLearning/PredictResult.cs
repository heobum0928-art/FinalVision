using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DeepLearning.UI
{
    public class PredictResult : NotifyPropertyChanged
    {
        #region fields
        protected string _ClassName;

        protected double _Confidence;

        protected int _ClassIndex;
        #endregion

        #region propertise
        public string ClassName
        {
            get
            {
                return _ClassName;
            }

            set
            {
                _ClassName = value;
                OnPropertyChanged();
            }
        }

        public double Confidence
        {
            get
            {
                return _Confidence;
            }

            set
            {
                _Confidence = value;
                OnPropertyChanged();
            }
        }

        public int ClassIndex
        {
            get
            {
                return _ClassIndex;
            }

            set
            {
                _ClassIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format($"ClassIndex : {_ClassIndex}, ClassName : {_ClassName}, Confidence : {_Confidence}");
        }
        #endregion

        #region constructors
        public PredictResult(string ClassName, int ClassIndex, double Confidence)
        {
            this.ClassName = ClassName;
            this.ClassIndex = ClassIndex;
            this.Confidence = Confidence;

        }
        #endregion
    }
}
