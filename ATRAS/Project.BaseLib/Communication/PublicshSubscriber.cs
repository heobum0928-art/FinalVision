using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{

    #region - PublishSubscriber
    // E와 M이 다른 이유
    // E 는 상속받을 Type. M은 Action할 Type.
    public interface IPublishSubscriber<E>
    where E : BaseDataStructure
    {
        void Subscribe<M>(Action<object, E> eventHandler, bool baseClass = false) where M : E;

        void Unsubscribe<M>(Action<object, E> eventHandler) where M : E;
    }


    public class PublishSubscriber<E> : IPublishSubscriber<E>
        where E : BaseDataStructure
    {
        protected Dictionary<Type, Action<object, E>> EventsMap;
        protected Dictionary<Type, Action<object, E>> EventsMapSubclass;

        public PublishSubscriber()
        {
            EventsMap = new Dictionary<Type, Action<object, E>>();
            EventsMapSubclass = new Dictionary<Type, Action<object, E>>();
        }
        public bool IsSubscribed<M>(Action<object, E> eventHandler)
        {
            Type key = typeof(M);
            if (!EventsMap.ContainsKey(key))
                return false;
            if (EventsMap[key] == null)
                return false;

            var invList = EventsMap[key].GetInvocationList();
            if (invList != null)
            {
                return invList.Any(d => d.Equals(eventHandler));
            }

            return false;
        }

        public void Subscribe<M>(Action<object, E> eventHandler, bool baseClass = false) where M : E
        {
            Type key = typeof(M);

            if (!EventsMap.ContainsKey(key))
                EventsMap[key] = null;

            EventsMap[key] += eventHandler;

            if (baseClass)
            {
                if (!EventsMapSubclass.ContainsKey(key))
                    EventsMapSubclass[key] = null;

                EventsMapSubclass[key] += eventHandler;
            }
        }

        public void Unsubscribe<M>(Action<object, E> eventHandler) where M : E
        {
            Type key = typeof(M);

            if (EventsMap.ContainsKey(key))
            {
                EventsMap[key] -= eventHandler;
            }

            if (EventsMapSubclass.ContainsKey(key))
            {
                EventsMapSubclass[key] -= eventHandler;
            }
        }

        protected void Publish(Type type, E message)
        {
            if (EventsMap.ContainsKey(type))
            {
                if (EventsMap[type] != null)
                {
                    EventsMap[type](this, message);
                }
            }
            else
            {
                foreach (var pair in EventsMapSubclass)
                {
                    if (type.IsSubclassOf(pair.Key))
                    {
                        if (pair.Value != null)
                        {
                            pair.Value(this, message);
                        }
                    }
                }
            }
        }

        protected void Publish(E message)
        {
            Type type = message.GetType();
            Publish(type, message);
        }
    }


    #endregion


    #region - GenericPublishSubscriber
    public interface IGenericPublishSubscriber<T, E>
    where E : BaseDataStructureEventArgs
    {
        void Subscribe(T key, Action<E> eventHanlder);

        void Unsubscribe(T key, Action<E> eventHandler);
    }
    

    public class GenericPublishSubscriber<T, E> : IGenericPublishSubscriber<T, E>
        where E : BaseDataStructureEventArgs
    {
        protected Dictionary<T, Action<E>> EventsMap;

        public GenericPublishSubscriber()
        {
            EventsMap = new Dictionary<T, Action<E>>();
        }

        public void Subscribe(T key, Action<E> eventHandler)
        {

            if (!EventsMap.ContainsKey(key))
                EventsMap[key] = null;

            EventsMap[key] += eventHandler;

        }

        public void Unsubscribe(T key, Action<E> eventHandler)
        {
            if (EventsMap.ContainsKey(key))
            {
                EventsMap[key] -= eventHandler;
            }
        }

        public void Publish(T key, E message)
        {
            if (EventsMap.ContainsKey(key))
            {
                if (EventsMap[key] != null)
                {
                    EventsMap[key](message);
                }
            }
        }
    }
    #endregion
}
