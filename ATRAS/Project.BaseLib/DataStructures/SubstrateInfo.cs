using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    public class SubstrateInfo : NotifyPropertyChanged
    {
        #region fields

        protected int _Port;
        protected int _Slot;

        protected bool _ProcFinished;

        protected DateTime _StartTime;

        protected DateTime _EndTime;
        
        #endregion

        #region propertise
        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                _Port = value;
                OnPropertyChanged();
            }
        }

        public int Slot
        {
            get
            {
                return _Slot;
            }

            set
            {
                _Slot = value;
                OnPropertyChanged();
            }
        }

        public bool ProcFinished
        {
            get
            {
                return _ProcFinished;
            }

            set
            {
                _ProcFinished = value;
                OnPropertyChanged();
            }
        }

        public DateTime StartTime
        {
            get
            {
                return _StartTime;
            }

            set
            {
                _StartTime = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndTime
        {
            get
            {
                return _EndTime;
            }

            set
            {
                _EndTime = value;
                OnPropertyChanged();
            }
        }

        public int TotalProcessingTimeMS
        {
            get
            {
                if(_StartTime != null && _EndTime != null)
                {
                    return (_EndTime - _StartTime).Milliseconds;
                }

                return 0;
            }
        }

        #endregion

        #region methods
        public void Copy(SubstrateInfo copy_elem)
        {
            
            copy_elem.Port = this.Port;
            copy_elem.Slot = this.Slot;
            copy_elem.ProcFinished = this.ProcFinished;

            copy_elem._StartTime = this._StartTime;
            copy_elem._EndTime = this._EndTime;

        }
        public SubstrateInfo Duplicate()
        {
            SubstrateInfo info = new SubstrateInfo();

            info.StartTime = this.StartTime;
            info.EndTime = this.EndTime;

            info.Port = this.Port;
            info.Slot = this.Slot;

            info.ProcFinished = this.ProcFinished;

            return info;
        }

        public override string ToString()
        {
            return string.Format($"Port : {_Port}, Slot : {_Slot}, Process Finished : {_ProcFinished}, StartTime : {_StartTime.ToString("HH: mm:ss,ffff")}, EndTime : {_EndTime.ToString("HH:mm:ss,ffff")}, TotalProcessingTime : {TotalProcessingTimeMS} ms");
        }

        public string ToStringDescription()
        {
            return string.Format($"{_Port} / {_Slot} / {_ProcFinished}");
        }
        #endregion

        #region constructors
        public SubstrateInfo()
            : this(0, 0)
        {
            _ProcFinished = false;
        }

        public SubstrateInfo(int _Port, int _Slot)
        {
            this._Port = _Port;
            this._Slot = _Slot;

            _ProcFinished = false;
        }
        #endregion
    }
}
