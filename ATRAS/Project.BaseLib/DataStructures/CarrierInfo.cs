using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class CarrierSlot : NotifyPropertyChanged
    {
        #region fields
        protected SlotStates _SlotState;

        protected int _SlotNumber;

        protected SubstrateInfo _SubstrateInfo;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public SlotStates SlotState
        {
            get
            {
                return _SlotState;
            }

            set
            {
                if (_SlotState != value)
                {
                    _SlotState = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Description");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int SlotNumber
        {
            get
            {
                return _SlotNumber;
            }

            set
            {
                if (_SlotNumber != value)
                {
                    _SlotNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Description");
                }
            }
        }

        public bool IsWaferExist
        {
            get
            {
                if (SubstrateInfo == null)
                    return false;

                return true;
            }
        }

        public bool ProcessFinished
        {
            get
            {
                if(_SlotState == SlotStates.ProcessDone)
                {
                    return true;
                }
                return false;
            }

            set
            {
                if(value == true)
                {
                    SlotState = SlotStates.ProcessDone;
                    SubstrateInfo.ProcFinished = true;
                }
                else
                {
                    SlotState = SlotStates.Correct;
                    SubstrateInfo.ProcFinished = false;
                }

                OnPropertyChanged();
            }
        }

        #endregion

        #region methods
        public SubstrateInfo SubstrateInfo
        {
            get
            {
                return _SubstrateInfo;
            }

            set
            {
                _SubstrateInfo = value;
                OnPropertyChanged();
                OnPropertyChanged("Description");
            }
        }

        public CarrierSlot Duplicate()
        {
            var info = new CarrierSlot();

            info.SlotNumber = this.SlotNumber;
            info.SlotState = this.SlotState;
            info.SubstrateInfo = this.SubstrateInfo.Duplicate();

            return info;
        }

        public void Copy(CarrierSlot copy_elem)
        {
            copy_elem.SlotNumber = this.SlotNumber;
            copy_elem.SlotState = this.SlotState;
            copy_elem.SubstrateInfo = this.SubstrateInfo.Duplicate();
        }

        public override string ToString()
        {
            string finished = ProcessFinished == true ? "Finsihed" : "None-Finished";
            return string.Format($"[{_SlotNumber}] SlotState : [{_SlotState}], ProcStatus : [{finished}]");
        }

        public string Description
        {
            get
            {
                string finished = ProcessFinished == true ? "Finsihed" : "None-Finished";

                string proc_time = SubstrateInfo == null ? "none" : (SubstrateInfo.EndTime - SubstrateInfo.StartTime).TotalMilliseconds.ToString();

                return string.Format($"[{_SlotNumber}] SlotState : [{_SlotState}], ProcStatus : [{finished}], StartTime : [{SubstrateInfo.StartTime.ToString("HHmmss")}], EndTime : [{SubstrateInfo.EndTime.ToString("HHmmss")}], ProcTime : [{proc_time} ms]");
            }
        }
        #endregion

        #region constructors
        public CarrierSlot()
            : this(0, SlotStates.Undefined)
        {
            
        }

        public CarrierSlot(int SlotNumber)
            : this(SlotNumber, SlotStates.Undefined)
        {
            
        }

        public CarrierSlot(int SlotNumber, SlotStates state)
        {
            this.SlotState = state;

            this.SlotNumber = SlotNumber;

            this.SubstrateInfo = new SubstrateInfo(0, SlotNumber);
        }
        #endregion
    }

    [DataContract]
    [System.SerializableAttribute()]
    public class CarrierInfo : NotifyPropertyChanged
    {
        #region fields
        public readonly int MAX_SLOT_NUMBER;
        public readonly int FOUP_INDEX;

        protected string _CarrierId;

        protected ObservableCollection<CarrierSlot> _SlotMap;

        #endregion

        #region propertise
        public ObservableCollection<CarrierSlot> SlotMap
        {
            get
            {
                return _SlotMap;
            }

            set
            {
                _SlotMap = value;
                OnPropertyChanged();
            }
        }

        public bool ProcessDone
        {
            get
            {
                if(SlotMap != null && SlotMap.Count != 0)
                {
                    return SlotMap.All(a => a.SubstrateInfo.ProcFinished == true);
                }

                return false;
            }
        }

        public string CarrierId
        {
            get
            {
                return _CarrierId;
            }

            set
            {
                _CarrierId = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get
            {
                string description = string.Empty;

                description = string.Format($"Carrier Id : {_CarrierId}") + "\r\n";
                description += string.Format($"Foup Index : {FOUP_INDEX}") + "\r\n";
                description += string.Format($"max slot count : {MAX_SLOT_NUMBER}") + "\r\n";

                for(int i = MAX_SLOT_NUMBER-1; i >= 0; i--)
                {
                    description += string.Format($"{SlotMap[i].ToString()}") + "\r\n";
                }

                return description;
            }
        }
        #endregion

        #region methods
        public void SetAllStatus(SlotStates state)
        {
            foreach(var info in _SlotMap)
            {
                info.SlotState = state;
            }
        }

        public void SetSlotStatus(int slot, SlotStates state)
        {
            var info = _SlotMap.FirstOrDefault(f => f.SlotNumber == slot);
            if(info != null)
            {
                info.SlotState = state;
            }
        }
        #endregion

        #region constructors

        public CarrierInfo(int INDEX, string _CarrierId, int MAX_SLOT_NUMBER)
        {
            this.FOUP_INDEX = INDEX;

            this.MAX_SLOT_NUMBER = MAX_SLOT_NUMBER;

            // _SlotInfoArray = new SlotInfo[MAX_SLOT_NUMBER];
            SlotMap = new ObservableCollection<CarrierSlot>();

            this.CarrierId = _CarrierId;

            for (int i = 0; i < MAX_SLOT_NUMBER; i++)
            {
                var slot = new CarrierSlot(i + 1, SlotStates.Undefined);
                slot.SubstrateInfo = new SubstrateInfo(INDEX, i+1);
                SlotMap.Add(slot);
                
            }
        }

        public CarrierInfo(int index, string _CarrierId)
            : this(index, _CarrierId, 25)
        {

        }

        public CarrierInfo(int INDEX, int MAX_SLOT_NUMBER)
            : this(INDEX, "unknown", MAX_SLOT_NUMBER)
        {

        }

        public CarrierInfo()
            : this(0, "unknown", 25)
        {
        }
        #endregion
    }
}
