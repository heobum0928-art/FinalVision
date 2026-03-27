using Project.BaseLib.DataStructures;
using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class RoiData : NotifyPropertyChanged
    {
        #region fields
        protected int index;

        protected int cameraNumber;

        protected int topOrbottmSide; // 0 : top, 1 : bottom

        protected int x;
        protected int y;
        protected int width;
        protected int height;

        protected RoiRectangle _ROI_RECT;

        protected ROITypes _ROIType;

        protected Color _Color = Colors.White;

        #endregion

        #region propertiese
        [System.Xml.Serialization.XmlElement]
        public int CameraNumber
        {
            get { return cameraNumber; }
            set
            {
                cameraNumber = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int TopOrBottomSide
        {
            get { return topOrbottmSide; }
            set
            {
                topOrbottmSide = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public Color Color
        {
            get { return _Color; }
            set 
            { 
                _Color = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public RoiRectangle ROI_RECT
        {
            get { return _ROI_RECT; }

            set
            {
                if(!_ROI_RECT.Equals(value))
                {
                    _ROI_RECT = value;
                    OnPropertyChanged();
                }
            }
        }

        [System.Xml.Serialization.XmlElement]
        public ROITypes ROIType
        {
            get { return _ROIType; }

            set { _ROIType = value; }
        }

        [System.Xml.Serialization.XmlAttribute]
        public int Index
        {
            get { return index; }

            set 
            { 
                index = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int X
        {
            get { return x; }

            set 
            { 
                x = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int Y
        {
            get { return y; }

            set 
            {
                y = value;
                OnPropertyChanged();
            }
        }
        [System.Xml.Serialization.XmlElement]
        public int Width
        {
            get { return width; }

            set 
            { 
                width = value;
                OnPropertyChanged();
            }
        }
        [System.Xml.Serialization.XmlElement]
        public int Height
        {
            get { return height; }

            set 
            { 
                height = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("ROI Data : Index=[{0}], CameraNumber = [{1}], TopOrBottomSide = [{2}], X=[{3}], Y=[{4}], Width=[{5}], " +
                "Height=[{6}], ROI Type=[{7}]",
                index, cameraNumber, topOrbottmSide, x, y, width, height, _ROIType);
        }

        public virtual void Copy(RoiData copy_elem)
        {
            copy_elem.index = this.Index;

            copy_elem.CameraNumber = this.CameraNumber;
            copy_elem.TopOrBottomSide = this.TopOrBottomSide;

            copy_elem.X = this.X;
            copy_elem.Y = this.Y;
            copy_elem.width = this.Width;
            copy_elem.Height = this.Height;

            copy_elem.ROIType = this.ROIType;

            copy_elem.ROI_RECT = this.ROI_RECT.Duplicate();
        }

        public RoiData Copy()
        {
            RoiData copy_elem = new RoiData();
            Copy(copy_elem);
            return copy_elem;
        }
        public RoiData Duplicate()
        {
            return (RoiData)this.Copy();
        }
        #endregion

        #region constructor
        public RoiData()
        {
            index = 0;
            cameraNumber = 0;

            topOrbottmSide = 0; // 0 : top, 1 : bottm

            x = 0;
            y = 0;
            width = 0;
            height = 0;

            _ROI_RECT = new RoiRectangle();

            _ROIType = ROITypes.Unknown;
        }
        #endregion
    }
}
