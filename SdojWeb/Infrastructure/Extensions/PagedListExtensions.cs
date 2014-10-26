using System.Linq;
using PagedList;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class PagedListExtensions
    {
        public static IPagedList<TModel> ToSortedPagedList<TModel>(this IQueryable<TModel> query, int? page, string orderBy, bool? asc) where TModel : class 
        {
            page = page ?? 1;

            if (!string.IsNullOrWhiteSpace(orderBy) && asc != null)
            {
                var orderByString = string.Format("{0} {1}", orderBy, asc.Value ? "asc" : "desc");
                query = query.OrderBy(orderByString);
            }
            
            var sortablePagedList = new SortablePagedList<TModel>(query, page.Value, AppSettings.DefaultPageSize)
            {
                OrderBy = orderBy, 
                Asc = asc
            };
            return sortablePagedList;
        }
    }
}