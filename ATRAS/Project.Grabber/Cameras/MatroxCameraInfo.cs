using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class MatroxCameraInfo : CameraInfoBase
    {
        #region fields
        protected int _Camera_ID;

        protected int _Board_ID;

        protected string _ComPortName;

        protected int _LineRate;

        protected string _CameraName;

        protected int _GrabIntervalTime;
        #endregion

        #region propertise

        [System.Xml.Serialization.XmlElement]
        public int LineRate
        {
            get { return _LineRate; }

            set { _LineRate = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public string CameraName
        {
            get { return _CameraName; }

            set { _CameraName = value; }
        }

        [System.Xml.Serialization.XmlElement]
        public int Camera_ID
        {
            get { return _Camera_ID; }

            set { _Camera_ID = value; }
        }
        [System.Xml.Serialization.XmlElement]
        public int Board_ID
        {
            get { return _Board_ID; }

            set { _Board_ID = value; }
        }
        [System.Xml.Serialization.XmlAttribute]
        public string ComPortName
        {
            get { return _ComPortName; }

            set { _ComPortName = value; }
        }

        [System.Xml.Serialization.XmlAttribute]
        public int GrabIntervalTime
        {
            get { return _GrabIntervalTime; }

            set { _GrabIntervalTime = value; }
        }
        #endregion

        #region methods
        public override void Copy(CameraInfoBase copy_elem)
        {
            base.Copy(copy_elem);

            MatroxCameraInfo info = copy_elem as MatroxCameraInfo;

            info._Board_ID = _Board_ID;
            info._Camera_ID = _Camera_ID;
            info._ComPortName= _ComPortName;
            info._LineRate = _LineRate;
            info._CameraName = _CameraName;
            info._GrabIntervalTime = _GrabIntervalTime;
        }

        public override CameraInfoBase Copy()
        {
            MatroxCameraInfo copy_elem = new MatroxCameraInfo();
            Copy(copy_elem);
            return copy_elem;
        }
        #endregion

        #region constructors
        public MatroxCameraInfo()
        {
            _Camera_ID = 0;
            _Board_ID = 0;
            _ComPortName = string.Empty;

            _LineRate = 0;

            _CameraName = string.Empty;

            _GrabIntervalTime = 0;
        }
        #endregion
    }
}
