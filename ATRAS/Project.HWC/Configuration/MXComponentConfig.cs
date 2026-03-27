using Project.BaseLib.Enums;
using Project.BaseLib.HW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.HWC
{
    [DataContract]
    [System.SerializableAttribute]
    public class MXComponentConfig : DeviceTypeConfig
    {
        #region fields

        private int _LogicalStationNumber = 0;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int LogicalStationNumber
        {
            get { return _LogicalStationNumber; }

            set { _LogicalStationNumber = value; }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public MXComponentConfig()
        {
            DeviceType = DeviceTypes.NotUsed;
            
        }

        #endregion
    }
}
