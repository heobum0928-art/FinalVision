using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class ThreadSafeList<T> : IList<T>
    {

        #region fields

        #endregion

        #region propertise
        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        #endregion

        #region methods
        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region constructors

        #endregion




    }


    //public class ThreadSafeList<T> : IList<T>
    //{
    //    protected List<T> _internalList = new List<T>();

    //    // Other Elements of IList implementation

    //    public IEnumerator<T> GetEnumerator()
    //    {
    //        return Clone().GetEnumerator();
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return Clone().GetEnumerator();
    //    }

    //    protected static object _lock = new object();

    //    public List<T> Clone()
    //    {
    //        List<T> newList = new List<T>();

    //        lock (_lock)
    //        {
    //            _internalList.ForEach(x => newList.Add(x));
    //        }

    //        return newList;
    //    }
    //}
}
