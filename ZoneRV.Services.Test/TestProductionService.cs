using Bogus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZoneRV.DBContexts;
using ZoneRV.Models;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;

namespace ZoneRV.Services.Test;

public class TestProductionService : IProductionService
{
    private int _id = 0;
    
    public TestProductionService(IServiceScopeFactory scopeFactory, IConfiguration configuration, Faker faker) : base(scopeFactory, configuration)
    {
        _faker = faker;
    }
    
    protected override string ServiceTypeName { get => "test"; }
    protected override Task SetupService(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected override Task LoadUsers(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < _faker.Random.Int(50, 100); i++)
        {
            var user = new User()
            {
                Id = i.ToString(),
                FullName = _faker.Person.UserName,
                AvatarUrl = _faker.Internet.Avatar(),
                Username = _faker.Internet.UserName()
            };

            Users.TryAdd(user.Id, user);
        }

        return Task.CompletedTask;
    }
    
    private int _vansInPreProd  = 10;
    private int _vansHandedOver = 10;

    protected override Task LoadProductionInfo(CancellationToken cancellationToken = default)
    {
        foreach (var productionLine in ProductionLines)
        {
            var locationOrder = productionLine.OrderedLineLocations.Where(x => x.Location.LocationType is ProductionLocationType.Bay or ProductionLocationType.Finishing or ProductionLocationType.Prep).OrderBy(x => x.Order).ToList();
            int vanNumber     = 0;
            int vanCount      = _faker.Random.Int(50, 75);
            
            for (int i = vanCount; i > 0 ; i--)
            {
                vanNumber++;
                var pastLocations = locationOrder.Take(i - _vansInPreProd);

                var locationHistory = pastLocations.Select(x => (DateTimeOffset.Now - TimeSpan.FromDays(-_vansInPreProd + locationOrder.Count + i), x));
                
                var salesOrder =
                    new SalesOrder()
                    {
                        Number       = i.ToString("000"),
                        Model        = _faker.PickRandom(productionLine.Models),
                        Url          = _faker.Internet.Url(),
                        Id           = (_id++).ToString(),
                        LocationInfo = new LocationInfo(productionLine, locationHistory)
                    };

                if (i > vanCount - _vansHandedOver)
                {
                    if (_faker.Random.Int(0, 3) % 3 == 0)
                        salesOrder.HandoverState = HandoverState.UnhandedOver;
                    else
                        salesOrder.HandoverState = HandoverState.HandedOver;
                }
                else
                {
                    salesOrder.HandoverState = HandoverState.UnhandedOver;
                }

                salesOrder.AddHandoverHistory(DateTimeOffset.Now - TimeSpan.FromDays(i + 15), DateTimeOffset.Now + TimeSpan.FromDays(vanNumber - _vansHandedOver)); 
                
                SalesOrders.TryAdd(salesOrder.Id, salesOrder);
            }
        }

        return Task.CompletedTask;
    }

