using Project.BaseLib.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    [AspNetCompatibilityRequirements(
  RequirementsMode =
    AspNetCompatibilityRequirementsMode.Allowed)]
    public abstract class NotificationsSubscriber<CLASS, MESSAGE, NOTIFIER> : NotifySingleton<CLASS>, IService, INotifier<MESSAGE>
    where NOTIFIER : class, INotifierService
    where MESSAGE : BaseDataStructure
    where CLASS : NotificationsSubscriber<CLASS, MESSAGE, NOTIFIER>
    {

        #region Logger

        #endregion

        protected List<NOTIFIER> subscribersList;

        protected NotificationsSubscriber()
        {
            subscribersList = new List<NOTIFIER>();
        }

        private void Add(NOTIFIER subscriber)
        {
            if (subscribersList.Contains(subscriber))
                return;

            var channelObject = (ICommunicationObject)subscriber;

            channelObject.Faulted += OnClientConnectionFaulted;
            channelObject.Closed += OnClientConnectionClosed;

            subscribersList.Add(subscriber);
        }

        private void OnClientConnectionFaulted(object sender, EventArgs e)
        {
            lock (this)
            {
                var channelObject = (ICommunicationObject)sender;

                channelObject.Abort();
            }
        }

        private void OnClientConnectionClosed(object sender, EventArgs e)
        {
            lock (this)
            {
                Remove(sender as NOTIFIER);
            }
        }

        private void Remove(NOTIFIER subscriber)
        {
            if (!subscribersList.Contains(subscriber))
                return;

            subscribersList.Remove(subscriber);
        }

        public virtual void Subscribe()
        {
            lock (this)
            {
                NOTIFIER subscriber = OperationContext.Current.GetCallbackChannel<NOTIFIER>();

                Add(subscriber);
            }
        }

        public void Unsubscribe()
        {
            lock (this)
            {
                NOTIFIER subscriber = OperationContext.Current.GetCallbackChannel<NOTIFIER>();

                Remove(subscriber);
            }
        }

        private void Publish(MESSAGE message)
        {
            lock (this)
            {
                subscribersList.ForEach(delegate (NOTIFIER subscriber)
                {
                    subscriber.OnNotifyMessage(message);
                });
            }
        }

        public void NotifyMessage(MESSAGE message)
        {
            if (message == null)
            {
                throw new ApplicationException("notification Message is null");
            }
            Publish(message);
        }


        public void IsAlive()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CLASS"></typeparam>
    /// <typeparam name="MESSAGE"></typeparam>
    public abstract class NotificationsSubscriber<CLASS, MESSAGE> :
        NotificationsSubscriber<CLASS, MESSAGE, INotifierService>
        where MESSAGE : BaseDataStructure
        where CLASS : NotificationsSubscriber<CLASS, MESSAGE>
    { }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="CLASS"></typeparam>
    public abstract class NotificationsSubscriber<CLASS> :
      NotificationsSubscriber<CLASS, BaseDataStructure, INotifierService>
        where CLASS : NotificationsSubscriber<CLASS>
    { }
}
