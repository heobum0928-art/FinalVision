using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    //Attribute used with fields in Code Generator to mark the field as a Derived classed
    public class IsDerivedAttribute : Attribute { }

    public static class TypesLoader
    {
        private static Type[] extraTypes;
        private static object syncObject = new object();

        public static Type[] ExtraTypes
        {
            get
            {
                lock (syncObject)
                {
                    if (extraTypes == null)
                    {
                        var executingAssembly = Assembly.GetExecutingAssembly();
                        var l = executingAssembly.Location;

                        extraTypes = TypesLoader.GetTypes(Path.GetDirectoryName(executingAssembly.Location) + @"\Project.BaseLib.DLL", "Project.BaseLib");
                    }

                    return extraTypes;
                }
            }
        }

        private static Type[] GetTypes(string assemblyName, string assemblyNamespace)
        {
            Assembly assembly;
            assembly = Assembly.LoadFrom(assemblyName);

            return AddTypes(assembly, assemblyNamespace);
        }

        private static Type[] AddTypes(Assembly assembly, string assemblyNamespace)
        {
            var query = assembly.GetTypes()
                         .Where(t => t.IsClass && t.Namespace == assemblyNamespace && !t.IsGenericType && Attribute.IsDefined(t, typeof(IsDerivedAttribute)));

            return query.ToArray();
        }
    }

    public static class GenericTypesLoader
    {
        private static Type[] extraTypes;
        private static object syncObject = new object();


        public static Type[] ExtraTypes
        {
            get
            {
                lock (syncObject)
                {
                    if (extraTypes == null)
                    {
                        var executingAssembly = Assembly.GetExecutingAssembly();
                        var l = executingAssembly.Location;

                        extraTypes = GenericTypesLoader.GetTypes(Path.GetDirectoryName(executingAssembly.Location) + @"\Project.BaseLib.DLL", "Project.BaseLib");
                    }

                    return extraTypes;
                }
            }
        }

        private static Type[] GetTypes(string assemblyName, string assemblyNamespace)
        {
            Assembly assembly;
            assembly = Assembly.LoadFrom(assemblyName);

            return AddTypes(assembly, assemblyNamespace);
        }

        private static Type[] AddTypes(Assembly assembly, string assemblyNamespace)
        {
            var query = assembly.GetTypes()
                  .Where(type => type.IsClass && type.Namespace == assemblyNamespace && Attribute.IsDefined(type, typeof(DataContractAttribute)));

            return query.ToArray();
        }
    }
}
