using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.OCR.UI
{
    [DataContract]
    [System.SerializableAttribute]
    public class OCRSystemConfiguration : ConfigurationBase
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
            var OCRConfig = (config as OCRSystemConfiguration);

            if (OCRConfig != null)
            {
                OCRConfig._RecipeName = this._RecipeName;
                OCRConfig._RecipePath = this._RecipePath;
            }
        }
        #endregion

        #region constructors
        public OCRSystemConfiguration()
            : base(DateTime.Now, "Default")
        {
            categoryName = "System";
            itemName = "OCRSystemConfiguration";

            _RecipePath = string.Empty;
            _RecipeName = string.Empty;
        }
        #endregion
    }
}
