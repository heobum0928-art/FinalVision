using Project.BaseLib.Extension;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Communication
{
    public class CommunicationSettings : Singleton<CommunicationSettings>
    {
        protected ILogger logger;
        protected string fileName = "configuration\\communication.config";

        public Configuration Configuration { get; private set; }
        private ProcessesCollection processes;
        private ComputersCollection computers;

        public bool LocalMode { get; private set; }

        public ProcessesCollection Processes
        {
            get
            {
                return processes;
            }
            set
            {
                processes = value;
            }
        }

        public ComputersCollection Computers
        {
            get
            {
                return computers;
            }
            set
            {
                computers = value;
            }
        }

        protected CommunicationSettings()
        {
            logger = LogManager.GetLogger("CommunicationSettings");

            var executingAssembly = Assembly.GetExecutingAssembly();

            Configuration =
                ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                {
                    ExeConfigFilename = Path.GetDirectoryName(executingAssembly.Location) + @"\" + fileName
                    
                }, ConfigurationUserLevel.None);

            string path = Path.GetDirectoryName(executingAssembly.Location) + @"\" + fileName;
            logger.Info()("Path : {0}", Path.GetDirectoryName(executingAssembly.Location) + @"\" + fileName);
            ProcessesSection processesSection =
                (ProcessesSection)Configuration.GetSection("processesSection");

            ComputersSection computersSection =
                (ComputersSection)Configuration.GetSection("computersSection");

            processes = processesSection.Processes;
            computers = computersSection.Computers;

            LocalMode = !NetworkExtensions.IsLocalAddress(processes.Cast<ProcessElement>().Select(p =>
            {
                IPAddress address;
                IPAddress.TryParse(p.IpAddress, out address);
                return address;
            }));
            
            if (LocalMode)
            {
                foreach (ProcessElement process in processes)
                {
                    bool runInLocalMode;
                    Boolean.TryParse(process.RunInLocalMode, out runInLocalMode);
                    if (runInLocalMode)
                        process.IpAddress = "localhost";
                }
            }
            logger.Info()("Communication LocalMode : {0}", LocalMode.ToString());
        }
    }

    #region Computers
    public class ComputersSection : ConfigurationSection
    {
        [ConfigurationProperty("computers", IsDefaultCollection = true)]

        public ComputersCollection Computers
        {
            get { return (ComputersCollection)base["computers"]; }
        }
    }

    public sealed class ComputersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ComputerElement();
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "computer";
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ComputerElement)element).IpAddress;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "computer"; }
        }

        public ComputerElement this[int index]
        {
            get { return (ComputerElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        new public ComputerElement this[string name]
        {
            get { return (ComputerElement)BaseGet(name); }
        }

        public bool ContainsKey(string key)
        {
            bool result = false;
            object[] keys = BaseGetAllKeys();

            foreach (object obj in keys)
            {
                if ((string)obj == key)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        [ConfigurationProperty("defaultusername", IsKey = false, IsRequired = false)]
        public string DefaultUserName
        {
            get { return (string)base["defaultusername"]; }
            set { base["defaultusername"] = value; }
        }

        [ConfigurationProperty("defaultpassword", IsKey = false, IsRequired = false)]
        public string DefaultPassword
        {
            get { return (string)base["defaultpassword"]; }
            set { base["defaultpassword"] = value; }
        }

        public ComputerElement GetComputerElement(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return null;

            if (this.ContainsKey(ipAddress))
                return this[ipAddress];

            return new ComputerElement()
            {
                Username = this.DefaultUserName,
                Password = this.DefaultPassword,
                IpAddress = ipAddress
            };
        }
    }

    public sealed class ComputerElement : ConfigurationElement
    {
        [ConfigurationProperty("ipAddress", IsRequired = true)]
        public string IpAddress
        {
            get { return (string)this["ipAddress"]; }
            set { this["ipAddress"] = value; }
        }

        [ConfigurationProperty("username", IsKey = false, IsRequired = false)]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", IsKey = false, IsRequired = false)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        public override string ToString()
        {
            return string.Format("ComputerElement: IpAddress = {0}, Username = {1}, Password = {2}",
                                 IpAddress, Username, Password);
        }
    }
    #endregion

    #region Processes

    public class ProcessesSection : ConfigurationSection
    {
        [ConfigurationProperty("processes", IsDefaultCollection = true)]

        public ProcessesCollection Processes
        {
            get { return (ProcessesCollection)base["processes"]; }

        }
    }
    public sealed class ProcessesCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProcessElement();
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "process";
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProcessElement)element).Name;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "process"; }
        }


        public ProcessElement this[int index]
        {
            get { return (ProcessElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        new public ProcessElement this[string name]
        {
            get { return (ProcessElement)BaseGet(name); }
        }

        public bool ContainsKey(string key)
        {
            bool result = false;
            object[] keys = BaseGetAllKeys();

            foreach (object obj in keys)
            {
                if ((string)obj == key)
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
    public sealed class ProcessElement : ConfigurationElement
    {
        [ConfigurationProperty("services", IsDefaultCollection = true)]
        public ServicesCollection Services
        {
            get { return (ServicesCollection)base["services"]; }
        }

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("processName", IsRequired = true)]
        public string ProcessName
        {
            get { return (string)this["processName"]; }
            set { this["processName"] = value; }
        }

        [ConfigurationProperty("ipAddress", IsRequired = true)]
        public string IpAddress
        {
            get { return (string)this["ipAddress"]; }
            set { this["ipAddress"] = value; }
        }

        [ConfigurationProperty("computerName", IsRequired = false)]
        public string ComputerName
        {
            get { return (string)this["computerName"]; }
            set { this["computerName"] = value; }
        }

        [ConfigurationProperty("logsdirectory", IsKey = false, IsRequired = false, DefaultValue = null)]
        public string LogsDirectory
        {
            get { return (string)this["logsdirectory"]; }
            set { this["logsdirectory"] = value; }
        }

        [ConfigurationProperty("runinlocalmode", IsKey = false, IsRequired = false, DefaultValue = "false")]
        public string RunInLocalMode
        {
            get { return (string)this["runinlocalmode"]; }
            set { this["runinlocalmode"] = value; }
        }

        public override string ToString()
        {
            return string.Format("ProcessElement: Name = {0}, ProcessName = {1}, IpAddress = {2}, ComputerName = {3}, RunInLocalMode = {4}",
                                 Name,
                                 ProcessName,
                                 IpAddress,
                                 ComputerName,
                                 RunInLocalMode);
        }
    }
    public sealed class ServicesCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceConfigurationElement();
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName == "service";
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServiceConfigurationElement)element).Contract;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "service"; }
        }


        public ServiceConfigurationElement this[int index]
        {
            get { return (ServiceConfigurationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        new public ServiceConfigurationElement this[string name]
        {
            get { return (ServiceConfigurationElement)BaseGet(name); }
        }

        public bool ContainsKey(string key)
        {
            bool result = false;
            object[] keys = BaseGetAllKeys();

            foreach (object obj in keys)
            {
                if ((string)obj == key)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
    public sealed class ServiceConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public string Port
        {
            get { return (string)this["port"]; }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("contract", IsRequired = true)]
        public string Contract
        {
            get { return (string)this["contract"]; }
            set { this["contract"] = value; }
        }

        [ConfigurationProperty("webcontract", IsRequired = false)]
        public string WebContract
        {
            get { return (string)this["webcontract"]; }
            set { this["webcontract"] = value; }
        }

        [ConfigurationProperty("webport", IsRequired = false)]
        public string WebPort
        {
            get { return (string)this["webport"]; }
            set { this["webport"] = value; }
        }

        [ConfigurationProperty("binding", DefaultValue = "Default", IsRequired = false)]
        public string Binding
        {
            get { return (string)this["binding"]; }
            set { this["binding"] = value; }
        }
    }

    #endregion
}
