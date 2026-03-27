using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;


using Project.BaseLib.Utils;


namespace Project.HWC
{
    [DataContract]
    [System.SerializableAttribute]
    public class ChamberConfig : NotifyPropertyChanged
    {
        #region fields
        protected int _RobotStationNumber;

        protected int _ProcessTime_Sec;

        protected int _Index;
        #endregion

        #region propertise
        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int Index
        {
            get
            {
                return _Index;
            }

            set
            {
                _Index = value;
                OnPropertyChanged();
            }
        }


        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int ProcessTime_Sec
        {
            get
            {
                return _ProcessTime_Sec;
            }

            set
            {
                _ProcessTime_Sec = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlElement]
        public int RobotStationNumber
        {
            get
            {
                return _RobotStationNumber;
            }

            set
            {
                _RobotStationNumber = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods

        #endregion

        #region constructors
        public ChamberConfig()
        {
            _Index = 0;
            _RobotStationNumber = 0;
            _ProcessTime_Sec = 60;
        }
        #endregion


    }
}
