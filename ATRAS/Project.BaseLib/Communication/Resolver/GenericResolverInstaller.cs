using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.ServiceModel.Configuration;


namespace Project.BaseLib.Communication
{
    public static class GenericResolverInstaller
   {
       static GenericResolver genericResolver;

       static GenericResolver CreateGenericResolver(string folderPath = "./")
       {
            lock (typeof(GenericResolver))
            {
                if (genericResolver == null)
                {
                    genericResolver = new GenericResolver();
                    genericResolver.AddTypes();
                }

                return genericResolver;     
            }
       }

      public static void AddGenericResolver(this ServiceHost host, string folderPath = "./")
      {
         foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
             AddGenericResolver(endpoint, folderPath);
      }

      public static void AddGenericResolver<T>(this DuplexChannelFactory<T> factory, string folderPath = "./")
      {
          AddGenericResolver(factory.Endpoint, folderPath);
      }

      static void AddGenericResolver(ServiceEndpoint endpoint, string folderPath)
      {
         foreach(OperationDescription operation in endpoint.Contract.Operations)
         {
            DataContractSerializerOperationBehavior behavior = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();

            behavior.DataContractResolver = CreateGenericResolver(folderPath);
         }
      }
   }
 }


   



