using System;
using System.Collections.Generic;
using System.Linq;
namespace EMart.ViewModels
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        public string SearchText { get; set; } = string.Empty;

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public int TotalRecords { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}