using StructureMap;

namespace SdojWeb.Infrastructure.IoC
{
    public class StandardRegistry : Registry
    {
        public StandardRegistry()
        {
            Scan(scan =>
            {
                scan.AssemblyContainingType<MvcApplication>();
                scan.WithDefaultConventions();
            });
        }
    }
}