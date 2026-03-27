using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute]
    public class MillimeterCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected double _X;
        protected double _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        public MillimeterCoordinates()
        {
            _X = 0.0;
            _Y = 0.0;
        }

        public MillimeterCoordinates(double x, double y)
        {
            _X = x;
            _Y = y;
        }

        public MicronCoordinates ToMicron()
        {
            var coordinate = new MicronCoordinates();
            coordinate.X = _X * 1000.0;
            coordinate.Y = _Y * 1000.0;

            return coordinate;
        }

        public NanoCoordinates ToNano()
        {
            NanoCoordinates coord = new NanoCoordinates();

            coord.X = (int)(_X * 1000000.0);
            coord.Y = (int)(_Y * 1000000.0);

            return coord;
        }

        public override bool Equals(object obj)
        {
            MillimeterCoordinates coord = (MillimeterCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1} mm]", _X, _Y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public MillimeterCoordinates TransferOffset(double X, double Y)
        {
            return new MillimeterCoordinates(_X + X, _Y + Y);
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class MicronCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected double _X;
        protected double _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        public MicronCoordinates()
        {
            _X = 0.0;
            _Y = 0.0;
        }

        public MillimeterCoordinates ToMillimeter()
        {
            MillimeterCoordinates coord = new MillimeterCoordinates();
            coord.X = _X / 1000.0;
            coord.Y = _Y / 1000.0;

            return coord;
        }

        public NanoCoordinates ToNano()
        {
            NanoCoordinates coord = new NanoCoordinates();
            coord.X = (int)(_X * 1000.0);
            coord.Y = (int)(_Y * 1000.0);

            return coord;
        }

        public override bool Equals(object obj)
        {
            MicronCoordinates coord = (MicronCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1} um]", _X, _Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class NanoCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected int _X;
        protected int _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        public NanoCoordinates()
        {
            _X = 0;
            _Y = 0;
        }

        public MillimeterCoordinates ToMillimeter()
        {
            MillimeterCoordinates coord = new MillimeterCoordinates();
            coord.X = _X / 1000000.0;
            coord.Y = _Y / 1000000.0;

            return coord;
        }
        public MicronCoordinates ToMicron()
        {
            MicronCoordinates coord = new MicronCoordinates();
            coord.X = _X / 1000.0;
            coord.Y = _Y / 1000.0;

            return coord;
        }
        public override bool Equals(object obj)
        {
            NanoCoordinates coord = (NanoCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1} nm]", _X, _Y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class PixelCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected int _X;
        protected int _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods

        public void Copy(PixelCoordinates copy_elem)
        {
            copy_elem._X = _X;
            copy_elem._Y = _Y;
        }

        public PixelCoordinates Copy()
        {
            PixelCoordinates copy_elem = new PixelCoordinates();
            Copy(copy_elem);
            return copy_elem;
        }
        public PixelCoordinates Duplicate()
        {
            return (PixelCoordinates)this.Copy();
        }
        protected virtual void AllocAll()
        {
            X = 0;
            Y = 0;
        }

        public static PixelCoordinates CreateNew()
        {
            PixelCoordinates temp = new PixelCoordinates();
            temp.AllocAll();
            return temp;
        }

        public void Move(int x, int y)
        {
            this.X += x;
            this.Y += y;
        }


        public virtual void Clear()
        {
            X = 0;
            Y = 0;
        }
        public override bool Equals(object obj)
        {
            PixelCoordinates coord = (PixelCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1}] Pixel", _X, _Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public DPointCoordinates ToDPointCoordinate()
        {
            DPointCoordinates dp = new DPointCoordinates(this.X, this.Y);

            return dp;
        }
        #endregion

        #region constructors
        public PixelCoordinates()
        {
        }

        public PixelCoordinates(Int32 X, Int32 Y)
        {
            this.X = X;
            this.Y = Y;
        }
        #endregion
    }


    [DataContract]
    [System.SerializableAttribute]
    public class DPixelCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected double _X;
        protected double _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods

        public void Copy(DPixelCoordinates copy_elem)
        {
            copy_elem._X = _X;
            copy_elem._Y = _Y;
        }

        public DPixelCoordinates Copy()
        {
            DPixelCoordinates copy_elem = new DPixelCoordinates();
            Copy(copy_elem);
            return copy_elem;
        }
        protected virtual void AllocAll()
        {
            X = 0.0;
            Y = 0.0;
        }

        public static DPixelCoordinates CreateNew()
        {
            DPixelCoordinates temp = new DPixelCoordinates();
            temp.AllocAll();
            return temp;
        }

        public DPixelCoordinates Duplicate()
        {
            return (DPixelCoordinates)this.Copy();
        }
        public virtual void Clear()
        {
            X = 0.0;
            Y = 0.0;
        }
        public override bool Equals(object obj)
        {
            DPixelCoordinates coord = (DPixelCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }

        public override string ToString()
        {
            return string.Format("[X={0}, Y={1} pixel]", _X, _Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region constructors
        public DPixelCoordinates()
        {
        }

        public DPixelCoordinates(Double X, Double Y)
        {
            this.X = X;
            this.Y = Y;
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class PointCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected int _X;
        protected int _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        public PointCoordinates TransferOffset(int X, int Y)
        {
            return new PointCoordinates(_X + X, _Y + Y);
        }
        public void Copy(PointCoordinates copy_elem)
        {
            copy_elem._X = _X;
            copy_elem._Y = _Y;
        }
        public PointCoordinates Copy()
        {
            PointCoordinates copy_elem = new PointCoordinates();
            Copy(copy_elem);
            return copy_elem;
        }
        protected virtual void AllocAll()
        {
            X = 0;
            Y = 0;
        }
        public static PointCoordinates CreateNew()
        {
            PointCoordinates temp = new PointCoordinates();
            temp.AllocAll();
            return temp;
        }
        public PointCoordinates Duplicate()
        {
            return (PointCoordinates)this.Copy();
        }
        public virtual void Clear()
        {
            X = 0;
            Y = 0;
        }
        public override bool Equals(object obj)
        {
            PointCoordinates coord = (PointCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }
        public override string ToString()
        {
            return string.Format("[X={0}, Y={1}]", _X, _Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region constructors
        public PointCoordinates()
        {
            _X = 0;
            _Y = 0;
        }

        public PointCoordinates(Int32 X, Int32 Y)
        {
            this.X = X;
            this.Y = Y;
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute]
    public class DPointCoordinates : NotifyPropertyChanged
    {
        #region Field
        protected double _X;
        protected double _Y;
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double X
        {
            get { return _X; }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public double Y
        {
            get { return _Y; }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region methods
        public DPointCoordinates TransferOffset(double X, double Y)
        {
            return new DPointCoordinates(_X + X, _Y + Y);
        }
        public void Copy(DPointCoordinates copy_elem)
        {
            copy_elem._X = _X;
            copy_elem._Y = _Y;
        }
        public DPointCoordinates Copy()
        {
            DPointCoordinates copy_elem = new DPointCoordinates();
            Copy(copy_elem);
            return copy_elem;
        }
        protected virtual void AllocAll()
        {
            X = 0;
            Y = 0;
        }
        public static DPointCoordinates CreateNew()
        {
            DPointCoordinates temp = new DPointCoordinates();
            temp.AllocAll();
            return temp;
        }
        public DPointCoordinates Duplicate()
        {
            return (DPointCoordinates)this.Copy();
        }
        public virtual void Clear()
        {
            X = 0;
            Y = 0;
        }
        public override bool Equals(object obj)
        {
            DPointCoordinates coord = (DPointCoordinates)obj;
            if (this.X == coord.X && this.Y == coord.Y)
                return true;

            return false;
        }
        public override string ToString()
        {
            return string.Format("[X={0}, Y={1}]", _X, _Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region constructors
        public DPointCoordinates()
        {
        }

        public DPointCoordinates(Double X, Double Y)
        {
            this.X = X;
            this.Y = Y;
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute()]
    public class ImageDimension : NotifyPropertyChanged
    {
        #region fields
        private Int32 _Width;
        private Int32 _Height;
        private Int32 _Pitch;
        #endregion

        #region constructors
        public ImageDimension()
        {

        }

        public ImageDimension(Int32 Width, Int32 Height, Int32 Pitch)
        {
            this.Width = Width;
            this.Height = Height;
            this.Pitch = Pitch;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Pitch
        {
            get
            {
                return _Pitch;
            }
            set
            {
                if (_Pitch != value)
                {
                    _Pitch = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            ImageDimension other = obj as ImageDimension;
            if (other == null)
                return false;
            if (_Width != null && other._Width != null)
            {
                if (!other._Width.Equals(_Width))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Width != other._Width)
                return false;
            if (_Height != null && other._Height != null)
            {
                if (!other._Height.Equals(_Height))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Height != other._Height)
                return false;
            if (_Pitch != null && other._Pitch != null)
            {
                if (!other._Pitch.Equals(_Pitch))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Pitch != other._Pitch)
                return false;

            return base.Equals(obj);
        }
        public ImageDimension Copy()
        {
            ImageDimension copy_elem = new ImageDimension();
            Copy(copy_elem);
            return copy_elem;
        }
        public void Copy(ImageDimension copy_elem)
        {
            copy_elem._Width = _Width;
            copy_elem._Height = _Height;
            copy_elem._Pitch = _Pitch;
        }

        public ImageDimension Duplicate()
        {
            return (ImageDimension)this.Copy();
        }

        public void Clear()
        {
            Width = 0;
            Height = 0;
            Pitch = 0;
        }

        protected void AllocAll()
        {
            Width = 0;
            Height = 0;
            Pitch = 0;
        }

        public static ImageDimension CreateNew()
        {
            ImageDimension temp = new ImageDimension();
            temp.AllocAll();
            return temp;
        }

        public override string ToString()
        {
            return string.Format("ImageDimension: {0} [ Width = {1}; Height = {2}; Pitch = {3};]"
                , base.ToString()
                , _Width
                , _Height
                , _Pitch
                );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute()]
    public class Dimension : NotifyPropertyChanged
    {
        #region fields
        private Int32 _Width;
        private Int32 _Height;
        #endregion

        #region constructors
        public Dimension()
        {
            _Width = 0;
            _Height = 0;
        }

        public Dimension(Int32 Width, Int32 Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Width
        {
            get
            {
                return _Width;
            }
            set
            {
                if (_Width != value)
                {
                    _Width = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Height
        {
            get
            {
                return _Height;
            }
            set
            {
                if (_Height != value)
                {
                    _Height = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            Dimension other = obj as Dimension;
            if (other == null)
                return false;
            if (_Width != null && other._Width != null)
            {
                if (!other._Width.Equals(_Width))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Width != other._Width)
                return false;
            if (_Height != null && other._Height != null)
            {
                if (!other._Height.Equals(_Height))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Height != other._Height)
                return false;



            return base.Equals(obj);
        }
        public Dimension Copy()
        {
            Dimension copy_elem = new Dimension();
            Copy(copy_elem);
            return copy_elem;
        }
        public void Copy(Dimension copy_elem)
        {
            copy_elem._Width = _Width;
            copy_elem._Height = _Height;
        }

        public Dimension Duplicate()
        {
            return (Dimension)this.Copy();
        }

        public void Clear()
        {
            Width = 0;
            Height = 0;
        }

        protected void AllocAll()
        {
            Width = 0;
            Height = 0;
        }

        public static Dimension CreateNew()
        {
            Dimension temp = new Dimension();
            temp.AllocAll();
            return temp;
        }

        public override string ToString()
        {
            return string.Format("ImageDimension: [ Width = {0}; Height = {1}]"
                , _Width
                , _Height
                );
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute()]
    public class Dimension2D : NotifyPropertyChanged
    {
        #region fields
        private Int32 _X;
        private Int32 _Y;
        #endregion

        #region constructors
        public Dimension2D()
        {
            _X = 0;
            _Y = 0;
        }

        public Dimension2D(Int32 X, Int32 Y)
        {
            this.X = X;
            this.Y = Y;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 X
        {
            get
            {
                return _X;
            }
            set
            {
                if (_X != value)
                {
                    _X = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 Y
        {
            get
            {
                return _Y;
            }
            set
            {
                if (_Y != value)
                {
                    _Y = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            Dimension2D other = obj as Dimension2D;
            if (other == null)
                return false;
            if (_X != null && other._X != null)
            {
                if (!other._X.Equals(_X))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_X != other._X)
                return false;
            if (_Y != null && other._Y != null)
            {
                if (!other._Y.Equals(_Y))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Y != other._Y)
                return false;



            return base.Equals(obj);
        }
        public Dimension2D Copy()
        {
            Dimension2D copy_elem = new Dimension2D();
            Copy(copy_elem);
            return copy_elem;
        }
        public void Copy(Dimension2D copy_elem)
        {
            copy_elem._X = _X;
            copy_elem._Y = _Y;
        }

        public Dimension2D Duplicate()
        {
            return (Dimension2D)this.Copy();
        }

        public void Clear()
        {
            X = 0;
            Y = 0;
        }

        protected void AllocAll()
        {
            X = 0;
            Y = 0;
        }

        public static Dimension2D CreateNew()
        {
            Dimension2D temp = new Dimension2D();
            temp.AllocAll();
            return temp;
        }

        public override string ToString()
        {
            return string.Format("Dimension2D: [ X = {0}; Y = {1}]", _X, _Y);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute()]
    public class RoiRectangle : NotifyPropertyChanged
    {
        #region Fields
        private const double EPSILON = 0.0001;

        private PointCoordinates _TopLeft;
        private Dimension _Size;

        private Color _Color;

        private int _RoiIndex;

        private ROITypes _ROIType;
        #endregion

        #region Constructors
        public RoiRectangle()
        {
            Color = Colors.Black;
            _ROIType = ROITypes.Unknown;
            _RoiIndex = 0;
        }
        public RoiRectangle(PointCoordinates TopLeft, Dimension Size)
        {
            this.TopLeft = TopLeft;
            this.Size = Size;
        }
        public RoiRectangle(PointCoordinates TopLeft, Dimension Size, Color color)
            : this(TopLeft.Y, TopLeft.X, TopLeft.Y + Size.Height, TopLeft.X + Size.Width, color)
        {

        }
        public RoiRectangle(PointCoordinates TopLeft, Dimension Size, Color color, ROITypes type)
            : this(TopLeft.Y, TopLeft.X, TopLeft.Y + Size.Height, TopLeft.X + Size.Width, color, type)
        {

        }
        public RoiRectangle(int top, int left, int bottom, int right)
            : this(new PointCoordinates(left, top), new Dimension(right - left, bottom - top))
        {

        }
        public RoiRectangle(int top, int left, int bottom, int right, Color color)
            : this(top, left, bottom, right)
        {
            this._Color = color;
        }
        public RoiRectangle(int top, int left, int bottom, int right, Color color, ROITypes type)
            : this(top, left, bottom, right, color)
        {
            this._ROIType = type;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int RoiIndex
        {
            get { return _RoiIndex; }

            set
            {
                if (_RoiIndex != value)
                {
                    _RoiIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public ROITypes ROIType
        {
            get { return _ROIType; }

            set
            {
                if(_ROIType != value)
                {
                    _ROIType = value;
                    OnPropertyChanged();
                }
            }
        }
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    OnPropertyChanged();
                }
            }
        }
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
        public int Top { get { return this.TopLeft.Y; } }
        [XmlIgnore]
        public int Left { get { return TopLeft.X; } }
        [XmlIgnore]
        public int Bottom { get { return TopLeft.Y + this.Size.Height; } }
        [XmlIgnore]
        public int Right { get { return TopLeft.X + Size.Width; } }
        [XmlIgnore]
        public double Area { get { return Size.Width * Size.Height; } }
        [XmlIgnore]
        public int Width { get { return Size.Width; } }
        [XmlIgnore]
        public int Height { get { return Size.Height; } }
        #endregion

        #region Overridden Methods
        public RoiRectangle TransferOffset(int left, int top)
        {
            var topleft = _TopLeft.TransferOffset(left, top);

            return new RoiRectangle(topleft, Size, _Color, _ROIType);
        }
        public override bool Equals(object obj)
        {
            RoiRectangle other = obj as RoiRectangle;
            if (other == null)
                return false;
            if (_TopLeft != null && other._TopLeft != null)
            {
                if (!other._TopLeft.Equals(_TopLeft))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_TopLeft != other._TopLeft)
                return false;
            if (_Size != null && other._Size != null)
            {
                if (!other._Size.Equals(_Size))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_Size != other._Size)
                return false;

            return base.Equals(obj);
        }
        public bool Equals(int left, int top, int width, int height)
        {
            int gap = 3;
            int right = left + width;
            int bottom = top + height;

            if (Left - gap < left && Left + gap > left &&
                Top - gap < top && Top + gap > top &&
                Right - gap < right && Right + gap > right &&
                Bottom - gap < bottom && Bottom + gap > bottom
                )
            {
                return true;
            }

            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public void Copy(RoiRectangle copy_elem)
        {
            if (_TopLeft != null)
            {
                copy_elem._TopLeft = _TopLeft.Duplicate();

            }
            if (_Size != null)
            {
                copy_elem._Size = _Size.Duplicate();
            }

            copy_elem._Color = _Color;
            copy_elem._ROIType = _ROIType;
            copy_elem._RoiIndex = _RoiIndex;
        }
        public RoiRectangle Copy()
        {
            RoiRectangle copy_elem = new RoiRectangle();
            Copy(copy_elem);
            return copy_elem;
        }
        public RoiRectangle Duplicate()
        {
            return (RoiRectangle)this.Copy();
        }
        public virtual void Clear()
        {
            if (TopLeft != null)
                TopLeft.Clear();
            if (Size != null)
                Size.Clear();
        }
        protected void AllocAll()
        {
            TopLeft = PointCoordinates.CreateNew();
            Size = Dimension.CreateNew();
        }
        public static RoiRectangle CreateNew()
        {
            RoiRectangle temp = new RoiRectangle();
            temp.AllocAll();
            return temp;
        }
        public override string ToString()
        {
            return string.Format("Rectangle: {0} [ TopLeft = {1}; Size = {2};]"
                , base.ToString()
                , _TopLeft
                , _Size
                );
        }
        public bool IsIntersect(RoiRectangle r)
        {
            return (IsIntersect(r.Top, r.Left, r.Bottom, r.Right));
        }
        private bool IsIntersect(int top, int left, int bottom, int right)
        {
            return (left < this.Right) &&
                   (this.Left < right) &&
                   (top < this.Bottom) &&
                   (this.Top < bottom);
        }

        ///    Creates a rectangle that represents the intersection between a and b.
        ///    If there is no intersection, null is returned. 
        public static RoiRectangle Intersect(RoiRectangle a, RoiRectangle b)
        {
            int x1 = Math.Max(a.Left, b.Left);
            int x2 = Math.Min(a.Right, b.Right);
            int y1 = Math.Max(a.Top, b.Top);
            int y2 = Math.Min(a.Bottom, b.Bottom);

            if (x2 > x1 && y2 > y1)
            {
                return new RoiRectangle(y1, x1, y2, x2);
            }

            return null;
        }
        public void Inflate(int size)
        {
            this.TopLeft.X -= size;
            this.TopLeft.Y -= size;

            this.Size.Width += 2 * size;
            this.Size.Height += 2 * size;
        }
        public bool Deflate(int size)
        {
            if ((Size.Width - (2 * size)) < 0 || (Size.Height - (2 * size)) < 0)
                return false;

            Inflate(-1 * size);
            return true;
        }
        public void Resize(float factor)
        {
            this.TopLeft.X = (int)((this.TopLeft.X * factor) + 0.5f);
            this.TopLeft.Y = (int)((this.TopLeft.Y * factor) + 0.5f);

            this.Size.Width = (int)((this.Size.Width * factor) + 0.5f);
            this.Size.Height = (int)((this.Size.Height * factor) + 0.5f);
        }
        public void Resize(float factorX, float factorY)
        {
            this.TopLeft.X = (int)((this.TopLeft.X * factorX) + 0.5f);
            this.TopLeft.Y = (int)((this.TopLeft.Y * factorY) + 0.5f);

            this.Size.Width = (int)((this.Size.Width * factorX) + 0.5f);
            this.Size.Height = (int)((this.Size.Height * factorY) + 0.5f);
        }
        public bool Contains(PointCoordinates point)
        {
            return (IsPointInLine(Left, Right, point.X) &&
                    IsPointInLine(Top, Bottom, point.Y));
        }
        private bool IsPointInLine(double lineStart, double lineEnd, double point)
        {
            return ((lineStart <= point - EPSILON) && (point <= lineEnd - EPSILON));
        }
        public PointCoordinates GetCenter()
        {
            return new PointCoordinates(TopLeft.X + (Size.Width / 2), TopLeft.Y + (Size.Height / 2));

        }
        #endregion
    }
    // ========================================================================================


}
