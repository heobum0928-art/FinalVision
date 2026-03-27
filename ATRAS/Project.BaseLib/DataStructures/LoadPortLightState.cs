using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

using Project.BaseLib.Utils;
using Project.BaseLib.Enums;

namespace Project.BaseLib.DataStructures
{
    [DataContract]
    [System.SerializableAttribute()]
    public partial class LoadPortLightState : NotifyPropertyChanged
    {
        #region Fields
        private LoadPortStates _LoadPortState;
        private LightStates _LightState;
        #endregion

        #region Constructors
        public LoadPortLightState()
        {
            this.LoadPortState = LoadPortStates.Undefined;
            this.LightState = LightStates.Unknown;
        }

        public LoadPortLightState(LoadPortStates LoadPortState, LightStates LightState)
        {
            this.LoadPortState = LoadPortState;
            this.LightState = LightState;
        }
        #endregion

        #region Properties
        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public LoadPortStates LoadPortState
        {
            get
            {
                return _LoadPortState;
            }
            set
            {
                if (_LoadPortState != value)
                {
                    _LoadPortState = value;
                }
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlAttribute]
        public LightStates LightState
        {
            get
            {
                return _LightState;
            }
            set
            {
                if (_LightState != value)
                {
                    _LightState = value;
                }
            }
        }

        #endregion

        #region Overridden Methods
        public override bool Equals(object obj)
        {
            LoadPortLightState other = obj as LoadPortLightState;
            if (other == null)
                return false;
            if (_LoadPortState != null && other._LoadPortState != null)
            {
                if (!other._LoadPortState.Equals(_LoadPortState))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_LoadPortState != other._LoadPortState)
                return false;
            if (_LightState != null && other._LightState != null)
            {
                if (!other._LightState.Equals(_LightState))
                    return false;
            }
            //Here both should be null so they should equal each other, otherwise return false
            else if (_LightState != other._LightState)
                return false;

            return base.Equals(obj);
        }



        public void Copy(LoadPortLightState copy_elem)
        {
            copy_elem._LoadPortState = _LoadPortState;
            copy_elem._LightState = _LightState;
        }

        public LoadPortLightState Copy()
        {
            LoadPortLightState copy_elem = new LoadPortLightState();
            Copy(copy_elem);
            return copy_elem;
        }

        public LoadPortLightState Duplicate()
        {
            return (LoadPortLightState)this.Copy();
        }

        public void Clear()
        {
            LoadPortState = LoadPortStates.Undefined;
            LightState = LightStates.Unknown;
        }

        protected void AllocAll()
        {
            LoadPortState = LoadPortStates.Undefined;
            LightState = LightStates.Unknown;
        }

        public static LoadPortLightState CreateNew()
        {
            LoadPortLightState temp = new LoadPortLightState();
            temp.AllocAll();
            return temp;
        }
        public override string ToString()
        {
            return string.Format("LoadPortLightState: {0} [ LoadPortState = {1}; LightState = {2};]"
                , base.ToString()
                , _LoadPortState
                , _LightState
                );
        }

        #endregion
    }
}
