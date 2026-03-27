using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Linq;
using Project.BaseLib.Logger;
using Project.BaseLib.Utils;

namespace Project.BaseLib.Communication
{
    public class GenericResolver : DataContractResolver
    {    
        ILogger Logger;

        private readonly string SYSTEM_COLLECTION_GENERIC = "System.Collections.Generic";

        readonly Dictionary<Type, Tuple<string, string>> typeToNamespaceMap;
        readonly Dictionary<string, Dictionary<string, Type>> namespaceToTypeMap;
        XmlDictionary dictionary = new XmlDictionary();

        public void AddTypes()
        {
            var types = GenericTypesLoader.ExtraTypes;

            foreach (Type type in types)
                AddType(type);
        }
 
        public void AddTypes(params Type[] typesToResolve)
        {
            foreach (Type type in typesToResolve)
                AddType(type);
        }
     
        private void AddType(Type type)
        { 
            string typeNamespace = type.Namespace;
            string typeName = type.Name;

            typeToNamespaceMap[type] = new Tuple<string, string>(typeNamespace, typeName);

            if (namespaceToTypeMap.ContainsKey(type.Namespace) == false)
                namespaceToTypeMap[type.Namespace] = new Dictionary<string, Type>();

            namespaceToTypeMap[type.Namespace][type.Name] = type;
        }

        public GenericResolver()
            : base()
        {
            try
            {         
                typeToNamespaceMap = new Dictionary<Type, Tuple<string, string>>();
                namespaceToTypeMap = new Dictionary<string, Dictionary<string, Type>>();
                Logger = LogManager.GetLogger("Host");
                AddTypes();
            }
            catch (Exception e)
            {
            }
            
        }

        // Used at deserialization 
        // Allows users to map xsi:type name to any Type  
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            Type type = null;

            if (namespaceToTypeMap.ContainsKey(typeNamespace))
            {
                if (namespaceToTypeMap[typeNamespace].ContainsKey(typeName))
                {
                    type = namespaceToTypeMap[typeNamespace][typeName];  
                    
                    return type;
                }

                try
                {
                    return HandleGeneric(typeName);
                }
                catch(Exception e)
                {
                    if(Logger != null)
                        Logger.Error(e, e.Message);
                }       
            }

            if(typeNamespace.Equals(SYSTEM_COLLECTION_GENERIC))
            {
                try
                {                    
                    return HandleGeneric(typeName);
                }
                catch (Exception e)
                {
                    if (Logger != null)
                        Logger.Error(e, e.Message);
                }               
            }


            var resolveName = knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);

            if(resolveName == null)
                if(Logger != null)
                    Logger.Error("ResolveName Failed : TypeName=" + typeName + ", typeNamespace=" + typeNamespace + ", DeclaredType=" + declaredType.ToString());

            return resolveName;
        }

        public Type HandleGeneric(string typeName)
        {
          
            GenericType t = JsonSerializer<GenericType>.Deserialize(typeName);

            Type g = Type.GetType(t.NamespaceName + "." + t.TypeName + "," + t.AssemblyName);

            Type[] typeArgs = new Type[t.BasicTypes.Count];

            for (int i = 0; i < t.BasicTypes.Count; ++i)
            {
                typeArgs[i] =
                    Type.GetType(t.BasicTypes[i].NamspaceName + "." + t.BasicTypes[i].TypeName + "," + t.BasicTypes[i].AssemblyName);
            }

            Type serviceType = g.MakeGenericType(typeArgs);

            return serviceType;

        }

        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (typeToNamespaceMap.ContainsKey(type))
            {
                
                typeNamespace = dictionary.Add(typeToNamespaceMap[type].Item1);
                typeName = dictionary.Add(typeToNamespaceMap[type].Item2);   
             
                return true;
            }

            if (type.GenericTypeArguments.Any())
            {
                try
                {
                    GenericType gType = new GenericType();

                    typeNamespace = dictionary.Add(type.Namespace);

                    gType.AssemblyName = type.Assembly.FullName;
                    gType.NamespaceName = type.Namespace;
                    gType.TypeName = type.Name;

                    gType.BasicTypes = new List<BasicType>();

                    for (int i = 0; i < type.GenericTypeArguments.Count() ; ++i)
                    {
                        gType.BasicTypes.Add(new BasicType()
                        {
                            AssemblyName = type.GenericTypeArguments[i].Assembly.FullName,
                            NamspaceName = type.GenericTypeArguments[i].Namespace,
                            TypeName = type.GenericTypeArguments[i].Name
                        });
                    }

                    typeName = dictionary.Add(JsonSerializer<GenericType>.Serialize(gType));
                
                    return true;
                }
                catch (Exception e)
                {
                    if (Logger != null)
                        Logger.Error(e, e.Message);
                }
            }


            var resolveType = knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
            if(!resolveType)
                if (Logger != null)
                    Logger.Error("Try Resolve Failed : Type=" + type.ToString() + ", " + "DeclaredType=" + declaredType.ToString());

            return resolveType;
        }
    }
}
