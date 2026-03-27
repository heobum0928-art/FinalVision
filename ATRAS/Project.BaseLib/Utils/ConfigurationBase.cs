using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.BaseLib.Utils
{
    public interface IConfiguration
    {
        bool Exists();
        void Clear();
        void Save();
        void Load();

        void SetData(string xml);
        string GetData();
    }
    [System.SerializableAttribute]
    public abstract class ConfigurationBase : IConfiguration
    {
        #region fields
        protected ILogger logger;
        protected string categoryName = string.Empty;
        protected string itemName = string.Empty;

        //public static string FILE_PATH;
        private DateTime _CreationDate;
        private String _Version;
        public readonly string ROOT_PATH = Directory.GetCurrentDirectory() + "\\..\\Data";
        #endregion

        #region properties
        [System.Xml.Serialization.XmlElement]
        public DateTime CreationData
        {
            get { return _CreationDate; }

            set
            {
                if (_CreationDate != value)
                {
                    _CreationDate = value;
                }
            }
        }
        [System.Xml.Serialization.XmlAttribute]
        public String Version
        {
            get { return _Version; }

            set
            {
                if (_Version != value)
                {
                    _Version = value;
                }
            }
        }
        #endregion

        #region constructor
        public ConfigurationBase()
        {
            this.logger = LogManager.GetLogger("ConfigurationBase");
            this._CreationDate = DateTime.Now;
            this._Version = "DefaultVersion";
        }
        public ConfigurationBase(DateTime CreationDate, String Version)
        {
            this.logger = LogManager.GetLogger("ConfigurationBase");
            this._CreationDate = CreationDate;
            this._Version = Version;
        }
        #endregion

        #region methods 
        public bool Exists()
        {
            string path = ROOT_PATH + "\\" + categoryName + "\\" + itemName + ".xml";

            return File.Exists(path);
        }
        public abstract void Copy(ConfigurationBase config);
        public abstract void AllocAll();
        public void Clear()
        {
            this._CreationDate = DateTime.Now;
            this._Version = String.Empty;
        }
        public virtual void Save()
        {
            string currentDir = ROOT_PATH + "\\" + categoryName;

            if (!Directory.Exists(currentDir))
                Directory.CreateDirectory(currentDir);
            string fullpath = currentDir + "\\" + itemName + ".xml";

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
        public virtual void Load()
        {
            string path = ROOT_PATH + "\\" + categoryName + "\\" + itemName + ".xml";

            if (!File.Exists(path))
                return;

            using (FileStream fs = new FileStream(path, FileMode.Open))
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
        
        
        public virtual void SetData(string xml)
        {
            using (StringReader sr = new StringReader(xml))
            {
                try
                {
                    XmlSerializer xs = new XmlSerializer(this.GetType());
                    ConfigurationBase config = (ConfigurationBase)xs.Deserialize(sr);
                    config.Copy(this);
                }
                catch(Exception e)
                {
                    logger.Error(e, "Invalid value type! Please Check Exception Message.");
                }
            }
        }
        public virtual string GetData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(this.GetType());
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                xs.Serialize(ms, this, ns);

                var buf = ms.ToArray();

                return Encoding.UTF8.GetString(buf);                        
            }
        }
        #endregion
    }
}

