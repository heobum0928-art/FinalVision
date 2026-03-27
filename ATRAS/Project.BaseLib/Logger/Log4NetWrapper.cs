using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using log4net;

namespace Project.BaseLib.Logger
{
    public delegate void MessageDelegate(string message, params object[] args);
    public delegate void MessageExDelegate(Exception ex, string message, params object[] args);

    public class MessageLog
    {
        public MessageLog(object messageObject, string memberName, string filePath, int lineNumber)
        {
            MessageObject = messageObject;
            FilePath = filePath;
            LineNumber = lineNumber;
            MemberName = memberName;
            FileName = Path.GetFileName(filePath);
        }

        public object MessageObject { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string MemberName { get; set; }
        public int LineNumber { get; set; }

        public override string ToString()
        {
            return MessageObject.ToString();
        }
    }
   
    public class Log4NetWrapper : ILogger
    {
        protected readonly log4net.ILog logger;

        public Log4NetWrapper(string loggerName)
        {
            logger = log4net.LogManager.GetLogger(loggerName);
        }

        public bool IsDebugEnabled {get { return logger.IsDebugEnabled; } }
        public bool IsInfoEnabled { get { return logger.IsInfoEnabled; } }
        public bool IsWarnEnabled { get { return logger.IsWarnEnabled; } }
        public bool IsErrorEnabled { get { return logger.IsErrorEnabled; } }

    
        public virtual MessageDelegate Info(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
                {
                    string messageObject = args.Any() ? string.Format(message, args) : message;
                    logger.Info(new MessageLog(messageObject, memberName, filePath, lineNumber));
                });
        }

        public virtual MessageDelegate Debug(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                logger.Debug(new MessageLog(messageObject, memberName, filePath, lineNumber));
            });
        }

        public virtual MessageDelegate Warn(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                logger.Warn(new MessageLog(messageObject, memberName, filePath, lineNumber));
            });
        }

        public virtual MessageDelegate Error(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                logger.Error(new MessageLog(messageObject, memberName, filePath, lineNumber));
            });
        }

        public ILogger CopyLogger(string newName)
        {
            Log4NetWrapper newLog = new Log4NetWrapper(newName);

            (newLog.logger.Logger as log4net.Repository.Hierarchy.Logger).Level = (logger.Logger as log4net.Repository.Hierarchy.Logger).Level;

            foreach (var appender in (logger.Logger as log4net.Repository.Hierarchy.Logger).Appenders)
            {
                (newLog.logger.Logger as log4net.Repository.Hierarchy.Logger).AddAppender(appender);
            }

            return newLog;
        }

        public bool AddAppender(string appenderName)
        {
            var appenders = log4net.LogManager.GetRepository().GetAppenders();
            var appender = appenders.First(a => a.Name == appenderName);
            var log = logger.Logger as log4net.Repository.Hierarchy.Logger;

            if (appender != null)
                log.AddAppender(appender);
            else
                return false;

            return true;
        }

 
        public virtual void Debug(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            logger.Debug(new MessageLog(messageObject, memberName, filePath, lineNumber));
        }

        public virtual void Info(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            logger.Info(new MessageLog(messageObject, memberName, filePath, lineNumber));
        }

        public virtual void Warn(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            logger.Warn(new MessageLog(messageObject, memberName, filePath, lineNumber));
        }
 
        public virtual void Error(Exception ex, string message = "", DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            logger.Error(new MessageLog(message, memberName, filePath, lineNumber), ex);
        }

        public virtual void Error(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            logger.Error(new MessageLog(messageObject, memberName, filePath, lineNumber));
        }
    }
}
