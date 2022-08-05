using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemsPage, int totalItems, int totalpages)
        {
            CurrentPage = currentPage;
            ItemsPage = itemsPage;
            TotalItems = totalItems;
            TotalPages = totalpages;
        }

        public int CurrentPage { get; set; }
        public int ItemsPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}