using System.Linq;
using PagedList;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class PagedListExtensions
    {
        public static IPagedList<TModel> ToSortedPagedList<TModel>(this IQueryable<TModel> models, int? page, string orderBy, bool? asc)
        {
            page = page ?? 1;
            orderBy = orderBy ?? "Id";
            asc = asc ?? false;

            var orderByString = string.Format("{0} {1}", orderBy, asc.Value ? "asc" : "desc");
            var ordered = models.OrderBy(orderByString);
            var sortablePagedList = new SortablePagedList<TModel>(ordered, page.Value, AppSettings.DefaultPageSize)
            {
                OrderBy = orderBy, 
                Asc = asc.Value
            };
            return sortablePagedList;
        }
    }
}