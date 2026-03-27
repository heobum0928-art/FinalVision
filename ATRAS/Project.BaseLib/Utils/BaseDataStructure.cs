using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    [DataContract]
    [Serializable]
    public abstract class BaseDataStructure : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public override int GetHashCode()
        {
            return 0;
        }

        static BaseDataStructure()
        {

        }

        public override bool Equals(object obj)
        {
            return true;
        }

        public virtual BaseDataStructure Copy()
        {
            throw new NotImplementedException("Cannot use Copy method of BaseDataStructure. You must implement Copy method");
        }
        protected void Copy(BaseDataStructure copy)
        { }

        protected virtual void AllocAll()
        { }

        public abstract void Clear();

        public void RaisePropertyChanged(string propertyName)
        {
            RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void RaisePropertyChanged(object sender, PropertyChangedEventArgs arg)
        {
            if (PropertyChanged != null)
                PropertyChanged(sender, arg);
        }

        protected virtual void RaiseCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                {
                    foreach (var item in e.NewItems)
                    {
                        var bsd = item as BaseDataStructure;
                        if (bsd != null)
                            bsd.PropertyChanged += new PropertyChangedEventHandler(this.RaisePropertyChanged);
                    }
                    break;
                }
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                {
                    foreach (var item in e.OldItems)
                    {
                        var bsd = item as BaseDataStructure;
                        if (bsd != null)
                            bsd.PropertyChanged -= new PropertyChangedEventHandler(this.RaisePropertyChanged);
                    }
                    break;
                }
            }
        }
    }
}
