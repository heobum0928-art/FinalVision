using log4net.Appender;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using log4net.Core;

namespace Project.BaseLib.Logger
{
    public delegate void LogAppend(ProjectLoggingEvent loggingEvent);

    public class DelegateAppender : AppenderSkeleton
    {
        private LogAppend logAppend;

        public LogAppend LogMethod
        {
            get { return logAppend; }
            set { logAppend = value; }
        }

        public DelegateAppender()
        {
            logAppend = LogManager.LogMethod;
            //logAppend = EmptyAppend;
        }

        private void EmptyAppend(ProjectLoggingEvent projectLoggingEvent)
        {
            // Do nothing
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (logAppend != null)
            {
                var ml = (loggingEvent.MessageObject as MessageLog);
            
                string memberName = "";
                string filePath = "";
                string fileName = "";
                int lineNumber = 0;

                if (ml != null)
                {
                    memberName = ml.MemberName;
                    filePath = ml.FilePath;
                    lineNumber = ml.LineNumber;
                    fileName = ml.FileName;

                    logAppend(new ProjectLoggingEvent(loggingEvent.TimeStamp,
                        loggingEvent.Level.ToString(),
                        loggingEvent.LoggerName,
                        loggingEvent.MessageObject.ToString(),
                        loggingEvent.ThreadName,
                        loggingEvent.ExceptionObject,
                        memberName,
                        filePath,
                        fileName,
                        lineNumber));
                }
            }
        }
    }
}
