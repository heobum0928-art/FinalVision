using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    [CallbackBehavior(UseSynchronizationContext = false,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ServiceClient<T> :
        PublishSubscriber<BaseDataStructure>, IServiceClient<T>, INotifierService, IClient, IDisposable
        where T : IService
    {
        protected ILogger Logger { get; private set; }

        private T service;
        public bool KeepAlive { get; private set; }

        public bool PingEnabled { get; set; }

        protected bool isListening = false;

        private Object syncObject;
        private int delayTime = 5000;
        private Object delayObject = new Object();
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private string netTcp = "net.tcp://";
        private string namedPipe = "net.pipe://";
        public string ipAddress { get; private set; }

        public string Name { get; private set; }

        public override string ToString()
        {
            return Name;
        }

        public event Action<object, CommunicationEventArgs> OnClientConnected;
        public event Action<object, CommunicationEventArgs> OnClientDisconnected;

        DuplexChannelFactory<T> channelFactory;

        public T Service { get { return service; } }

        public CommunicationState State
        {
            get
            {
                var channel = service as IClientChannel;

                if (channel == null)
                    return CommunicationState.Closed;

                return channel.State;
            }
        }

        public void Connect()
        {
            if (isListening) return;

            var mre = new ManualResetEvent(false);

            OnClientConnected += (s, e) => { mre.Set(); };

            Open();

            mre.WaitOne();
        }

        protected ServiceClient(string name, bool keepAlive, Object syncObject = null)
        {
            Name = name;

            Logger = LogManager.GetLogger("ServiceClient");

            PingEnabled = true;

            this.syncObject = syncObject;

            if (this.syncObject == null)
                this.syncObject = new Object();
        }

        protected ServiceClient(string name, string ipAddress, string port, string serviceName, string bindingName, bool keepAlive, object syncObject = null)
            : this(name, keepAlive, syncObject)
        {
            try
            {
                Logger.Debug()("Service Client is starting for service... " + serviceName + "," + ipAddress + ",");

                Binding binding = null;
                EndpointAddress endPointAddress = null;

                this.KeepAlive = keepAlive;
                this.ipAddress = ipAddress;

                Configuration commConfig = CommunicationSettings.Instance.Configuration;
                ServiceModelSectionGroup serviceModel = ServiceModelSectionGroup.GetSectionGroup(commConfig);

                if (serviceModel.Bindings.NetTcpBinding.Bindings.ContainsKey(bindingName))
                {
                    StandardBindingElement bindingElement = serviceModel.Bindings.NetTcpBinding.Bindings[bindingName];

                    binding = new NetTcpBinding();

                    Logger.Debug()("Connecting to TCP service... "
                        + netTcp + ipAddress + ":" + port + "/" + serviceName + ",binding=" + bindingName + " MaxConnections " + ((NetTcpBinding)binding).MaxConnections);

                    bindingElement.ApplyConfiguration(binding);

                    Logger.Debug()("MaxConnections " + ((NetTcpBinding)binding).MaxConnections);

                    endPointAddress = new EndpointAddress(netTcp + ipAddress + ":" + port + "/" + serviceName);
                }
                else if (serviceModel.Bindings.NetNamedPipeBinding.Bindings.ContainsKey(bindingName))
                {
                    StandardBindingElement bindingElement = serviceModel.Bindings.NetNamedPipeBinding.Bindings[bindingName];
                    Logger.Debug()("Connecting to NamedPipe service... "
                        + namedPipe + ipAddress + "/" + serviceName + ",binding=" + bindingName);

                    binding = new NetNamedPipeBinding();
                    bindingElement.ApplyConfiguration(binding);
                    endPointAddress = new EndpointAddress(namedPipe + ipAddress + "/" + serviceName);
                }

                Logger.Debug()("Initializing the Channel Factory... ");

                channelFactory = new DuplexChannelFactory<T>(this, binding, endPointAddress);

                channelFactory.AddGenericResolver();

                channelFactory.Opened += OnChannelOpened;
                channelFactory.Faulted += OnChannelFaulted;
                channelFactory.Closed += OnChannelClosed;

            }
            catch (Exception e)
            {
                Logger.Error()("Service Client failed to connect to service... "
                    + serviceName + "," + ipAddress + "," + port + "," + bindingName);

                Logger.Error(e, e.Message);

            }
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            Logger.Debug(Name + "." + typeof(T).Name + " Client Channel Closed ! ");
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            Logger.Debug(Name + "." + typeof(T).Name + " Client Channel Faulted !! ");
        }

        private void OnChannelOpened(object sender, EventArgs e)
        {
            Logger.Debug(Name + "." + typeof(T).Name + " channel is waiting for service to start...");
        }

        public ServiceClient(string name, string ipAddress, string port, string serviceName, string bindingName, bool keepAlive,
            Action<object, CommunicationEventArgs> connectedEventHandler = null,
            Action<object, CommunicationEventArgs> disconnectedEventHandler = null,
            object syncObject = null)
            : this(name, ipAddress, port, serviceName, bindingName, keepAlive, syncObject)
        {

            if (connectedEventHandler != null)
                OnClientConnected += connectedEventHandler;

            if (disconnectedEventHandler != null)
                OnClientDisconnected += disconnectedEventHandler;

            Logger.Debug()("Client created successfully : " + serviceName + "," + ipAddress + "," + port + "," + bindingName);

        }

        private void SetChannel()
        {
            bool endPointNotFound = true;

            Task.Run(() =>
            {
                while (endPointNotFound)
                {
                    lock (syncObject)
                    {
                        if (!isListening) break;

                        try
                        {
                            service = channelFactory.CreateChannel();
                            var innerChannel = (ICommunicationObject)service;
                            innerChannel.Opened += OnServiceOpened;
                            innerChannel.Closed += OnServiceClosed;

                            if (service == null)
                                throw new Exception("Failed to create channel,...");

                            IClientChannel c = service as IClientChannel;
                            if (OpenChannel(c))
                            {
                                Logger.Info()("OpenChannel(c) Sucessfuly.");
                                service.Subscribe();
                                endPointNotFound = false;
                                IsOpen = true;

                                if (OnClientConnected != null)
                                {
                                    Task.Run(() =>
                                    {
                                        try
                                        {
                                            Logger.Debug()("OnClientConnected " + "Name=" + Name + "." + typeof(T).Name);
                                            OnClientConnected(this, new CommunicationEventArgs(Name, true, typeof(T), ipAddress));
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.Error(e, "Service Opened, OnClientConnected Error... " + e.Message + ", Name=" + Name + "." + typeof(T).Name);
                                        }

                                        return;
                                    });
                                }
                            }
                        }
                        catch (FaultException)
                        {
                        }
                        catch (CommunicationObjectFaultedException)
                        {
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                        }
                        catch (CommunicationException)
                        {
                        }
                        catch (Exception e)
                        {
                            throw (e);
                        }
                    }
                }
            });
        }

        [DebuggerNonUserCode]
        private bool OpenChannel(IClientChannel c)
        {
            try
            {
                c.Open();
                Logger.Info()("IClientChannel.OPen()");
                return true;
            }
            catch (EndpointNotFoundException)
            {
                autoResetEvent.WaitOne(delayTime);
                return false;
            }
        }

        public void Open()
        {
            if (isListening)
                return;

            isListening = true;

            SetChannel();
        }

        private void OnServiceClosed(object sender, EventArgs e)
        {
            lock (syncObject)
            {
                if (OnClientDisconnected != null)
                {
                    Task.Run(() =>
                    {
                        OnClientDisconnected(this, new CommunicationEventArgs(Name, false, typeof(T), ipAddress));
                    });
                }

                IsOpen = false;

                Logger.Debug()("Disconnected from Service... " + Name + "." + typeof(T).Name);

                if (PingEnabled)
                {
                    Logger.Debug()("Waiting for service to start... " + Name + "." + typeof(T).Name);
                    SetChannel();
                }
            }
        }

        private void OnServiceOpened(object sender, EventArgs e)
        {
            lock (syncObject)
            {
                var innerChannel = (ICommunicationObject)service;

                Logger.Debug()("Connected to Service... " + Name + "." + typeof(T).Name);

                innerChannel.Faulted += OnServiceFaulted;
            }
        }

        private void OnServiceFaulted(object sender, EventArgs e)
        {
            Logger.Debug()("Service is Faulted... " + Name + "." + typeof(T).Name);

            var channelObject = (ICommunicationObject)service;

            if (channelObject != null)
                channelObject.Abort();
        }

        public void OnNotifyMessage(BaseDataStructure e)
        {
            try
            {
                if (e != null)
                {
                    Task.Run(() =>
                    {
                        Publish(e.GetType(), e);
                    });
                }
                else
                {
                    throw new ArgumentNullException("OnNotifyMessage failed with e. e argument is null");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnNotifyMessage critical error, " + ex.Message);
                throw ex;
            }
        }

        public void Close()
        {
            if (!isListening)
                return;

            isListening = false;

            autoResetEvent.Set();
        }

        public void Dispose()
        {
        }

        public bool IsOpen
        {
            get;
            private set;
        }
    }



}
