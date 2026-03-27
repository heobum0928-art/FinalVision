using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.DeepLearning.UI
{
    [DataContract]
    [System.SerializableAttribute]
    public class DLSystemConfiguration : ConfigurationBase
    {
        #region fields
        protected string _RecipePath;
        protected string _RecipeName;
        #endregion

        #region propertise
        [System.Xml.Serialization.XmlElement]
        public string RecipePath
        {
            get
            {
                return _RecipePath;
            }

            set
            {
                _RecipePath = value;
            }
        }

        [System.Xml.Serialization.XmlElement]
        public string RecipeName
        {
            get
            {
                return _RecipeName;
            }

            set
            {
                _RecipeName = value;
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
            var DLConfig = (config as DLSystemConfiguration);

            if (DLConfig != null)
            {
                DLConfig._RecipeName = this._RecipeName;
                DLConfig._RecipePath = this._RecipePath;
            }
        }

        //public void Save(string name)
        //{
        //    itemName = name;
        //    _RecipeName = name;
        //    base.Save();
        //}

        //public void Load(string name)
        //{
        //    itemName = name;
        //    _RecipeName = name;
        //    base.Load();
        //}

        #endregion

        #region constructors
        public DLSystemConfiguration()
            : base(DateTime.Now, "Default")
        {
            categoryName = "System";
            itemName = "DLSystemConfiguration";
                       
            _RecipePath = string.Empty;
            _RecipeName = string.Empty;
        }
        #endregion

    }
}
