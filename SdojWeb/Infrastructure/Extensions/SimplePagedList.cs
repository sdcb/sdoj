using System.Collections.Generic;

namespace SdojWeb.Infrastructure.Extensions
{
    public class SimplePagedList<T>
    {
        public List<T> Items { get; set; }

        public int TotalCount { get; set; }
    }
}