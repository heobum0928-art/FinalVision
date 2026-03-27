using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Logger
{
    public class MessageLogRollingAppender : log4net.Appender.CompresssionRollingFileAppender
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var properties = loggingEvent.Properties;

            properties["filePath"] = string.Empty;
            properties["lineNumber"] = string.Empty;
            properties["memberName"] = string.Empty;
            properties["fileName"] = string.Empty;

            var ml = (loggingEvent.MessageObject as MessageLog);

            if (ml != null)
            {
                properties["filePath"] = ml.FilePath;
                properties["lineNumber"] = ml.LineNumber;
                properties["memberName"] = ml.MemberName;
                properties["fileName"] = ml.FileName;

            }

            base.Append(loggingEvent);
        }
    }
}
