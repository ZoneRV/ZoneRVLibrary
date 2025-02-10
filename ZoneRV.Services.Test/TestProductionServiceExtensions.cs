using System.Text.RegularExpressions;
using Bogus;
using Bogus.DataSets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.EntityFrameworkCore;
using ZoneRV.DBContexts;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;
using ZoneRV.Services.Test.Models;


namespace ZoneRV.Services.Test;

public static class TestProductionServiceExtensions
{
    
    private static int _workspaceId          = 0;
    private static int _lineId               = 0;
    private static int _locationId           = 0;
    private static int _orderedLocationId    = 0;
    private static int _modelId              = 0;
    private static int _areaId               = 0;
    private static int _locationCustomNameId = 0;

    private static string[] _randomIcons = ["person", "group", "science", "architecture", "volcano", "school"];

    private static string[] _randomAreaNames =
    [
        "Chassis",
        "Electrical",
        "Cabinetry",
        "Walls",
        "QC",
        "Commissioning",
        "Sealing",
        "Welding",
        "Cabs",
        "Gas",
        "Paint"
    ];

    private static WorkspaceLocationSettings[] _workspaceSettings =

    [
        new WorkspaceLocationSettings("Chassis", "chassis bay", ProductionLocationType.Module),
        new WorkspaceLocationSettings("Bay 1",   null,          ProductionLocationType.Bay),
        new WorkspaceLocationSettings("Subs",    null,          ProductionLocationType.Subassembly),
        new WorkspaceLocationSettings("Bay 2",   null,          ProductionLocationType.Bay),
        new WorkspaceLocationSettings("Wall",    null,          ProductionLocationType.Module),
        new WorkspaceLocationSettings("Bay 3",   null,          ProductionLocationType.Bay),
        new WorkspaceLocationSettings("Bay 4",   null,          ProductionLocationType.Bay),
        new WorkspaceLocationSettings("Bay 5",   null,          ProductionLocationType.Bay),
        new WorkspaceLocationSettings("Dome",    null,          ProductionLocationType.Finishing)
    ];

    private static Dictionary<int, int> _workspaceBayNumber = [];
    
    public static IServiceCollection AddTestProductionService(this IServiceCollection serviceCollection, bool generateRandomWorkspaces = false, int? seed = null)
    {
        var faker = new Faker();
        
        if (seed is not null)
            faker.Random = new Randomizer((int)seed);
        
        var productionContext = new Mock<ProductionContext>();

        List<ProductionWorkspace> workspaces = [];
        
        workspaces.Add(
            GenerateWorkspace(
                faker, 
                "Zone RV", 
                "Test data roughly representing Zone",
                [
                    new LineSettings(
                        "Expo", 
                        "Line For Expo vans",
                        [
                            new AreaOfOriginSettings("Chassis",       null, null),
                            new AreaOfOriginSettings("Electrical",    null, null),
                            new AreaOfOriginSettings("Cabinetry",     null, null),
                            new AreaOfOriginSettings("Walls",         null, null),
                            new AreaOfOriginSettings("QC",            null, null),
                            new AreaOfOriginSettings("Commissioning", null, null),
                            new AreaOfOriginSettings("Sealing",       null, null),
                            new AreaOfOriginSettings("Welding",       null, null),
                            new AreaOfOriginSettings("Cabs",          null, null),
                            new AreaOfOriginSettings("Gas",           null, null),
                            new AreaOfOriginSettings("Paint",         null, null),
                        ],
                        [
                            new ModelSettings("Expo", "Expedition", "exp")
                        ]
                        ),
                    new LineSettings(
                        "Gen 2", 
                        "Gen 2 vans",
                        [
                            new AreaOfOriginSettings("Chassis", null, null),
                            new AreaOfOriginSettings("Electrical", null, null),
                            new AreaOfOriginSettings("Cabinetry", null, null),
                            new AreaOfOriginSettings("Walls", null, null),
                            new AreaOfOriginSettings("QC", null, null),
                            new AreaOfOriginSettings("Commissioning", null, null),
                            new AreaOfOriginSettings("Sealing", null, null),
                            new AreaOfOriginSettings("Welding", null, null),
                            new AreaOfOriginSettings("Cabs", null, null),
                            new AreaOfOriginSettings("Gas", null, null),
                            new AreaOfOriginSettings("Paint", null, null),
                        ],
                        [
                            new ModelSettings("Peregrine", "The little one", "zpp"),
                            new ModelSettings("Sojourn", "The slightly bigger one", "zsp"),
                            new ModelSettings("Summit", "The actual big one", "zss")
                        ]
                        )
                ],
                2));

        if (generateRandomWorkspaces)
        {
            for (int i = 0; i < 4; i++)
            {
                workspaces.Add(
                    GenerateWorkspace(
                        faker));
            }
        }
        
        productionContext.Setup(x => x.Workspaces).ReturnsDbSet(workspaces);
        productionContext.Setup(x => x.Lines).ReturnsDbSet(workspaces.SelectMany(x => x.Lines));
        productionContext.Setup(x => x.OrderedLineLocations).ReturnsDbSet(workspaces.SelectMany(x => x.WorkspaceLocations.SelectMany(y => y.OrderedLineLocations)));
        productionContext.Setup(x => x.AreaOfOrigin).ReturnsDbSet(workspaces.SelectMany(x => x.Lines.SelectMany(y => y.AreaOfOrigins)));
        productionContext.Setup(x => x.Models).ReturnsDbSet(workspaces.SelectMany(x => x.Lines.SelectMany(y => y.Models)));
    
        serviceCollection.AddScoped<ProductionContext>(_ => productionContext.Object);
        serviceCollection.AddSingleton<IProductionService, TestProductionService>(
            serviceProvider => new TestProductionService(
                serviceProvider.GetRequiredService<IServiceScopeFactory>(), 
                serviceProvider.GetRequiredService<IConfiguration>(),
                faker));

        return serviceCollection;
    }

