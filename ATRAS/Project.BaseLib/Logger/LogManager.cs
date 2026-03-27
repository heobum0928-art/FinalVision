using System;
using System.Linq;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using System.Reflection;
using log4net.Util;
using System.Globalization;

namespace Project.BaseLib.Logger
{
    public static class LogManager
    {
        private static bool previewErrors = true;
        public static bool PreviewErrors { get { return previewErrors; } }

        private static bool initialized = false;
        public static bool Initialized { get { return initialized; } }

        public static EventHandler<ProjectLoggingEvent> LogEvent;

        public static EventHandler<object> AfterLogAction;


 
        //DBUpgradeProxy calls this method - If method parameters are changed need to try-catch in  DBUpgradeProxy
        public static void InitializeLogger(string configFileName = null, bool b = false, string instanceName = null, string logsDirectory = null, bool previewErrors = true)
        {
            if (initialized) return;

            LogManager.previewErrors = previewErrors;
            log4net.GlobalContext.Properties["logsDirectory"] = logsDirectory != null ? logsDirectory : "logsDirectory";
            log4net.GlobalContext.Properties["instanceName"] = instanceName != null ? instanceName : "instanceName";

            if(b)
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(log4net.LogManager.GetRepository(), new System.IO.FileInfo(configFileName));
            }
            else if(configFileName != null)
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(configFileName));
            else
                log4net.Config.XmlConfigurator.Configure();

            if (!log4net.LogManager.GetRepository().Configured)
                throw new Exception("Log4Net Configuration Failed...");
            
            foreach(ILog log in log4net.LogManager.GetCurrentLoggers())
            {
                foreach(IAppender appender in log.Logger.Repository.GetAppenders())
                {
                    DelegateAppender delegateAppender = appender as DelegateAppender;
                    if (delegateAppender != null)
                    {
                        delegateAppender.LogMethod = LogMethod;
                    }
                }
            }

            initialized = true;
        }

        public static void InitializeLogger(bool previewErrors = true)
        {

            if (initialized) return;
            LogManager.previewErrors = previewErrors;

            log4net.Config.XmlConfigurator.Configure();

            if (!log4net.LogManager.GetRepository().Configured)
            {
                throw new Exception("Log4Net Configuration Failed...");
            }

            foreach (ILog log in log4net.LogManager.GetCurrentLoggers())
            {
                foreach (IAppender appender in log.Logger.Repository.GetAppenders())
                {
                    DelegateAppender delegateAppender = appender as DelegateAppender;
                    if (delegateAppender != null)
                    {
                        delegateAppender.LogMethod = LogMethod;
                    }
                }
            }

            initialized = true;
        }

        public static void LogMethod(ProjectLoggingEvent projectLoggingEvent)
        {
            if (LogEvent != null)
            {
                LogEvent(null, projectLoggingEvent);
            }
        }

        public static ILogger GetLogger(string loggerName)
        {
            if(log4net.LogManager.GetRepository().Configured)
            {
                // if configuration file says log4net...
                return new Log4NetWrapper(loggerName);
            }

            return null;

            //throw new Exception("Logger was not initialized. Call LogManager.InitializeLogger() first."); 
        }

        public static log4net.Appender.IAppender GetAppender(string appenderName)
        {
            var appenders = log4net.LogManager.GetRepository().GetAppenders();
            var appender = appenders.First(a => a.Name == appenderName);

            return appender;

        }

        public static log4net.Appender.IAppender[] GetAllAppenders()
        {
            return log4net.LogManager.GetRepository().GetAppenders();
        }

    }
}
