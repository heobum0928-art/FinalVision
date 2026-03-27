using Project.BaseLib.Utils;
using Project.BaseLib.Enums;
using Project.BaseLib.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    [System.Runtime.Serialization.DataContract]
    [System.SerializableAttribute()]
    public class SlotMapResult : NotifyPropertyChanged
    {
        #region fields
        private ReadStatus _Status;
        private List<CarrierSlot> _SlotMap;
        #endregion

        #region propertise
        [System.Runtime.Serialization.DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public ReadStatus Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                }
            }
        }

        [System.Runtime.Serialization.DataMember]
        [System.Xml.Serialization.XmlArrayItemAttribute("CarrierSlot"), System.ComponentModel.DefaultValue(null)]
        public List<CarrierSlot> SlotMap
        {
            get
            {
                return _SlotMap;
            }
            set
            {
                if (_SlotMap != value)
                {
                    _SlotMap = value;
                }
            }
        }


        [System.Xml.Serialization.XmlIgnore]
        public bool SlotMapSpecified
        {
            get
            {
                return _SlotMap != null && _SlotMap.Count > 0;
            }
            set
            {
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format("SlotMapResult: {0} [ Status = {1}; SlotMap = {2};]"
                , base.ToString()
                , _Status
                , _SlotMap != null ? _SlotMap.ToStringItems(256) : null
                );
        }
        public void Clear()
        {
            Status = ReadStatus.ReadOK;
            if (SlotMap != null)
                SlotMap.Clear();
        }

        protected void AllocAll()
        {
            Status = ReadStatus.ReadOK;
            SlotMap = new List<CarrierSlot>();
        }

        public static SlotMapResult CreateNew()
        {
            SlotMapResult temp = new SlotMapResult();
            temp.AllocAll();
            return temp;
        }
        public void Copy(SlotMapResult copy_elem)
        {
            copy_elem._Status = _Status;
            if(_SlotMap != null)
            {
                copy_elem._SlotMap = new List<CarrierSlot>();

                foreach (CarrierSlot msg in _SlotMap)
                    copy_elem._SlotMap.Add((msg == null) ? null : msg.Duplicate());
            }
        }
        public SlotMapResult Duplicate()
        {
            SlotMapResult smr = new SlotMapResult();

            Copy(smr);

            return smr;
        }
        #endregion

        #region constructors
        public SlotMapResult()
        {
            this.Status = ReadStatus.ReadOK;
        }

        public SlotMapResult(ReadStatus Status, List<CarrierSlot> SlotMap)
        {
            this.Status = Status;
            this.SlotMap = SlotMap;
        }
        #endregion

    }
}
