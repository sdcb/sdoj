using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdojWeb.Infrastructure.Mapping
{
    interface IHaveCustomMapping
    {
        void CreateMappings(IConfiguration configuration);
    }
}
