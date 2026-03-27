using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Logger
{
    public class ProjectLoggingEvent
    {
        public ProjectLoggingEvent(DateTime timeStamp, string level, string loggerName, 
          string message, string threadName, Exception exceptionObject, 
          string memberName, string filePath,  string fileName, int lineNumber)
        {
            TimeStamp = timeStamp;
            Level = level;
            LoggerName = loggerName;
            Message = message;
            ThreadName = threadName;
            Exception = exceptionObject;
            FilePath = filePath;
            FileName = fileName;
            LineNumber = lineNumber;
            MemberName = memberName;
        }

        public DateTime TimeStamp { get; set; }

        public string Level { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public string MemberName { get; set; }

        public string FilePath { get; set; }
 
        public string FileName { get; set; }
    
        public int LineNumber { get; set; }

        public string ThreadName { get; set; }
        
        public Exception Exception { get; set; }
    }
}
