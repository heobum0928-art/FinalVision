using Project.BaseLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    public class ClientsFactory : Singleton<ClientsFactory>
    {
        private readonly Hashtable proxyCache = new Hashtable();

        protected ClientsFactory()
        {

        }

        public void Close(string processName)
        {
            if (!proxyCache.ContainsKey(processName))
                throw new Exception("Process Name " + processName + " Does not exist: ");

            var processServices = (Hashtable)proxyCache[processName];

            var keys = ((Hashtable)proxyCache[processName]).Keys;

            foreach (var k in keys)
                ((IClient)processServices[k]).Close();
        }

        public void Open(string processName)
        {
            if (!proxyCache.ContainsKey(processName))
                throw new Exception("Process Name " + processName + " Does not exist: ");

            var processServices = (Hashtable)proxyCache[processName];

            var keys = ((Hashtable)proxyCache[processName]).Keys;

            foreach (var k in keys)
                ((IClient)processServices[k]).Open();


        }


        public ServiceClient<T> GetClient<T>(
            string processName,
            Action<object, CommunicationEventArgs> connectedEventHandler = null,
            Action<object, CommunicationEventArgs> disconnectedEventHandler = null)
            where T : IService
        {
            lock (proxyCache)
            {
                if (!proxyCache.ContainsKey(processName))
                {
                    try
                    {
                        var services = new Hashtable();
                        proxyCache.Add(processName, services);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to create client: " + ex.Message, ex);
                    }
                }

                try
                {
                    var services = (Hashtable)proxyCache[processName];

                    if (!services.ContainsKey(typeof(T).ToString()))
                    {
                        var newClient = CreateClient<T>(processName, connectedEventHandler, disconnectedEventHandler);
                        services.Add(typeof(T).ToString(), newClient);

                        return newClient;
                    }

                    var client = (ServiceClient<T>)((Hashtable)proxyCache[processName])[typeof(T).ToString()];

                    if (!(client is ServiceClient<T>))
                        throw new ApplicationException("Wrong processName or Interface requested : " + "process=" + processName + ",interface=" + typeof(T).ToString());

                    if (connectedEventHandler != null)
                        client.OnClientConnected += connectedEventHandler;

                    if (disconnectedEventHandler != null)
                        client.OnClientDisconnected += disconnectedEventHandler;

                    return client;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to Find provider: " + ex.Message, ex);
                }
            }
        }

        public ServiceClient<T> CreateClient<T>(string processName,
            Action<object, CommunicationEventArgs> connectedEventHandler,
            Action<object, CommunicationEventArgs> disconnectedEventHandler,
            object syncObject = null)
            where T : IService
        {
            var processes = CommunicationSettings.Instance.Processes;
            if (!processes.ContainsKey(processName))
                throw new ApplicationException("Process name " + processName + " does not exist in communication.config file");

            string serviceType = typeof(T).ToString();

            foreach (var se in processes[processName].Services)
            {
                ServiceConfigurationElement service = se as ServiceConfigurationElement;

                if (service.Contract.Equals(serviceType))
                {
                    return new ServiceClient<T>(processName, processes[processName].IpAddress,
                        service.Port, service.Name, service.Binding, false,
                        connectedEventHandler,
                        disconnectedEventHandler,
                        syncObject);
                }
            }

            throw new ApplicationException("Service " + serviceType + " could not be found");
        }
    }
}
