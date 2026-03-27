using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class PLCProtocalBase
    {
        #region fields
        protected short[] data = null;

        #endregion

        #region propertise
        [DataMember]
        [XmlElement]
        public short[] Data
        {
            get
            {
                return data;
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        protected PLCProtocalBase() { }

        public PLCProtocalBase(short[] data) 
        {
            this.data = new short[data.Length];

            Array.Copy(data, this.data, data.Length);
        }
        #endregion

    }
}
