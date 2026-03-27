using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    [DataContract]
    [Serializable]
    public abstract class BaseDataStructure : INotifyPropertyChanged, INotifyCollectionChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        [field: NonSerializedAttribute()]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

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

        public override string ToString()
        {
            return string.Empty;
        }

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



        //public virtual List<ExploredInfo> GetImages(string level)
        //{
        //    return new List<ExploredInfo>();
        //}

        //public virtual List<ExploredInfo> GetBlobs(string level)
        //{
        //    return new List<ExploredInfo>();
        //}

        //public static List<ExploredInfo> GetCollectionImages(IEnumerable enumerable, string level)
        //{
        //    List<ExploredInfo> items = new List<ExploredInfo>();

        //    if (enumerable == null)
        //        return items;

        //    if (enumerable is IDictionary)
        //    {
        //        enumerable = (enumerable as IDictionary).Values;
        //    }

        //    int index = 0;

        //    foreach (var item in enumerable)
        //    {
        //        if (item is IEnumerable)
        //        {
        //            var result = GetCollectionImages(item as IEnumerable, level + "." + item.GetIndexer() + item.GetType().Name + index);

        //            if (result != null && result.Any())
        //                items.AddRange(result);
        //        }
        //        else if (item is BaseDataStructure)
        //        {
        //            var result = (item as BaseDataStructure).GetImages(level + "." + item.GetIndexer() + item.GetType().Name + index);

        //            if (result != null && result.Any())
        //                items.AddRange(result);
        //        }

        //        index++;
        //    }

        //    return items;
        //}

        //public static List<ExploredInfo> GetCollectionBlobs(IEnumerable enumerable, string level)
        //{
        //    List<ExploredInfo> items = new List<ExploredInfo>();

        //    if (enumerable == null)
        //        return items;

        //    if (enumerable is IDictionary)
        //    {
        //        enumerable = (enumerable as IDictionary).Values;
        //    }

        //    int index = 0;

        //    foreach (var item in enumerable)
        //    {
        //        if (item is IEnumerable)
        //        {
        //            var result = GetCollectionImages(item as IEnumerable, level + "." + item.GetType().Name + index);

        //            if (result != null && result.Any())
        //                items.AddRange(result);
        //        }
        //        else if (item is BaseDataStructure)
        //        {
        //            var result = (item as BaseDataStructure).GetBlobs(level + "." + item.GetType().Name + index);

        //            if (result != null && result.Any())
        //                items.AddRange(result);
        //        }

        //        index++;
        //    }

        //    return items;
        //}
    }


    [DataContract]
    public class BaseNotificationDataStructure : BaseDataStructure
    {
        public BaseNotificationDataStructure()
        {

        }

        public virtual int GetMessageId()
        {
            return 0;
        }
        public virtual Int64 GetNotificationId()
        {
            return 0;
        }

        public override BaseDataStructure Copy()
        {
            throw new NotImplementedException();
        }

        public override void Clear()
        {

        }
    }

    [DataContract]
    public abstract class BaseDataStructureEventArgs : BaseDataStructure
    {
        public BaseDataStructureEventArgs()
        {
        }

        public override void Clear()
        {
        }
    }

}
