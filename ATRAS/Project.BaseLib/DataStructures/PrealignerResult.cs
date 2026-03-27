using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Project.BaseLib.Utils;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public partial class PrealignerResult : NotifyPropertyChanged
    {
        #region Fields
        private Double _Result1;
        private Double _Result2;
        private Double _EccentricityMagnitude;
        #endregion

        #region Constructors
        public PrealignerResult()
        {
            _Result1 = 0.0;
            _Result2 = 0.0;
            _EccentricityMagnitude = 0.0;
        }

        public PrealignerResult(Double Result1, Double Result2, Double EccentricityMagnitude)
        {
            this.Result1 = Result1;
            this.Result2 = Result2;
            this.EccentricityMagnitude = EccentricityMagnitude;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Double Result1
        {
            get
            {
                return _Result1;
            }
            set
            {
                if (_Result1 != value)
                {
                    _Result1 = value;
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Double Result2
        {
            get
            {
                return _Result2;
            }
            set
            {
                if (_Result2 != value)
                {
                    _Result2 = value;
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Double EccentricityMagnitude
        {
            get
            {
                return _EccentricityMagnitude;
            }
            set
            {
                if (_EccentricityMagnitude != value)
                {
                    _EccentricityMagnitude = value;
                }
            }
        }

        #endregion

        #region Overridden Methods
        public override bool Equals(object obj)
        {
            PrealignerResult other = obj as PrealignerResult;
            if (other == null)
                return false;
            if (_Result1 != null && other._Result1 != null)
            {
                if (!other._Result1.Equals(_Result1))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Result1 != other._Result1)
                return false;
            if (_Result2 != null && other._Result2 != null)
            {
                if (!other._Result2.Equals(_Result2))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Result2 != other._Result2)
                return false;
            if (_EccentricityMagnitude != null && other._EccentricityMagnitude != null)
            {
                if (!other._EccentricityMagnitude.Equals(_EccentricityMagnitude))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_EccentricityMagnitude != other._EccentricityMagnitude)
                return false;

            return base.Equals(obj);
        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Copy(PrealignerResult copy_elem)
        {
            copy_elem._Result1 = _Result1;
            copy_elem._Result2 = _Result2;
            copy_elem._EccentricityMagnitude = _EccentricityMagnitude;
        }

        public PrealignerResult Copy()
        {
            PrealignerResult copy_elem = new PrealignerResult();
            Copy(copy_elem);
            return copy_elem;
        }

        public PrealignerResult Duplicate()
        {
            return (PrealignerResult)this.Copy();
        }

        public void Clear()
        {
            Result1 = 0;
            Result2 = 0;
            EccentricityMagnitude = 0;
        }

        protected void AllocAll()
        {
            Result1 = 0;
            Result2 = 0;
            EccentricityMagnitude = 0;
        }

        public static PrealignerResult CreateNew()
        {
            PrealignerResult temp = new PrealignerResult();
            temp.AllocAll();
            return temp;
        }
        public override string ToString()
        {
            return string.Format("PrealignerResult: {0} [ Result1 = {1}; Result2 = {2}; EccentricityMagnitude = {3};]"
                , base.ToString()
                , _Result1
                , _Result2
                , _EccentricityMagnitude
                );
        }

        #endregion
    }
}
