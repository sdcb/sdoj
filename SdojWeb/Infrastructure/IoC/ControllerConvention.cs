using System;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.TypeRules;
using System.Web.Mvc;
using StructureMap;
using StructureMap.Graph.Scanning;

namespace SdojWeb.Infrastructure.IoC
{
    public class ControllerConvention : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            if ((type.CanBeCastTo(typeof(Controller)))
                && !type.IsAbstract)
            {
                registry.For(type).LifecycleIs(new UniquePerRequestLifecycle());
            }
        }

        public void ScanTypes(TypeSet types, Registry registry)
        {
            foreach (Type type in types.AllTypes())
            {
                Process(type, registry);
            }
        }
    }
}