using System;
using System.Web.Http;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.TypeRules;
using System.Web.Mvc;
using System.Diagnostics;

namespace SdojWeb.Infrastructure.IoC
{
    public class ControllerConvention : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            Debug.WriteLine(type.Name);
            if ((type.CanBeCastTo(typeof(Controller)) || type.CanBeCastTo(typeof(ApiController)))
                && !type.IsAbstract)
            {
                registry.For(type).LifecycleIs(new UniquePerRequestLifecycle());
            }
        }
    }
}