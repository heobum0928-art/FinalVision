using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public static class AppLogger
    {
        static ILogger logger;
        static AppLogger()
        {
            try
            {
                LogManager.InitializeLogger(false);

                logger = LogManager.GetLogger("AppLogger");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        } 
        public static MessageDelegate Warn(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                logger.Warn(messageObject, datetime, memberName, filePath, lineNumber);
            });
        }
        public static MessageDelegate Debug(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                ////logger.Debug(new MessageLog(messageObject, memberName, filePath, lineNumber));
                logger.Debug(messageObject, datetime, memberName, filePath, lineNumber);
            });
        }
        public static MessageDelegate Info(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                ////logger.Debug(new MessageLog(messageObject, memberName, filePath, lineNumber));
                if(logger != null)
                    logger.Info(messageObject, datetime, memberName, filePath, lineNumber);
            });
        }
        public static void Error(Exception ex, string message = "", DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            logger.Error(ex, message, datetime, memberName, filePath, lineNumber);
        }


        public static MessageDelegate Error(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return new MessageDelegate((message, args) =>
            {
                string messageObject = args.Any() ? string.Format(message, args) : message;
                ////logger.Debug(new MessageLog(messageObject, memberName, filePath, lineNumber));
                logger.Error(messageObject, datetime, memberName, filePath, lineNumber);
            });
        }
    }
}
