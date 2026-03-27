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
        void OnNotifyMessage(BaseDataStructure message);
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

    public interface IServiceClient<out T> where T : IService
    {
        string Name { get; }
    }

    public interface INotifier<MESSAGE>
        where MESSAGE : BaseDataStructure
    {
        void NotifyMessage(MESSAGE message);
    }

    public interface IClient
    {
        bool IsOpen { get; }

        void Open();

        void Close();
    }
}
