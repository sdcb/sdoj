using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SdojWeb.Infrastructure.Tasks
{
    public interface IRunAtInit
    {
        void Execute();
    }
}