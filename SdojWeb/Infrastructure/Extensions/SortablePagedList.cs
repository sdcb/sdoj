using System.Collections.Generic;
using System.Linq;
using PagedList;

namespace SdojWeb.Infrastructure.Extensions
{
    public class SortablePagedList<T> : PagedList<T>
    {
        public SortablePagedList(IQueryable<T> superset, int pageNumber, int pageSize) : base(superset, pageNumber, pageSize)
        {
        }

        public SortablePagedList(IEnumerable<T> superset, int pageNumber, int pageSize) : base(superset, pageNumber, pageSize)
        {
        }

        public string OrderBy { get; set; }

        public bool Asc { get; set; }
    }
}