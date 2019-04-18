using StructureMap;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.ModelMetadata
{
    public class ModelMetadataRegistry : Registry
    {
        public ModelMetadataRegistry()
        {
            For<ModelMetadataProvider>().Use<ExtensibleModelMetadataProvider>();

            Scan(scan =>
            {
                scan.AssemblyContainingType<MvcApplication>();
                scan.AddAllTypesOf<IModelMetadataFilter>(); 
            });
        }
    }
}