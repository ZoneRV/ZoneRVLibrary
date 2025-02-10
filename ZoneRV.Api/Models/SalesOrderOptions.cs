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

    public SalesOrderSortingOptions? SalesOrderSortingOptions;
    public PaginationOptions?        Pagination  { get; set; }
    public CardOptions?              CardOptions { get; set; }

    public Func<SalesOrder, bool> FilterFunction()
    {
        return so =>
            (this.WorkspaceIds is null || !this.WorkspaceIds.Any() || this.WorkspaceIds.Contains(so.Model.Line.Workspace.Id)) &&
            (this.LineIds is null || !this.LineIds.Any() || this.LineIds.Contains(so.Model.Line.Id)) &&
            (this.ModelIds is null || !this.ModelIds.Any() || this.ModelIds.Contains(so.Model.Id)) &&
            (this.Names is null || !this.Names.Any() || this.Names.Contains(so.Name)) &&
            (this.Ids is null || !this.Ids.Any() || so.Id is null || this.Ids.Contains(so.Id)) &&
            (this.OrderedLocationIds is null || !this.OrderedLocationIds.Any() || so.LocationInfo.CurrentLocation is not null && this.OrderedLocationIds.Contains(so.LocationInfo.CurrentLocation.Id)) &&
            (this.WorkspaceLocationIds is null || !this.WorkspaceLocationIds.Any() || so.LocationInfo.CurrentLocation is not null && this.WorkspaceLocationIds.Contains(so.LocationInfo.CurrentLocation.Location.Id));
    }

    public IEnumerable<SalesOrder> OrderFunction(IEnumerable<SalesOrder> salesOrders)
    {
        if (SalesOrderSortingOptions is null)
            return salesOrders;

        switch (SalesOrderSortingOptions)
        {
            case Models.SalesOrderSortingOptions.Name:
                return salesOrders.OrderBy(x => x.Name, StringComparer.InvariantCulture);

            case Models.SalesOrderSortingOptions.Progress:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => x.Progress);

            case Models.SalesOrderSortingOptions.CurrentLocation:
                return salesOrders.Where(x => x.LocationInfo.CurrentLocation is not null).OrderBy(x => x.LocationInfo.CurrentLocation?.Order);

            case Models.SalesOrderSortingOptions.RedlineDate:
                return salesOrders.Where(x => x.RedlineDate is not null).OrderBy(x => x.RedlineDate);

            case Models.SalesOrderSortingOptions.JobCardsCompleteCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).JobCompleted);

            case Models.SalesOrderSortingOptions.JobCardsDueCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).JobCardsDue);

            case Models.SalesOrderSortingOptions.JobCardsOutStandingCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).JobCardsOutStanding);

            case Models.SalesOrderSortingOptions.RedCardsCompleteCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).RedCardsCompleted);

            case Models.SalesOrderSortingOptions.RedCardsIncompleteCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).RedCardsIncomplete);

            case Models.SalesOrderSortingOptions.YellowCardsCompleteCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).YellowCardsCompleted);

            case Models.SalesOrderSortingOptions.YellowCardsIncompleteCount:
                return salesOrders.Where(x => x.ProductionInfoLoaded).OrderBy(x => (x.Stats ?? new SalesOrderStats(x, CardOptions is null ? null : CardOptions.FilterFunction())).YellowCardsIncomplete);

            default:
                throw new ArgumentOutOfRangeException(Enum.GetName((SalesOrderSortingOptions)SalesOrderSortingOptions), "Unsupported sorting option specified.");
        }
    }
}