using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Reflection;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;


namespace Project.BaseLib.Communication
{

    public partial class GenericType
    {
        public GenericType()
        {
            this.BasicTypes = new List<BasicType>();
        }

        public List<BasicType> BasicTypes { get; set;}

        public string AssemblyName {get; set;}
      
        public string NamespaceName { get; set;}
      
        public string TypeName { get; set;}
       
    }

    public partial class BasicType
    {
        public string AssemblyName  { get; set;}

        public string NamspaceName  { get; set;}

        public string TypeName  { get; set;}       
    }
}
