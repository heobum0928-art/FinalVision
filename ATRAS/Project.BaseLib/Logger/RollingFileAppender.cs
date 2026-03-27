using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Logger
{
    public class RollingFileAppender : log4net.Appender.CompresssionRollingFileAppender
    {
        //Write the header only once per file, not every time the application starts
        protected override void WriteHeader()
        {
            if (LockingModel.AcquireLock().Length == 0)
            {
                base.WriteHeader();
            }
        }
    }
}
