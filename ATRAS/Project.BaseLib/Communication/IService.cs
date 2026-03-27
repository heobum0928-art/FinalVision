using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    [ServiceContract]
    public interface INotifierService
    {
        [OperationContract(IsOneWay = true)]
        void OnNotifyMessage(object message);
    }

    [ServiceContract(SessionMode = SessionMode.Allowed,
    CallbackContract = typeof(INotifierService))]
    public interface IService
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        void IsAlive();
    }
}
