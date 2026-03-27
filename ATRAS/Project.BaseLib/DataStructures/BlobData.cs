using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class BlobData : NotifyPropertyChanged
    {
        #region fields
        protected PointCoordinates _TopLeft;
        protected Dimension _Size;
        protected int _Area;

        protected PointCoordinates _CenterOfGravity;

        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public PointCoordinates TopLeft
        {
            get
            {
                return _TopLeft;
            }
            set
            {
                if (_TopLeft != value)
                {
                    _TopLeft = value;
                    OnPropertyChanged();
                }
            }
        }
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public Dimension Size
        {
            get
            {
                return _Size;
            }
            set
            {
                if (_Size != value)
                {
                    _Size = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlIgnore]
        public int Left
        {
            get { return this._TopLeft.X; }

            set { this._TopLeft.X = value; }
        }
        [XmlIgnore]
        public int Top
        {
            get { return this._TopLeft.Y; }

            set { this._TopLeft.Y = value; }
        }
        [XmlIgnore]
        public int Right
        {
            get { return this._TopLeft.X + this._Size.Width; }
        }
        [XmlIgnore]
        public int Bottom
        {
            get { return this._TopLeft.Y + this._Size.Height; }
        }
        [XmlIgnore]
        public int Area 
        {
            get { return this._Area; }
            set { this._Area = value; }
        }
        [XmlIgnore]
        public int Width 
        { 
            get { return this._Size.Width; }
            set { this._Size.Width = value; }
        }
        [XmlIgnore]
        public int Height
        {
            get { return this._Size.Height; }

            set { this._Size.Height = value; }
        }


        [XmlIgnore]
        public int CogX
        {
            get
            {
                return _CenterOfGravity.X;
            }

            set
            {
                _CenterOfGravity.X = value;
            }
        }

        [XmlIgnore]
        public int CogY
        {
            get
            {
                return _CenterOfGravity.Y;
            }

            set
            {
                _CenterOfGravity.Y = value;
            }
        }

        #endregion

        #region methods
        public void Copy(BlobData copy_elem)
        {
            if (_TopLeft != null)
            {
                copy_elem._TopLeft = _TopLeft.Duplicate();

            }
            if (_Size != null)
            {
                copy_elem._Size = _Size.Duplicate();
            }

            copy_elem._Area = _Area;

        }
        public BlobData Duplicate()
        {
            BlobData copy_elem = new BlobData();
            Copy(copy_elem);
            return copy_elem;
        }
        public static BlobData CreateNew()
        {
            return new BlobData();
        }
        public override string ToString()
        {
            return string.Format("BlobData: [ TopLeft = {0}; Size = {1}; Area = {2}]"
                , _TopLeft
                , _Size
                , _Area
                ) ;
        }
        #endregion

        #region constructors
        public BlobData()
        {
            _TopLeft = new PointCoordinates();
            _Size = new Dimension();
            _Area = 0;

            _CenterOfGravity = new PointCoordinates();
        }
        public BlobData(PointCoordinates TopLeft, Dimension Size, int Area)
        {
            this._TopLeft = TopLeft;
            this._Size = Size;
            this._Area = Area;
        }
        public BlobData(int left, int top, int right, int bottom, int Area)
            : this(new PointCoordinates(left, top), new Dimension(right - left, bottom - top), Area)
        {

        }
        #endregion
    }
}
