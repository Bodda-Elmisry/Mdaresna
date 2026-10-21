using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Schools.Api.Controllers;
internal sealed record SchoolListWindow<T>(IQueryable<T> Query,int TotalCount,int PageNumber,int PageSize,int TotalPages)
{
    public object Result(object items) => new {items,totalCount=TotalCount,pageNumber=PageNumber,pageSize=PageSize,totalPages=TotalPages};
}
internal static class SchoolListPaging
{
    public static bool Valid(int? page,int size) => !page.HasValue || page is >=1 and <=1000000 && size is >=1 and <=100;
    public static async Task<SchoolListWindow<T>> Window<T>(IQueryable<T> query,int? page,int size,CancellationToken ct)
    {
        if (!page.HasValue) return new(query,0,1,size,1);
        var total=await query.CountAsync(ct);var pages=Math.Max(1,(int)Math.Ceiling(total/(double)size));var current=Math.Min(page.Value,pages);
        return new(query.Skip((current-1)*size).Take(size),total,current,size,pages);
    }
}
