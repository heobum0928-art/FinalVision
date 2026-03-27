using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Project.BaseLib.Utils;
using Project.BaseLib.Enums;
using System.Collections.ObjectModel;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public class LoadPortInfo : NotifyPropertyChanged
    {
        #region fields
        //public readonly int MAX_SLOT_NUMBER;

        private Int32 _PortId;
        private Boolean _CarrierPresence;
        private Boolean _CarrierPlacement;
        private ClampStates _ClampState;
        private DockStates _DockState;
        private DoorStates _DoorState;

        protected string _CarrierId;

        protected ObservableCollection<CarrierSlot> _SlotMap;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
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

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
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

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public string LoadPortDescription
        {
            get
            {
                string description = string.Empty;


                description = string.Format($"Carrier Id : {_CarrierId}") + "\r\n";
                description += string.Format($"Foup Index : {_PortId}") + "\r\n";
                description += string.Format($"max slot count : {_SlotMap.Count}") + "\r\n";

                for (int i = _SlotMap.Count - 1; i >= 0; i--)
                {
                    description += string.Format($"{SlotMap[i].ToString()}") + "\r\n";
                }

                return description;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Int32 PortId
        {
            get
            {
                return _PortId;
            }
            set
            {
                if (_PortId != value)
                {
                    _PortId = value;
                    OnPropertyChanged("PortId");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Boolean CarrierPresence
        {
            get
            {
                return _CarrierPresence;
            }
            set
            {
                if (_CarrierPresence != value)
                {
                    _CarrierPresence = value;
                    OnPropertyChanged("CarrierPresence");

                    OnPropertyChanged("CarrierPlacemented");
                    OnPropertyChanged("LoadCompleted");
                    OnPropertyChanged("UnloadCompleted");
                    OnPropertyChanged("ClampCompleted");
                    OnPropertyChanged("DockCompleted");
                    OnPropertyChanged("UndockCompleted");
                    OnPropertyChanged("OpenCompleted");
                    OnPropertyChanged("CloseCmpleted");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public Boolean CarrierPlacement
        {
            get
            {
                return _CarrierPlacement;
            }
            set
            {
                if (_CarrierPlacement != value)
                {
                    _CarrierPlacement = value;
                    OnPropertyChanged("CarrierPlacement");

                    OnPropertyChanged("CarrierPlacemented");
                    OnPropertyChanged("LoadCompleted");
                    OnPropertyChanged("UnloadCompleted");
                    OnPropertyChanged("ClampCompleted");
                    OnPropertyChanged("DockCompleted");
                    OnPropertyChanged("UndockCompleted");
                    OnPropertyChanged("OpenCompleted");
                    OnPropertyChanged("CloseCmpleted");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public ClampStates ClampState
        {
            get
            {
                return _ClampState;
            }
            set
            {
                if (_ClampState != value)
                {
                    _ClampState = value;
                    OnPropertyChanged("ClampState");

                    OnPropertyChanged("CarrierPlacemented");
                    OnPropertyChanged("LoadCompleted");
                    OnPropertyChanged("UnloadCompleted");
                    OnPropertyChanged("ClampCompleted");
                    OnPropertyChanged("DockCompleted");
                    OnPropertyChanged("UndockCompleted");
                    OnPropertyChanged("OpenCompleted");
                    OnPropertyChanged("CloseCmpleted");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public DockStates DockState
        {
            get
            {
                return _DockState;
            }
            set
            {
                if (_DockState != value)
                {
                    _DockState = value;
                    OnPropertyChanged("DockState");

                    OnPropertyChanged("CarrierPlacemented");
                    OnPropertyChanged("LoadCompleted");
                    OnPropertyChanged("UnloadCompleted");
                    OnPropertyChanged("ClampCompleted");
                    OnPropertyChanged("DockCompleted");
                    OnPropertyChanged("UndockCompleted");
                    OnPropertyChanged("OpenCompleted");
                    OnPropertyChanged("CloseCmpleted");
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public DoorStates DoorState
        {
            get
            {
                return _DoorState;
            }
            set
            {
                if (_DoorState != value)
                {
                    _DoorState = value;
                    OnPropertyChanged("DoorState");

                    OnPropertyChanged("CarrierPlacemented");
                    OnPropertyChanged("LoadCompleted");
                    OnPropertyChanged("UnloadCompleted");
                    OnPropertyChanged("ClampCompleted");
                    OnPropertyChanged("DockCompleted");
                    OnPropertyChanged("UndockCompleted");
                    OnPropertyChanged("OpenCompleted");
                    OnPropertyChanged("CloseCmpleted");
                }
            }
        }

        public bool CarrierPlacemented
        {
            get
            {
                if (CarrierPlacement == true ||
                    ClampState == ClampStates.Clamped ||
                    DockState == DockStates.Docked ||
                    DoorState == DoorStates.Opened) return true;
                return false;
            }
        }

        public bool LoadCompleted
        {
            get
            {
                if (CarrierPresence == true &&
                    CarrierPlacement == true &&
                    ClampState == ClampStates.Clamped &&
                    DockState == DockStates.Docked &&
                    DoorState == DoorStates.Opened) return true;
                return false;
            }
        }

        public bool UnloadCompleted
        {
            get
            {
                if (//CarrierPresence != true ||
                    //CarrierPlacement != true ||
                    ClampState != ClampStates.Clamped &&
                    DockState != DockStates.Docked &&
                    DoorState != DoorStates.Opened) return true;
                return false;
            }
        }

        public bool ClampCompleted
        {
            get
            {
                if (CarrierPresence == true &&
                    CarrierPlacement == true &&
                    ClampState == ClampStates.Clamped) return true;
                return false;
            }
        }

        public bool DockCompleted
        {
            get
            {
                if (CarrierPresence == true &&
                    CarrierPlacement == true &&
                    ClampState == ClampStates.Clamped ||
                    DockState == DockStates.Docked) return true;
                return false;
            }
        }

        public bool UndockCompleted
        {
            get
            {
                if (DockState == DockStates.Undocked)
                    return true;

                return false;
            }
        }

        public bool OpenCompleted
        {
            get
            {
                if (CarrierPresence == true &&
                    CarrierPlacement == true &&
                    ClampState == ClampStates.Clamped &&
                    DockState == DockStates.Docked &&
                    DoorState == DoorStates.Opened) return true;

                return false;
            }
        }

        public bool CloseCmpleted
        {
            get
            {
                if (DoorState == DoorStates.Closed)
                    return true;

                return false;
            }

        }

        public CarrierStates ToCarrierState
        {
            get
            {
                if (CarrierPresence)
                {
                    if (CarrierPlacement)
                    {
                        if (ClampState == ClampStates.Clamped)
                        {
                            if (DockState == DockStates.Docked)
                            {
                                if (DoorState == DoorStates.Opened)
                                {
                                    return CarrierStates.Loaded;
                                }

                                return CarrierStates.Docked;
                            }

                            return CarrierStates.Clamped;
                        }

                        return CarrierStates.Placed;
                    }

                    return CarrierStates.Presence;
                }

                return CarrierStates.Empty;
            }
        }

        public bool CarrierProcessDone
        {
            get
            {
                if (_SlotMap == null)
                    return false;

                if (_SlotMap.Count == 0)
                    return false;

                var loadport_exist_list = _SlotMap.Where(w => w.SlotState != SlotStates.Empty).Select(s => s).ToList();

                var loadport_all_proc_finished = loadport_exist_list.All(a => a.ProcessFinished == true);

                return loadport_all_proc_finished;
            }
        }
        #endregion

        #region methods

        public void CreateSlotMap(int SlotCount, SlotStates state = SlotStates.Undefined)
        {
            SlotMap = new ObservableCollection<CarrierSlot>();

            for(int i = 0; i < SlotCount; i++)
            {
                var slot = new CarrierSlot(i + 1, state);
                slot.SubstrateInfo = new SubstrateInfo(_PortId, i + 1);

                SlotMap.Add(slot);
            }
        }

        public void Copy(LoadPortInfo copy_elem)
        {
            copy_elem.PortId = _PortId;
            copy_elem.CarrierPresence = _CarrierPresence;
            copy_elem.CarrierPlacement = _CarrierPlacement;
            copy_elem.ClampState = _ClampState;
            copy_elem.DockState = _DockState;
            copy_elem.DoorState = _DoorState;

            copy_elem.SlotMap = _SlotMap;
            copy_elem.CarrierId = _CarrierId;
        }

        public LoadPortInfo Copy()
        {
            LoadPortInfo copy_elem = new LoadPortInfo();
            Copy(copy_elem);
            return copy_elem;
        }

        public LoadPortInfo Duplicate()
        {
            return (LoadPortInfo)this.Copy();
        }

        public void Clear()
        {
            PortId = 0;
            CarrierPresence = false;
            CarrierPlacement = false;
            ClampState = ClampStates.Undefined;
            DockState = DockStates.Undefined;
            DoorState = DoorStates.Undefined;
        }

        protected void AllocAll()
        {
            PortId = 0;
            CarrierPresence = false;
            CarrierPlacement = false;
            ClampState = ClampStates.Undefined;
            DockState = DockStates.Undefined;
            DoorState = DoorStates.Undefined;
        }

        public static LoadPortInfo CreateNew()
        {
            LoadPortInfo temp = new LoadPortInfo();
            temp.AllocAll();
            return temp;
        }

        public override string ToString()
        {
            return string.Format("LoadPortInfo: [ PortId = {0}; CarrierPresence = {1}; CarrierPlacement = {2}; ClampState = {3}; DockState = {4}; DoorState = {5};]"
                , _PortId
                , _CarrierPresence
                , _CarrierPlacement
                , _ClampState
                , _DockState
                , _DoorState
                );
        }

        public string ToShortString()
        {
            return string.Format("PortId={0} {1}/{2} {3},{4},{5}",
                                    PortId,
                                    (CarrierPresence == true) ? "Presence" : "NoPresence",
                                    (CarrierPlacement == true) ? "Placed" : "NoPlaced",
                                    ClampState,
                                    DockState,
                                    DoorState);
        }
        #endregion

        #region constructors
        public LoadPortInfo()
        {
            this.ClampState = ClampStates.Undefined;
            this.DockState = DockStates.Undefined;
            this.DoorState = DoorStates.Undefined;
        }

        public LoadPortInfo(Int32 PortId, Boolean CarrierPresence, Boolean CarrierPlacement, ClampStates ClampState, DockStates DockState, DoorStates DoorState)
        {
            this.PortId = PortId;
            this.CarrierPresence = CarrierPresence;
            this.CarrierPlacement = CarrierPlacement;
            this.ClampState = ClampState;
            this.DockState = DockState;
            this.DoorState = DoorState;

            _SlotMap = new ObservableCollection<CarrierSlot>();
        }

        #endregion
    }
}
