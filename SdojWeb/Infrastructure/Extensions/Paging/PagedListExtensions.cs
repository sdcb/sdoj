using System.Linq;
using PagedList;
using EntityFramework.Extensions;

namespace SdojWeb.Infrastructure.Extensions
{
    public static class PagedListExtensions
    {
        public static IPagedList<TModel> ToSortedPagedList<TModel>(this IQueryable<TModel> query, int? page, string orderBy, bool? asc) where TModel : class
        {
            ArrangePagingArgs(ref query, ref page, orderBy, asc);

            var sortablePagedList = new SortablePagedList<TModel>(query, page.Value, AppSettings.DefaultPageSize)
            {
                OrderBy = orderBy,
                Asc = asc
            };
            return sortablePagedList;
        }

        public static SimplePagedList<TModel> ToSimplePagedList<TModel>(this IQueryable<TModel> query, 
            int? page, string orderBy, bool? asc) where TModel : class
        {
            ArrangePagingArgs(ref query, ref page, orderBy, asc);

            var pageSize = AppSettings.DefaultPageSize;

            var futureCount = query.FutureCount();
            var futureData = page.Value == 1
                ? query.Take(pageSize).Future()
                : query.Skip((page.Value - 1) * pageSize).Take(pageSize).Future();

            return new SimplePagedList<TModel>
            {
                Items = futureData.ToList(), 
                TotalCount = futureCount
            };
        }

        private static void ArrangePagingArgs<TModel>(ref IQueryable<TModel> query, ref int? page, string orderBy, bool? asc) where TModel : class
        {
            page = page ?? 1;

            if (!string.IsNullOrWhiteSpace(orderBy) && asc != null)
            {
                var orderByString = string.Format("{0} {1}", orderBy, asc.Value ? "asc" : "desc");
                query = query.OrderBy(orderByString);
            }
        }
    }
}