using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Project.BaseLib.Communication
{
    public class WCFFactory : Singleton<WCFFactory>
    {
        protected ILogger logger;

        protected bool initialized;
        protected Dictionary<string, ServiceHost> hosts;


        protected WCFFactory()
        {
            logger = LogManager.GetLogger("WCFFactory");

            initialized = false;

            hosts = new Dictionary<string, ServiceHost>();
        }



        private Uri[] GetBaseAddresses(string ipAddress, string port)
        {
            List<Uri> uris = new List<Uri>();
            uris.Add(new Uri("net.pipe://" + ipAddress));
            uris.Add(new Uri("net.tcp://" + ipAddress + ":" + port + "/"));


            foreach (var uri in uris)
            {
                logger.Debug()("Uri : {0}", uri.ToString());
            }

            return uris.ToArray();
        }

        public void Initialize(string instanceName)
        {
            #region -- Service Initialize
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes().
                Where(t => t.GetInterfaces().Where(i => i.Equals(typeof(IService))).Any()));

            var Processes = CommunicationSettings.Instance.Processes;
            string currentProcessName = Process.GetCurrentProcess().ProcessName.Replace(".vshost", string.Empty);
            logger.Debug()("CurrentProcessName : {0}", currentProcessName);
            logger.Debug()("Current Instance Name : {0}", instanceName);

            foreach (var pe in Processes)
            {

                ProcessElement process = (ProcessElement)pe;
                logger.Debug()("Configuration ProcessName : {0}", process.ProcessName);
                if (process.ProcessName.Equals(currentProcessName))
                {
                    logger.Debug()("Configuration process(intance Name) : {0}", process.Name);
                    if (instanceName != null && instanceName.Equals(process.Name))
                    {
                        foreach (var se in process.Services)
                        {
                            ServiceConfigurationElement service = se as ServiceConfigurationElement;

                            Type type = allTypes.FirstOrDefault(x => x.FullName == service.Name);
                            logger.Debug()("Configuration TypeFullName == ServiceName : {0} == {1}", type.FullName, service.Name);

                            if (type == null)
                            {
                                logger.Error("Service can not found, or not loaded : " + "[" + service.Name + "]" + ", Are you missing reference ? ");
                                throw new ApplicationException("Service can not found, or not loaded : " + "[" + service.Name + "]" + ", Are you missing reference ? ");
                            }
                            else
                            {
                                Type contractType = type.GetInterface(service.Contract);

                                if (contractType == null)
                                {
                                    logger.Error("Contract can not found, or not loaded : " + "[" + type + "]" + "[" + service.Name + "]" + ", Are you missing reference ? ");
                                    throw new ApplicationException("Contract can not found, or not loaded : " + "[" + type + "]" + "[" + service.Name + "]" + ", Are you missing reference ? ");
                                }
                                else
                                {
                                    logger.Debug()("Configuration contractType : {0}", contractType.FullName);
                                    ServiceHost host = new ServiceHost(type, GetBaseAddresses(process.IpAddress, service.Port));
                                    hosts.Add(type.FullName, host);

                                    Binding binding;
                                    Configuration commConfig = CommunicationSettings.Instance.Configuration;
                                    ServiceModelSectionGroup serviceModel = ServiceModelSectionGroup.GetSectionGroup(commConfig);

                                    if (serviceModel.Bindings.NetTcpBinding.Bindings.ContainsKey(service.Binding))
                                    {
                                        logger.Debug()("TCP Service Found... "
                                        + "net.tcp://"
                                        + process.IpAddress + ":"
                                        + service.Port + "/"
                                        + service.Name + ", "
                                        + "[" + service.Binding + ", "
                                        + contractType.Name + "] ");

                                        StandardBindingElement bindingElement = serviceModel.Bindings.NetTcpBinding.Bindings[service.Binding];

                                        binding = new NetTcpBinding();
                                        bindingElement.ApplyConfiguration(binding);
                                        host.AddServiceEndpoint(contractType, binding, service.Name);
                                    }
                                    else if (serviceModel.Bindings.NetNamedPipeBinding.Bindings.ContainsKey(service.Binding))
                                    {
                                        logger.Debug()("NamedPipe Service Found... "
                                        + "net.pipe://"
                                        + process.IpAddress + "/"
                                        + service.Name
                                        + " [" + service.Binding + ", "
                                        + contractType.Name + "]");

                                        StandardBindingElement bindingElement = serviceModel.Bindings.NetNamedPipeBinding.Bindings[service.Binding];

                                        binding = new NetNamedPipeBinding();
                                        bindingElement.ApplyConfiguration(binding);
                                        host.AddServiceEndpoint(contractType, binding, service.Name);
                                    }
                                    logger.Debug()("Opening Service : " + service.Name);
                                    ServiceDebugBehavior sdb = host.Description.Behaviors[typeof(ServiceDebugBehavior)] as ServiceDebugBehavior;

                                    host.Description.Behaviors.Add(new ServiceErrorHandler());

                                    if (sdb != null)
                                        sdb.IncludeExceptionDetailInFaults = true;

                                    host.AddGenericResolver();

                                    host.Open();

                                    logger.Debug()("Service Opened Successfully... "
                                      + process.IpAddress + "://"
                                      + service.Name + ":"
                                      + service.Port + ", "
                                      + "[" + service.Binding + ","
                                      + contractType.Name + "]");
                                }
                            }
                        }
                    }
                }

            }
            #endregion

            #region -- Client Initailize

            #endregion
        }

        private readonly Hashtable proxyCache = new Hashtable();

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
