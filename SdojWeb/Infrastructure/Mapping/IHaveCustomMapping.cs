using AutoMapper;

namespace SdojWeb.Infrastructure.Mapping
{
    interface IHaveCustomMapping
    {
        void CreateMappings(IMapperConfigurationExpression configuration);
    }
}
