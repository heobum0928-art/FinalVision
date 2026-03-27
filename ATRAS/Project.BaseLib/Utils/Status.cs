using Project.BaseLib.Communication;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Project.BaseLib.Utils
{
    [DataContract]
    [SerializableAttribute]
    public class OperationStatus : BaseDataStructure
    {
        private BaseError errorInfo;

        [DataMember]
        [XmlAttribute]
        public ResultStatus Status { get; private set; }

        [DataMember]
        [XmlElement]
        public BaseError ErrorInfo
        {
            get { return errorInfo; }

            set
            {
                Status = value == null ? ResultStatus.Success : ResultStatus.Error;
                errorInfo = value;
            }
        }

        public OperationStatus(ResultStatus status = ResultStatus.Success)
        {
            this.Status = status;
        }

        public OperationStatus(BaseError errorInfo)
        {
            Status = errorInfo == null ? ResultStatus.Success : ResultStatus.Error;
            this.errorInfo = errorInfo;
        }

        public override string ToString()
        {
            string str = Status == ResultStatus.Success ? "Success" : "Error : ";

            if (errorInfo != null)
                str += " : " + errorInfo.ToString();

            return str;
        }

        public static OperationStatus Success { get { return new OperationStatus(); } }
        public static OperationStatus Error { get { return new OperationStatus(ResultStatus.Error); } }

        public bool IsSuccess { get { return Status == ResultStatus.Success; } }
        public bool IsError { get { return Status == ResultStatus.Error; } }

        public override void Clear()
        {
            errorInfo.Clear();
            Status = ResultStatus.Success;
        }
    }

    [DataContract]
    [SerializableAttribute]
    public class OperationStatus<T> : OperationStatus
    {
        [DataMember]
        [XmlElement]
        public virtual T Result { get; set; }

        public OperationStatus(T result, OperationStatus opStatus)
            : base((opStatus != null && opStatus.IsError)? ResultStatus.Error : opStatus.Status)
        {
            this.Result = result;
            this.ErrorInfo = (opStatus != null ? opStatus.ErrorInfo : null);
        }

        public OperationStatus(T result, ResultStatus status = ResultStatus.Success)
        {
            this.Result = result;
        }

        public OperationStatus(T result, BaseError errorInfo)
            : base(errorInfo)
        {
            this.Result = result;
        }

        public OperationStatus(BaseError errorInfo)
            : base(errorInfo)
        {
            this.Result = default(T);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), string.Format("Result: {0}", Result != null ? Result.ToString() : "Null"));
        }

        public new static OperationStatus<T> Success { get { return new OperationStatus<T>(default(T)); } }
        public new static OperationStatus<T> Error { get { return new OperationStatus<T>(default(T), ResultStatus.Error); } }

    }
}
