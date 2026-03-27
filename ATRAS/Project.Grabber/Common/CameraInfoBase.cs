using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.Grabber
{
    [DataContract]
    [System.SerializableAttribute]
    public abstract class CameraInfoBase : NotifyPropertyChanged
    {
        #region field
        protected string _InfoFilePathName;

        protected string _VirtualGrabPath;

        protected int _CamNumber;

        protected int _Width; // Pixel

        protected int _Height; // Pixel

        protected int _OffsetX; // Pixel

        protected int _OffsetY; // Pixel

        protected bool _ReverseX;

        protected bool _ReverseY;

        //protected TriggerSources _TriggerSource;
        protected ExposureModes _ExposureMode; // Timed, TriggerWidth

        protected TriggerModes _TriggerMode; // External, Internal

        protected int _ExposureTime; // ns

        protected int _FrameBufferCount; // 

        protected int _PixelResolution; // nm
        #endregion

        #region properties
        [System.Xml.Serialization.XmlElement]
        public int PixelResolution
        {
            get { return _PixelResolution; }
            set { _PixelResolution = value; }
        }


        [System.Xml.Serialization.XmlElement]
        public int FrameBufferCount
        {
            get { return _FrameBufferCount; }
            set { _FrameBufferCount = value; }
        }

        [System.Xml.Serialization.XmlAttribute]
        public string InfoFilePathName
        {
            get { return _InfoFilePathName; }
            set { _InfoFilePathName = value; }
        }

        [System.Xml.Serialization.XmlAttribute]
        public string VirtualGrabPath
        {
            get { return _VirtualGrabPath; }
            set { _VirtualGrabPath = value; }
        }



        [System.Xml.Serialization.XmlAttribute]
        public int CamNumber
        {
            get { return _CamNumber; }
            set { _CamNumber = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public ExposureModes ExposureMode
        {
            get { return _ExposureMode; }
            set { _ExposureMode = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public TriggerModes TriggerMode
        {
            get { return _TriggerMode; }
            set { _TriggerMode = value; }
        }
        [System.Xml.Serialization.XmlElement]
        public int ExposureTime
        {
            get { return _ExposureTime; }
            set { _ExposureTime = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public int Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public int Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public int OffsetX
        {
            get { return _OffsetX; }
            set { _OffsetX = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public int OffsetY
        {
            get { return _OffsetY; }
            set { _OffsetY = value; }
        }


        [System.Xml.Serialization.XmlElement]
        public bool ReverseX
        {
            get { return _ReverseX; }
            set { _ReverseX = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public bool ReverseY
        {
            get { return _ReverseY; }
            set { _ReverseY = value; }
        }

        #endregion

        #region Methods
        public abstract CameraInfoBase Copy();
        //{
        //    CameraInfoBase copy_elem = new CameraInfoBase();
        //    Copy(copy_elem);
        //    return copy_elem;
        //}

        public virtual void Copy(CameraInfoBase copy_elem)
        {
            copy_elem._InfoFilePathName = _InfoFilePathName;
            copy_elem._VirtualGrabPath = _VirtualGrabPath;

            copy_elem._FrameBufferCount = _FrameBufferCount;
            //copy_elem._SensorWidth = _SensorWidth;
            //copy_elem._SensorHeight = _SensorHeight;

            copy_elem._Width = _Width;
            copy_elem._Height = _Height;
            copy_elem._OffsetX = _OffsetX;
            copy_elem._OffsetY = _OffsetY;
            copy_elem._TriggerMode = _TriggerMode;
            copy_elem._ExposureMode = _ExposureMode;
            copy_elem._ExposureTime = _ExposureTime;
            copy_elem._ReverseX = _ReverseX;
            copy_elem._ReverseY = _ReverseY;
            copy_elem._CamNumber = _CamNumber;

            copy_elem._PixelResolution = _PixelResolution;
        }

        #endregion

        #region constructor
        public CameraInfoBase()
        {
            _Width = 0;

            _Height = 0;

            _OffsetX = 0;

            _OffsetY = 0;

            _ReverseX = false;

            _ReverseY = false;

            _TriggerMode = TriggerModes.Internal;

            //_TriggerSource = TriggerSources.TIMER;
            _ExposureMode = ExposureModes.Timed;

            _ExposureTime = 0;

            _CamNumber = 0;

            _VirtualGrabPath = "D:\\VirtualGrab";

            _InfoFilePathName = string.Empty;

            _PixelResolution = 0;
        }
        #endregion

    }
}
