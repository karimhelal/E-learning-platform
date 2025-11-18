using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Misc;

public class PagingRequest
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 9;

    public string SortBy { get; set; } = "title";
    public string SortOrder { get; set; } = "ASC";
}
