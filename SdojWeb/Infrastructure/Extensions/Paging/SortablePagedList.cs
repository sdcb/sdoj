using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using EntityFramework.Extensions;
using PagedList;
using SdojWeb.Infrastructure.Html;

namespace SdojWeb.Infrastructure.Extensions
{
    [Serializable]
    public class SortablePagedList<T> : BasePagedList<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PagedList.PagedList`1"/> class that divides the supplied superset into subsets the size of the supplied pageSize. The instance then only containes the objects contained in the subset specified by index.
        /// 
        /// </summary>
        /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements <see cref="T:System.Linq.IQueryable`1"/>, it will be treated as such.</param><param name="pageNumber">The one-based index of the subset of objects to be contained by this instance.</param><param name="pageSize">The maximum size of any individual subset.</param><exception cref="T:System.ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception><exception cref="T:System.ArgumentOutOfRangeException">The specified page size cannot be less than one.</exception>
        public SortablePagedList(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");

            var futureCount = superset.FutureCount();
            var futureData = pageNumber == 1
                ? superset.Skip(0).Take(pageSize).Future()
                : superset.Skip((pageNumber - 1) * pageSize).Take(pageSize).Future();

            TotalItemCount = superset == null ? 0 : futureCount.Value;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            int num = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = num > TotalItemCount ? TotalItemCount : num;

            if (superset == null || TotalItemCount <= 0)
                return;

            Subset.AddRange(futureData.ToList());
        }

        public string OrderBy { get; set; }

        public bool? Asc { get; set; }

        public SortableThBuilder<T> GetThBuilder(HtmlHelper htmlHelper, string actionName, RouteValueDictionary route)
        {
            var builder = new SortableThBuilder<T>(htmlHelper, this, actionName, route);
            return builder;
        }
    }
}