    private            Faker  _faker          { get; set; }
    protected override Task<SalesOrder> _loadSalesOrderFromSourceAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default)
    {
        foreach (var area in salesOrder.Model.Line.AreaOfOrigins)
        {
            var locations = salesOrder.Model.Line.OrderedLineLocations.Where(x => x.CustomNames.Any(cn => cn.CustomName.Split(':')[1] == area.Name));

            foreach (var location in locations)
            {
                for (int i = 0; i < _faker.Random.Int(10, 15); i++)
                {
                    var jobinfo =
                        new JobCardCreationInfo()
                        {
                            Name = _faker.WaffleTitle(),
                            AttachmentInfos = [],
                            CardStatus = 
                                location < salesOrder.LocationInfo.CurrentLocation! 
                                ? _faker.Random.WeightedRandom(
                                    [CardStatus.Completed, CardStatus.InProgress, CardStatus.NotStarted, CardStatus.UnableToComplete], [2f, 1f, 1f, 0.5f]) 
                                : CardStatus.NotStarted,
                            CardStatusLastUpdated = salesOrder.LocationInfo.FirstOrDefault(x => x.lineLocation == location).moveDate,
                            ChecklistInfos        = [],
                            CommentInfos          = [],
                            Id                    = (_id++).ToString(),
                            TaskTime              = TimeSpan.FromMinutes(_faker.Random.Int(15, 120)),
                            Url                   = _faker.Internet.Url()
                        };

                    if (jobinfo.CardStatusLastUpdated != default(DateTimeOffset))
                        jobinfo.CardStatusLastUpdated += TimeSpan.FromHours(_faker.Random.Double(0, 10));

                    FilloutCardInfo(jobinfo, salesOrder.Model);
                    
                    BuildJobCard(salesOrder, jobinfo, area, location);
                }
            }
            
            

            if (salesOrder.LocationInfo.CurrentLocation is null)
            {
                continue;
            }

            double redAndYellowMultiplier = (double)salesOrder.LocationInfo.Count() / (double)salesOrder.Model.Line.OrderedLineLocations.Count();

            int redCardCount = (int)(_faker.Random.Int(3, 10) * redAndYellowMultiplier);

            for (int i = 0; i < redCardCount; i++)
            {
                var redCardInfo =
                    new RedCardCreationInfo()
                    {
                        AttachmentInfos = [],
                        CardStatus =
                            _faker.Random.WeightedRandom(
                                [CardStatus.Completed, CardStatus.InProgress, CardStatus.NotStarted, CardStatus.UnableToComplete], [2f, 1f, 1f, 0.5f]),
                        CardStatusLastUpdated = null, // TODO
                        ChecklistInfos        = [],
                        CommentInfos          = [],
                        CreationDate          = DateTimeOffset.Now - TimeSpan.FromDays(_faker.Random.Double(0, 50)),
                        Id                    = (_id++).ToString(),
                        Name                  = _faker.WaffleTitle(),
                        RedFlagIssue          = _faker.PickRandom<RedFlagIssue>(),
                        Url                   = _faker.Internet.Url()
                    };

                FilloutCardInfo(redCardInfo, salesOrder.Model);

                BuildRedCard(salesOrder, redCardInfo, area);
            }

            int yellowCardCount = (int)(_faker.Random.Int(0, 2) * redAndYellowMultiplier);

            for (int i = 0; i < yellowCardCount; i++)
            {
                var yellowCardInfo =
                    new YellowCardCreationInfo()
                    {
                        AttachmentInfos = [],
                        CardStatus =
                            _faker.Random.WeightedRandom(
                                [CardStatus.Completed, CardStatus.InProgress, CardStatus.NotStarted, CardStatus.UnableToComplete], [2f, 1f, 1f, 0.5f]),
                        CardStatusLastUpdated = null, // TODO
                        ChecklistInfos        = [],
                        CommentInfos          = [],
                        CreationDate          = DateTimeOffset.Now - TimeSpan.FromDays(_faker.Random.Double(0, 50)),
                        Id                    = (_id++).ToString(),
                        Name                  = _faker.WaffleTitle(),
                        Url                   = _faker.Internet.Url()
                    };

                FilloutCardInfo(yellowCardInfo, salesOrder.Model);

                BuildYellowCard(salesOrder, yellowCardInfo, area);
            }
        }
        
        return Task.FromResult(salesOrder);
    }

    private CardCreationInfo FilloutCardInfo(CardCreationInfo card, Model model)
    {
        for (int j = 0; j < _faker.Random.Int(0, 5); j++)
        {
            card.AttachmentInfos.Add(new AttachmentCreationInfo()
            {
                Id  = (_id++).ToString(),
                Url = _faker.Image.PlaceholderUrl(100, 100)
            });
        }

        for (int j = 0; j < _faker.Random.Int(0, 3); j++)
        {
            var checklist = new ChecklistCreationInfo()
            {
                Id   = (_id++).ToString(), 
                Name = $"{card.Name}: {j + 1}"
            };

            for (int k = 0; k < _faker.Random.Int(0, 10); k++)
            {
                bool state;

                if (card.CardStatus is CardStatus.Completed)
                    state = true;
                            
                else if (card.CardStatus is CardStatus.NotStarted)
                    state = false;

                else
                    state = _faker.Random.Bool();
                            
                checklist.CheckInfos.Add(new CheckCreationInfo()
                {
                    Id         = (_id++).ToString(),
                    IsChecked  = state,
                    LasUpdated = state && card.CardStatusLastUpdated != default(DateTimeOffset) ? card.CardStatusLastUpdated - TimeSpan.FromMinutes(_faker.Random.Int(15, 45)) : null,
                    Name       = _faker.WaffleText(1, false)
                });
            }
                        
            card.ChecklistInfos.Add(checklist);
        }

        for (int i = 0; i < _faker.Random.Int(0, 10); i++)
        {
            card.CommentInfos.Add(new CommentCreationInfo()
            {
                AuthorId = _faker.PickRandom(Users.Keys),
                Content = _faker.Rant.Review(model.Name),
                DateCreated = DateTimeOffset.Now - TimeSpan.FromDays(_faker.Random.Double(0, 40)),
                Id = (_id++).ToString()
            });
        }

        return card;
    }

    public override int MaxDegreeOfParallelism { get; protected set; } = 1;
}