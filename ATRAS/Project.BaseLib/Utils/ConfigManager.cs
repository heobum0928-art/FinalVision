using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        #region Fields
        private Dictionary<string, ConfigurationBase> _Configurations;
        public Dictionary<string, ConfigurationBase> Configurations
        {
            get { return _Configurations; }
        }
        #endregion

        #region Constructor

        protected ConfigManager()
        {
            _Configurations = new Dictionary<string, ConfigurationBase>();
        }
        #endregion

        #region methods
        public T GetConfiguration<T>() where T : ConfigurationBase
        {
            Type configType = typeof(T);

            if (!_Configurations.ContainsKey(configType.Name))
            {
                T config = (T)Activator.CreateInstance(configType);

                _Configurations[configType.Name] = config;
                _Configurations[configType.Name].Load();
            }

            return (T)_Configurations[configType.Name];
        }

        public ConfigurationBase GetConfiguration(string configName)
        {
            if(_Configurations.ContainsKey(configName))
            {
                return _Configurations[configName];
            }
            
            Type configType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (configType != null)
                    break;

                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == configName)
                    {
                        configType = type;
                        break;
                    }
                }
            }

            if (configType == null)
                return null;

            if (!_Configurations.Keys.Contains(configType.Name))
            {
                ConfigurationBase config = (ConfigurationBase)Activator.CreateInstance(configType);
                _Configurations[configType.Name] = config;
                config.Load();
            }
            return _Configurations[configType.Name];
        }

        public string OnConfigurationDataCheck(string category, string dbName, string xml)
        {
            var config = GetConfiguration(dbName);
            if (config != null)
            {
                config.SetData(xml);
                config.Save();
            }
            return xml;
        }

        public void CreateConfiguration()
        {
            List<Type> types = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(ConfigurationBase)))
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (Exception e)
                {

                }
                        
                //var types11 = assembly.GetTypes();

            }

            foreach (var type in types)
            {
                ConfigurationBase config = (ConfigurationBase)Activator.CreateInstance(type);
                if (!config.Exists())
                    config.Save();
            }
        }

        #endregion
    }

}
