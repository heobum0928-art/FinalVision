using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;

using System.Collections.Specialized;
using System.ComponentModel;

namespace Project.BaseLib.Utils
{
    public class NotifyPropertyChanged : INotifyPropertyChanged, INotifyCollectionChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionAdded([CallerMemberName] string name = null)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if(handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            }
        }

        protected void OnCollectionRemoved([CallerMemberName] string name = null)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            }
        }
    }
}
