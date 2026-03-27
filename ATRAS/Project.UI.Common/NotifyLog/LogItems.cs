using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.UI.Common
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public class LogItems : NotifyPropertyChanged
    {
        public DateTime TimeStamp { get; set; }

        private string loggerName;
        public string LoggerName
        {
            get { return loggerName; }
            set
            {
                loggerName = value;
                OnPropertyChanged();
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged();
            }
        }

        private LogLevel logLevel;
        public LogLevel LogLevel
        {
            get { return logLevel; }
            set
            {
                logLevel = value;
                OnPropertyChanged();
            }
        }

        public string LogString
        {
            get{ return ToString();}
        }
        public LogItems(string msg)
        {
            this.TimeStamp = DateTime.Now;
            this.LoggerName = null;
            this.Message = msg;
            this.LogLevel = LogLevel.Info;
        }

        public LogItems(string msg, string loggerName, LogLevel level)
        {
            this.TimeStamp = DateTime.Now;
            this.LoggerName = loggerName;
            this.Message = msg;
            this.LogLevel = level;
        }
        public override string ToString()
        {
            if (loggerName != null)
            {
                return string.Format("{0} {1,-30} {2}", 
                                        TimeStamp.ToString("T",new CultureInfo("en-US")),
                                        LoggerName,
                                        Message);
            }
            else
            {
                return string.Format("{0} {1}", 
                                        TimeStamp.ToString("T", new CultureInfo("en-US")), 
                                        Message);
            }
        }
    }
}
