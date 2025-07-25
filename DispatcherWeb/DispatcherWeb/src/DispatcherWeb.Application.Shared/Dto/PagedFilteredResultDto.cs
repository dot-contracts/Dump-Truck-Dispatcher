using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace DispatcherWeb.Dto
{
    [Serializable]
    public class PagedFilteredResultDto<T> : PagedResultDto<T>, IPagedFilteredResult<T>
    {
        /// <summary>Total count of Items after applied filters</summary>
        public int FilteredCount { get; set; }

        public PagedFilteredResultDto()
        {
        }

        /// <param name="totalCount">Total count of Items without filters</param>
        /// <param name="filteredCount">Total count of Items after applied filters</param>
        /// <param name="items">List of items in current page</param>
        public PagedFilteredResultDto(int totalCount, int filteredCount, IReadOnlyList<T> items)
          : base(totalCount, items)
        {
            FilteredCount = filteredCount;
        }
    }
}
