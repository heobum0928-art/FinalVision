using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    public class ServiceErrorHandler : ServiceErrorHandlerBehaviorExtensionElement, IErrorHandler, IServiceBehavior
    {
        #region IErrorHandler Members

        public bool HandleError(Exception error)
        {
            return true;
        }

        public void ProvideFault(Exception error, System.ServiceModel.Channels.MessageVersion version, ref System.ServiceModel.Channels.Message fault)
        {
            if (!(error is FaultException))
            {
                Exception err = error;
                string message = "";

                while (err != null)
                {
                    message += err.Message + ";";
                    err = err.InnerException;
                }

                FaultException faultException = new FaultException(message);
                MessageFault messageFault = faultException.CreateMessageFault();
                fault = Message.CreateMessage(version, messageFault, faultException.Action);
            }
        }

        #endregion

        #region IServiceBehavior Members

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channDisp in serviceHostBase.ChannelDispatchers)
            {
                channDisp.ErrorHandlers.Add(this);
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        #endregion
    }

    public class ServiceErrorHandlerBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(ServiceErrorHandler); }
        }

        protected override object CreateBehavior()
        {
            return new ServiceErrorHandler();
        }

    }
}