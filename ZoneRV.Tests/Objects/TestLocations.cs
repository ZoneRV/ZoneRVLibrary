using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;

namespace ZoneRV.Tests.Objects;

public static class TestLocations
{
    public static LocationFactory LocationFactory;

    static TestLocations()
    {
        LocationFactory = new LocationFactory();

        LocationFactory.CreateModuleLocation("Gen2 chassis", "Chassis are build here", 0M, ProductionLine.Gen2, null);
        LocationFactory.CreateModuleLocation("Expo chassis", "Chassis are build here", 0M, ProductionLine.Expo, null);
        LocationFactory.CreateModuleLocation("Cabs", "Cabs are build here", 0.5m, null, null);
        LocationFactory.CreateModuleLocation("Subs", "stuffs are build here", .25M, null, null);
        LocationFactory.CreateModuleLocation("Gen2 Wall Mod", "Wall are build here", 3.5M, ProductionLine.Gen2, null);
        LocationFactory.CreateModuleLocation("Expo Wall Mod", "Wall are build here", 1.5M, ProductionLine.Expo, null);
        
        LocationFactory.CreateGen2BayLocation("Bay 1", "Bay 1 of gen 2", 1, null);
        LocationFactory.CreateGen2BayLocation("Bay 2", "Bay 2 of gen 2", 2, null);
        LocationFactory.CreateGen2BayLocation("Bay 3", "Bay 3 of gen 2", 3, null);
        LocationFactory.CreateGen2BayLocation("Bay 4", "Bay 4 of gen 2", 4, null);
        LocationFactory.CreateGen2BayLocation("Bay 5", "Bay 5 of gen 2", 5, null);
        LocationFactory.CreateGen2BayLocation("Bay 6", "Bay 6 of gen 2", 6, null);
        LocationFactory.CreateGen2BayLocation("Bay 7", "Bay 7 of gen 2", 7, null);
        
        LocationFactory.CreateExpoBayLocation("Bay 1", "Bay 1 of expo", 1, null);
        LocationFactory.CreateExpoBayLocation("Bay 2", "Bay 2 of expo", 2, null);
        LocationFactory.CreateExpoBayLocation("Bay 3", "Bay 3 of expo", 3, null);
        LocationFactory.CreateExpoBayLocation("Bay 4", "Bay 4 of expo", 4, null);
        LocationFactory.CreateExpoBayLocation("Bay 5", "Bay 5 of expo", 5, null);
    }
}