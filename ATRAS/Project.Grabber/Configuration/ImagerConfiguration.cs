using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    [DataContract]
    [System.SerializableAttribute]
    public class ImagerConfiguration<T> : NotifyPropertyChanged
        where T : new()
        //where T : ImagerConfigBase<T>
    {
        #region field
        private List<T> _CameraInfos;

        private GrabberTypes _GrabberType;

        protected int _GrabBufferCount;

        protected string _ImageSavePath = string.Empty;
        #endregion

        #region properties
          
        [System.Xml.Serialization.XmlAttribute]
        public string ImageSavePath
        {
            get { return _ImageSavePath; }

            set
            {
                _ImageSavePath = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlAttribute]
        public int GrabBufferCount
        {
            get { return _GrabBufferCount; }

            set
            {
                _GrabBufferCount = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlAttribute]
        public GrabberTypes GrabberType
        {
            get { return _GrabberType; }

            set
            {
                _GrabberType = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlElement]
        public List<T> CameraInfos
        {
            get { return _CameraInfos; }
            set
            {
                _CameraInfos = value;
                OnPropertyChanged();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public int CameraCount
        {
            get { return _CameraInfos.Count; }
        }
        #endregion

        #region Methods
        public T GetConfig(Imagers imager)
        {
            if (_CameraInfos == null)
                return default(T);

            if (_CameraInfos.Count < (int)Imagers.Max)
                return default(T);

            return _CameraInfos[(int)imager];
        }
        #endregion

        #region constructor

        public ImagerConfiguration()
        {
            _CameraInfos = new List<T>();

            _GrabberType = GrabberTypes.Simulator;

            _GrabBufferCount = 1;
            //_UseTrigger = false;

        }
        public ImagerConfiguration(int ImagerCount)
            : this()
        {
            for (int i = 0; i < ImagerCount; i++)
            {
                T info = new T();
                //info.CamNumber = i;
                _CameraInfos.Add(info);
            }
        }


        #endregion
    }
}