    private static ProductionWorkspace GenerateWorkspace(
        Faker faker,
        string?                          name              = null, 
        string?                          description       = null, 
        List<LineSettings>?              lineSettings      = null, 
        int                              lineCount         = 2)
    {
        var workspace = new ProductionWorkspace()
        {
            Id = _workspaceId++,
            Name = name ?? faker.Company.CompanyName(),
            Description = description ?? faker.Company.CatchPhrase(),
            Lines = [],
            WorkspaceLocations = []
        };
        
        _workspaceBayNumber.Add(workspace.Id, 1);

        if (lineSettings is not null && lineSettings.Count > 0)
        {
            foreach (var line in lineSettings)
            {
                workspace.Lines.Add(GenerateLine(faker, workspace, line));
            }
        }
        else
        {
            for (int i = 0; i < lineCount; i++)
            {
                workspace.Lines.Add(GenerateLine(faker, workspace, new LineSettings(null, null)));
            }
        }

        foreach (var workspaceLocationSetting in _workspaceSettings)
        {
            var wLocation = GenerateLocation(faker, workspace, workspaceLocationSetting);

            foreach (var line in workspace.Lines)
            {
                wLocation.OrderedLineLocations.Add(GenerateOrderedLineLocation(faker, wLocation, line, null)); // todo fix random order
            }
            
            workspace.WorkspaceLocations.Add(wLocation);
        }

        return workspace;
    }


    private static ProductionLine GenerateLine(Faker faker, ProductionWorkspace workspace, LineSettings settings)
    {
        var line = new ProductionLine()
        {
            Id = _lineId++,
            Workspace = workspace,
            Name = settings.Name ?? faker.Vehicle.Type(),
            Description = settings.Description,
            OrderedLineLocations = [],
            AreaOfOrigins = [],
            Models = []
        };

        if (settings.AreaSettings is null || !settings.AreaSettings.Any())
        {
            var areaCount = faker.Random.Int(5, 10);
            var areaNames = faker.PickRandom(_randomAreaNames, areaCount).ToList();
            
            for (int i = 0; i < areaCount; i++)
            {
                line.AreaOfOrigins.Add(
                    GenerateArea(faker, line, new AreaOfOriginSettings(areaNames.ElementAt(i))));
            }
        }
        else
        {
            foreach (var areaSetting in settings.AreaSettings)
            {
                line.AreaOfOrigins.Add(
                    GenerateArea(faker, line, areaSetting));
            }
        }

        if (settings.ModelSettings is null || !settings.ModelSettings.Any())
        {
            for (int i = 0; i < faker.Random.Int(1, 3); i++)
            {
                line.Models.Add(
                    GenerateModel(faker, line, new ModelSettings()));
            }
        }
        else
        {
            foreach (var modelSetting in settings.ModelSettings)
            {
                line.Models.Add(
                    GenerateModel(faker, line, modelSetting));
            }
        }

        return line;
    }

    private static WorkspaceLocation GenerateLocation(Faker faker, ProductionWorkspace workspace, WorkspaceLocationSettings settings)
    {
        var type = faker.PickRandom(Enum.GetValues<ProductionLocationType>()); // todo better random locations
        
        return new WorkspaceLocation()
            {
                Id           = _locationId++,
                Name         = settings.Name ?? $"Bay {_workspaceBayNumber[workspace.Id]++}",
                Description  = settings.Description,
                Workspace    = workspace,
                LocationType = settings.LocationType ?? ProductionLocationType.Bay, 
                OrderedLineLocations = []
            };
    }

    private static OrderedLineLocation GenerateOrderedLineLocation(Faker faker, WorkspaceLocation location, ProductionLine line, decimal? order)
    {
        var newLocation =
            new OrderedLineLocation()
            {
                Id             = _orderedLocationId++,
                Line           = line,
                Location       = location,
                Order          =  location.Id + faker.Random.Decimal() * faker.Random.Int(0, 2),
                CustomNames    = [],
                InventoryNames = [] // TODO add once inventory is added
            };

        newLocation.CustomNames = faker.PickRandom(line.AreaOfOrigins, int.Min(faker.Random.Int(1, line.AreaOfOrigins.Count - 1), 2))
                                         .Select(x =>
                                                     new LocationCustomName()
                                                     {
                                                         Id = _locationCustomNameId++, CustomName = $"{location.Name}:{x.Name}", ServiceType = "test", LineLocation = newLocation
                                                     }).ToList();
        
        line.OrderedLineLocations.Add(newLocation);

        return newLocation;
    }

    private static AreaOfOrigin GenerateArea(Faker faker, ProductionLine line, AreaOfOriginSettings settings)
    {
        return new AreaOfOrigin()
        {
            Id = _areaId++,
            Name = settings.Name ?? faker.Commerce.Department(),
            Icon = settings.Icon ?? _randomIcons[faker.Random.Int(0, _randomIcons.Length - 1)], 
            Line = line
        };
    }

    private static Model GenerateModel(Faker faker, ProductionLine line, ModelSettings settings)
    {
        var modelName = settings.Name ?? faker.Vehicle.Model();
        
        return new Model()
        {
            Id          = _modelId++,
            Name        = modelName,
            Description = settings.Description ?? faker.Vehicle.Type(),
            Prefix      = settings.Prefix ?? new string(Regex.Replace(modelName, "[^a-zA-Z]", "").Take(3).ToArray()),
            Line        = line
        };
    }
    
}