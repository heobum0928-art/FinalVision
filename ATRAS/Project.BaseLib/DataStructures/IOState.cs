using Project.BaseLib.Enums;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.DataStructures
{
    public class IOState : NotifyPropertyChanged
    {
        #region fields
        protected int _Number;

        protected string _Description;

        protected IOStates _State;

        #endregion

        #region propertise
        public int Number
        {
            get
            {
                return _Number;
            }

            //set
            //{
            //    _Number = value;
            //    OnPropertyChanged();
            //}
        }
        public string Description
        {
            get
            {
                return _Description;
            }

            set
            {
                _Description = value;
                OnPropertyChanged();
            }
        }
        public IOStates State
        {
            get
            {
                return _State;
            }

            set
            {
                _State = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region methods
        public override string ToString()
        {
            return string.Format($"Number : {_Number}, Description : {_Description}, State : {_State}");
        }
        #endregion

        #region constructors
        protected IOState()
            :this(0, "Unknown")
        {
            _State = IOStates.Unknown;
        }

        public IOState(int number, string description)
        {
            _Number = number;

            _Description = description;

            _State = IOStates.Unknown;
        }
        #endregion

    }




}
