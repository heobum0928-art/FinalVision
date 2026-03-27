using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.DeepLearning.UI
{
    [DataContract]
    [System.SerializableAttribute]
    public class DeepLearningConfiguration : ConfigurationBase
    {
        #region fields
        protected int _ItemWidth;
        protected int _ItemHeight;


        protected int _Epoch;
        protected int _BatchSize;

        protected int _Patience;

        protected double _LearningRatio;

        protected ClsssNameList _ClassNameList;

        #endregion

        #region propertise
        [System.Xml.Serialization.XmlElement]
        public ClsssNameList ClassList
        {
            get
            {
                return _ClassNameList;
            }

            set
            {
                _ClassNameList = value;
                
            }
        }





        [System.Xml.Serialization.XmlElement]
        public string ModelName
        {
            get
            {
                return itemName;
            }

            set
            {
                itemName = value;
            }
        }


        [System.Xml.Serialization.XmlElement]
        public int Epoch
        {
            get
            {
                return _Epoch;
            }

            set
            {
                _Epoch = value;

            }
        }

        [System.Xml.Serialization.XmlElement]
        public int BatchSize
        {
            get
            {
                return _BatchSize;
            }

            set
            {
                _BatchSize = value;

            }
        }

        [System.Xml.Serialization.XmlElement]
        public int Patience
        {
            get
            {
                return _Patience;
            }

            set
            {
                _Patience = value;

            }
        }

        [System.Xml.Serialization.XmlElement]
        public double LearningRatio
        {
            get
            {
                return _LearningRatio;
            }

            set
            {
                _LearningRatio = value;

            }
        }

        [System.Xml.Serialization.XmlElement]
        public int ItemWidth
        {
            get { return _ItemWidth; }
            set
            {
                if (_ItemWidth != value)
                {
                    _ItemWidth = value;
                }
            }
        }
        [System.Xml.Serialization.XmlElement]
        public int ItemHeight
        {
            get { return _ItemHeight; }
            set
            {
                if (_ItemHeight != value)
                {
                    _ItemHeight = value;
                }
            }
        }
        #endregion

        #region methods
        public override void AllocAll()
        {
            throw new NotImplementedException();
        }

        public override void Copy(ConfigurationBase config)
        {
            var sysConfig = (config as DeepLearningConfiguration);

            if (sysConfig != null)
            {
                sysConfig._ItemWidth = this._ItemWidth;
                sysConfig._ItemHeight = this._ItemHeight;
                sysConfig._Patience = this._Patience;

                sysConfig._Epoch = this._Epoch;
                sysConfig._BatchSize = this._BatchSize;
                sysConfig._LearningRatio = this._LearningRatio;

                sysConfig._ClassNameList = this._ClassNameList;
            }
        }

        public void Save(string path, string name)
        {
            string currentDir = path + "\\Recipe\\" + name;

            if (!Directory.Exists(currentDir))
                Directory.CreateDirectory(currentDir);
            string fullpath = currentDir + "\\" + name + ".xml";

            if (File.Exists(fullpath))
                File.Delete(fullpath);

            using (FileStream fs = new FileStream(fullpath, FileMode.Create))
            {
                XmlSerializer xs = new XmlSerializer(this.GetType());
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                xs.Serialize(fs, this, ns);
            }


            
        }

        public void Load(string path, string name)
        {
            string path2 = path + "\\Recipe\\" + name + "\\" + name + ".xml";

            if (!File.Exists(path2))
                return;

            using (FileStream fs = new FileStream(path2, FileMode.Open))
            {
                XmlSerializer xs = new XmlSerializer(this.GetType());

                try
                {
                    ConfigurationBase config = (ConfigurationBase)xs.Deserialize(fs);

                    config.Copy(this);
                }
                catch (Exception e)
                {

                    //logger.Error(e, "Invalid value type! Please Check Exception Message.");
                    AppLogger.Error()("Invalid value type! Please Check Exception Message : {0}", e.Message);
                }

            }
        }

        #endregion

        #region constructors
        public DeepLearningConfiguration()
        : base(DateTime.Now, "Default")
        {
            categoryName = "System";
            itemName = "DeepLearningConfiguration";

            this._ItemWidth = 200;
            this._ItemHeight = 200;
            
            this._Epoch = 50;
            this._BatchSize = 16;
            this._Patience = 3;
            this._LearningRatio = 0.001;

            _ClassNameList = new ClsssNameList();
        }
        #endregion

    }
}
