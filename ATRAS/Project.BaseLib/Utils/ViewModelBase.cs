using Project.BaseLib.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class ViewModelBase : NotifyPropertyChanged
    {
        public ILogger logger;

        public ViewModelBase(string loggerName)             
        {
            try
            {
                LogManager.InitializeLogger(false);
                this.logger = LogManager.GetLogger(loggerName);
                ConfigManager.Instance.CreateConfiguration();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //throw;
            }

        }
    }
}
