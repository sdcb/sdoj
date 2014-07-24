using StructureMap.Configuration.DSL;

namespace SdojWeb.Infrastructure.IoC
{
    public class ControllerRegistry : Registry
    {
        public ControllerRegistry()
        {
            Scan(scan =>
            {
                scan.AssemblyContainingType<MvcApplication>();
                scan.With(new ControllerConvention());
            });
        }
    }
}