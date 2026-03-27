using Project.BaseLib.HW;
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
    public class ClsssNameList : HWConfigBase
    {
        #region fields
        protected List<string> _ClassName;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public List<string> ClassName
        {
            get
            {
                return _ClassName;
            }

            set
            {
                _ClassName = value;
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public ClsssNameList()
        {
            _ClassName = new List<string>();
        }

        #endregion


    }
}
