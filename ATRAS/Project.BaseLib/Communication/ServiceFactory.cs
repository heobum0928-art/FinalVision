using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Project.BaseLib.Communication;

namespace Project.BaseLib.Communication
{
    public class ServiceFactory : Singleton<ServiceFactory>
    {
        protected ILogger logger;
        protected bool initialized;

        protected Dictionary<string, ServiceHost> hosts;

        protected ServiceFactory()
        {
            logger = LogManager.GetLogger("ServiceFactory");

            initialized = false;

            hosts = new Dictionary<string, ServiceHost>();
        }

        private Uri[] GetBaseAddresses(string ipAddress, string port)
        {
            List<Uri> uris = new List<Uri>();
            uris.Add(new Uri("net.pipe://" + ipAddress));
            uris.Add(new Uri("net.tcp://" + ipAddress + ":" + port + "/"));


            foreach(var uri in uris)
            {
                logger.Debug()("Uri : {0}", uri.ToString());
            }

            return uris.ToArray();
        }

        public void Initialize(string instanceName)
        {
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

                            if(type == null)
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
        }
        
        public void Initialize()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x=> x.GetTypes());

            var Processes = CommunicationSettings.Instance.Processes;
            string currentProcessName = Process.GetCurrentProcess().ProcessName.Replace(".vshost", string.Empty);
            logger.Debug()("CurrentProcessName : {0}", currentProcessName);

            foreach (var pe in Processes)
            {
                ProcessElement process = (ProcessElement)pe;
                logger.Debug()("process.ProcessName : {0}", process.ProcessName);
                logger.Debug()("process.Name(instanceName) : {0}", process.Name);

                if (process.ProcessName.Equals(currentProcessName))
                {

                }
            }
        }
    }
}
