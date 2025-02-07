// ReSharper disable CollectionNeverUpdated.Global
namespace ZoneRV.Api.Models;

public class SalesOrderOptions
{
    public List<string>? OptionalFields; 
    public List<int>?    WorkspaceIds;
    public List<int>?    LineIds;
    public List<int>?    ModelIds;
    public List<string>? Names;
    public List<string>? Ids;
    public List<int>?    OrderedLocationIds;
    public List<int>?    WorkspaceLocationIds;

    public PaginationOptions? Pagination { get; set; }
}