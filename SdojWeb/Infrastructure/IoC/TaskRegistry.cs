using SdojWeb.Infrastructure.Tasks;
using StructureMap.Configuration.DSL;

namespace SdojWeb.Infrastructure.IoC
{
    public class TaskRegistry : Registry
    {
        public TaskRegistry()
        {
            Scan(scan =>
            {
                var myNamespace = typeof(MvcApplication).Namespace;
                scan.AssembliesFromApplicationBaseDirectory(
                    a => a.FullName.StartsWith(myNamespace));
                scan.AddAllTypesOf<IRunAtInit>();
                scan.AddAllTypesOf<IRunAtStartup>();
                scan.AddAllTypesOf<IRunOnEachRequest>();
                scan.AddAllTypesOf<IRunOnError>();
                scan.AddAllTypesOf<IRunAfterEachRequest>();
            });
        }
    }
}