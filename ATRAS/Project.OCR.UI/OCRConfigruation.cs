using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.OCR.UI
{
    [DataContract]
    [System.SerializableAttribute]
    public class OCRConfigruation : ConfigurationBase
    {
        #region fields
        protected int _CharacterMinWidth;
        protected int _CharacterMaxWidth;

        protected int _CharacterMinHeight;
        protected int _CharacterMaxHeight;

        protected int _CharMinArea;
        protected int _CharMaxArea;

        protected int _CharacterThreshold;

        //protected int _CharCount;
        protected int _DilationErosion;

        protected int _CharGap;

        protected int _FillHoleArea;

        protected CharacterPolarity _CharacterPolarity;
        
        protected bool _AutoProcessing;

        protected bool _GrayImage;


        protected int _OtsuLevel;


        #endregion

        #region propertise

        [System.Xml.Serialization.XmlElement]
        public CharacterPolarity CharacterPolarity
        {
            get
            {
                return _CharacterPolarity;
            }

            set
            {
                _CharacterPolarity = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int CharacterMinWidth
        {
            get
            {
                return _CharacterMinWidth;
            }

            set
            {
                _CharacterMinWidth = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int CharacterMaxWidth
        {
            get
            {
                return _CharacterMaxWidth;
            }

            set
            {
                _CharacterMaxWidth = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int CharacterMinHeight
        {
            get
            {
                return _CharacterMinHeight;
            }

            set
            {
                _CharacterMinHeight = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int CharacterMaxHeight
        {
            get
            {
                return _CharacterMaxHeight;
            }

            set
            {
                _CharacterMaxHeight = value;
            }
        }




        [System.Xml.Serialization.XmlElement]
        public int CharMinArea
        {
            get
            {
                return _CharMinArea;
            }

            set
            {
                _CharMinArea = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int CharMaxArea
        {
            get
            {
                return _CharMaxArea;
            }

            set
            {
                _CharMaxArea = value;
            }
        }



        [System.Xml.Serialization.XmlElement]
        public int CharacterThreshold
        {
            get
            {
                return _CharacterThreshold;
            }

            set
            {
                _CharacterThreshold = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public int DilationErosion
        {
            get
            {
                return _DilationErosion;
            }

            set
            {
                _DilationErosion = value;
            }
        }


        [System.Xml.Serialization.XmlElement]
        public int CharGap
        {
            get
            {
                return _CharGap;
            }

            set
            {
                _CharGap = value;
            }
        }
        
        [System.Xml.Serialization.XmlElement]
        public int FillHoleArea
        {
            get
            {
                return _FillHoleArea;
            }

            set
            {
                _FillHoleArea = value;
            }
        }
        [System.Xml.Serialization.XmlElement]
        public bool AutoProcessing
        {
            get
            {
                return _AutoProcessing;
            }

            set
            {
                _AutoProcessing = value;
            }
        }

        
        [System.Xml.Serialization.XmlElement]
        public bool GrayImage
        {
            get
            {
                return _GrayImage;
            }

            set
            {
                _GrayImage = value;
            }
        }


        [System.Xml.Serialization.XmlElement]
        public int OtsuLevel
        {
            get
            {
                return _OtsuLevel;
            }

            set
            {
                _OtsuLevel = value;
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
            var sysConfig = (config as OCRConfigruation);

            if (sysConfig != null)
            {
                sysConfig._CharacterMinWidth = this._CharacterMinWidth;
                sysConfig._CharacterMaxWidth = this._CharacterMaxWidth;
                
                sysConfig._CharacterMinHeight = this._CharacterMinHeight;
                sysConfig._CharacterMaxHeight = this._CharacterMaxHeight;

                sysConfig._CharMinArea = this._CharMinArea;
                sysConfig._CharMaxArea = this._CharMaxArea;

                sysConfig._CharacterThreshold = this._CharacterThreshold;

                sysConfig._CharacterPolarity = this._CharacterPolarity;

                sysConfig._DilationErosion = this._DilationErosion;

                sysConfig._CharGap = this._CharGap;
                sysConfig._FillHoleArea = this._FillHoleArea;

                sysConfig._AutoProcessing = this._AutoProcessing;

                sysConfig._GrayImage = this._GrayImage;

                sysConfig._OtsuLevel = this._OtsuLevel;
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
        public OCRConfigruation()
            : base(DateTime.Now, "Default")
        {
            categoryName = "System";
            itemName = "OCRConfiguration";

            //this._ItemWidth = 200;
            //this._ItemHeight = 200;

            //this._Epoch = 50;
            //this._BatchSize = 16;
            //this._Patience = 3;
            //this._LearningRatio = 0.001;

            //_ClassNameList = new ClsssNameList();

            _CharacterPolarity = CharacterPolarity.DarkOnLight;

            _CharacterMinWidth = 10;
            _CharacterMaxWidth = 20;

            _CharacterMinHeight = 20;
            _CharacterMaxHeight = 30;


            _CharacterThreshold = 50;

            _DilationErosion = 1;

            _CharGap = 1;


            _AutoProcessing = false;

            _GrayImage = false;

            _OtsuLevel = 0;
            //_CharacterInfo = new CharacterInfo();

            //_FontInfo = new CharacterInfo();
        }
        #endregion
    }
}
