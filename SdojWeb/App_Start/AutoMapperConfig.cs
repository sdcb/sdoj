using AutoMapper;
using SdojWeb.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SdojWeb
{
    public class AutoMapperConfig
    {
        public static void Execute()
        {
            var types = Assembly.GetExecutingAssembly().GetExportedTypes();
            Mapper.Initialize(cfg =>
            {
                LoadStandardMappings(cfg, types);
                LoadCustomMappings(cfg, types);
            });
        }

        private static void LoadCustomMappings(IMapperConfigurationExpression cfg, Type[] types)
        {
            IEnumerable<IHaveCustomMapping> mappings =
                (from t in types
                 from i in t.GetInterfaces()
                 where typeof(IHaveCustomMapping).IsAssignableFrom(t) &&
                     !t.IsAbstract &&
                     !t.IsInterface
                 select (IHaveCustomMapping)Activator.CreateInstance(t));

            foreach (var mapping in mappings)
            {
                mapping.CreateMappings(cfg);
            }
        }

        private static void LoadStandardMappings(IMapperConfigurationExpression cfg, Type[] types)
        {
            var maps =
                (from t in types
                 from i in t.GetInterfaces()
                 where i.IsGenericType &&
                       i.GetGenericTypeDefinition() == typeof(IMapFrom<>) &&
                       !t.IsAbstract &&
                       !t.IsInterface
                 select new
                 {
                     Source = i.GetGenericArguments()[0],
                     Destination = t,
                 });

            foreach (var map in maps)
            {
                cfg.CreateMap(map.Source, map.Destination);
            }
        }
    }
}