using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Project.BaseLib.Utils
{
    public class Singleton<T> where T : Singleton<T>
    {
        private static T instance;
        private static object sync = new object();

        protected Singleton() { }

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    CreateInstance();
                }

                return instance;
            }
        }


        public static void CreateInstance()
        {
            if(instance == null)
            {
                Type t = typeof(T);

                ConstructorInfo[] ctors = t.GetConstructors();

                if(ctors.Length > 0)
                {
                    throw new InvalidOperationException(
                        String.Format(
                            "{0} has at least one accesible ctor making it impossible to enforce singleton hevaviour",
                            t.Name));
                }
                instance = (T)Activator.CreateInstance(t, true);
            }
        }
    }
}
