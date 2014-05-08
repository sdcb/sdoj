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
            asc = asc ?? true;

            var orderByString = string.Format("{0} {1}", orderBy, asc.Value ? "asc" : "desc");
            var ordered = models.OrderBy(orderByString);
            var pagedList = ordered.ToPagedList(page.Value, AppSettings.DefaultPageSize);
            return pagedList;
        }
    }
}