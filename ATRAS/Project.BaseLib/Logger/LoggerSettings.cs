using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Logger
{
    public class LoggerSettings : ConfigurationSection
    {
        private static LoggerSettings settings
        = ConfigurationManager.GetSection("loggerSettings") as LoggerSettings;

        public static LoggerSettings Settings { get { return settings; } }

        [ConfigurationProperty("file", IsRequired = false)]
        public string File
        {
            get { return (string)this["file"]; }
            set { this["file"] = value; }
        }
    }
}
