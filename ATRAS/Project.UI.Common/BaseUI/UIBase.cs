using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Project.UI.Common.BaseUI
{
    public class UIBase : Window
    {
        protected ILogger logger;
        public UIBase(string loggerName)
        {
            LogManager.InitializeLogger(false);
            logger = LogManager.GetLogger(loggerName);
        }

    }
}
