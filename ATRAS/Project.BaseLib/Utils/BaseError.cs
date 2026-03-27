using Project.BaseLib.Communication;
using Project.BaseLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    [DataContract]
    public class BaseError : BaseDataStructure
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string InnerExceptionStackTrace { get; set; }

        [DataMember]
        public string InnerExceptionMessage { get; set; }

        [DataMember]
        public BaseError InnerError { get; set; }

        [DataMember]
        public ErrorSeverity Severity { get; set; }

        [DataMember]
        public string Who { get; set; }

        public BaseError(string message = null, Exception innerException = null)
            : this(ErrorSeverity.Low, message, innerException)
        {

        }
        public BaseError(ErrorSeverity errorSeverity, string message = null, Exception innerException = null)
        {
            Severity = errorSeverity;
            Message = message;
            ExploreException(innerException);            
        }
        public BaseError(BaseError innerError, string message = null, Exception innerException = null)
            : this(innerError, innerError != null ? innerError.Severity : ErrorSeverity.Low, message, innerException)
        {

        }
        public BaseError(BaseError innerError, ErrorSeverity errorSeverity, string message = null, Exception innerException = null)
            : this(message, innerException)
        {
            InnerError = innerError;
            Severity = errorSeverity;
        }

        private void ExploreException(Exception innerException)
        {
            if (innerException == null)
                return;

            Exception ex = innerException;

            while(ex != null)
            {
                InnerExceptionMessage += ex.Message + Environment.NewLine;
                InnerExceptionStackTrace += ex.StackTrace + Environment.NewLine;
                ex = ex.InnerException;
            }
        }

        public T GetInnerError<T>() where T : BaseError
        {
            if (this is T)
                return this as T;

            return InnerError == null ? null : InnerError.GetInnerError<T>();
        }

        public override string ToString()
        {
            string str = string.Format("{0} {1}", GetType().Name, Message);

            if (InnerError != null)
                str += string.Format(" > {0}", InnerError);

            if (InnerExceptionStackTrace != null)
                str += string.Format("\n {0}", InnerExceptionStackTrace);

            return str;
        }
        public virtual string ToErrorString()
        {
            string str = string.Format("{0} {1}", GetType().Name, Message);

            if (InnerError != null)
                str += string.Format(" > {0}", InnerError.ToErrorString());

            if (InnerExceptionMessage != null)
                str += string.Format(", {0}", InnerExceptionMessage);

            return str;
        }
        public virtual string ToTypeErrorString()
        {
            string str = string.Format("{0}", GetType().Name);

            if (InnerError != null)
                str += string.Format(" > {0}", InnerError.ToTypeErrorString());

            if (InnerExceptionMessage != null)
                str += string.Format(", {0}", InnerExceptionMessage);

            return str;
        }

        public virtual string ToMessageString()
        {
            string str = string.IsNullOrEmpty(Message) ? string.Empty : string.Format("{0}", Message + ". ");

            if (InnerError != null)
            {
                string innerStr = InnerError.ToMessageString();
                str += string.IsNullOrEmpty(innerStr) ? string.Empty : string.Format("{0}", innerStr);
            }

            str += string.IsNullOrEmpty(InnerExceptionMessage) ? string.Empty : string.Format("{0}", InnerExceptionMessage + ". ");

            return str;
        }

        public virtual string ToTypeMessageString()
        {
            string str = string.IsNullOrEmpty(Message) ? string.Empty : string.Format("{0}", Message + ". ");

            if (InnerError != null)
            {
                string innerStr = InnerError.ToMessageString();
                str += string.IsNullOrEmpty(innerStr) ? string.Empty : string.Format("{0}", innerStr);
            }

            str += string.Format("[{0}]", GetType().Name);

            return str;
        }

        public override void Clear()
        {
            Message = String.Empty;
            InnerExceptionMessage = String.Empty;
            InnerError.Clear();
        }
    }
}
