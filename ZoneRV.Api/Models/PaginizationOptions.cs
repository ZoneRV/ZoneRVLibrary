namespace ZoneRV.Api.Models;

public class PaginationOptions
{
    public uint PageLimit { get; set; }
    public uint PageCount { get; set; }

    public uint PageStartIndex => checked(PageLimit * (PageCount - 1));
    public uint PageEndIndex   => PageStartIndex + PageLimit;
}