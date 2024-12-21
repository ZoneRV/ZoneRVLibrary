using ZoneRV.Models.Enums;
using ZoneRV.Models.ProductionPosition;
using ZoneRV.Tests.TestObjects;

namespace ZoneRV.Tests;

public class ProductionPositionTests
{
    [Fact]
    public void PositionOperatorTests()
    {
        Assert.True(ProductionPositions.g1Pos < ProductionPositions.g2Pos);
        Assert.True(ProductionPositions.g1Pos < ProductionPositions.g4Pos);
        
        Assert.True(ProductionPositions.g4Pos > ProductionPositions.g2Pos);
        Assert.True(ProductionPositions.g4Pos > ProductionPositions.g3Pos);
        
        Assert.True(ProductionPositions.g1Pos == ProductionPositions.g1PosIdentical);
        
        Assert.True(ProductionPositions.g1Pos != ProductionPositions.g3Pos);
        Assert.True(ProductionPositions.g4Pos != ProductionPositions.g3Pos);
    }

    [Fact]
    public void PositionEqualityTests()
    {
        Assert.Equal(ProductionPositions.g1Pos, ProductionPositions.g1Pos);
        Assert.Equal(ProductionPositions.g3Pos, ProductionPositions.g3Pos);
        
        Assert.NotEqual(ProductionPositions.g1Pos, ProductionPositions.g3Pos);
        Assert.NotEqual(ProductionPositions.g4Pos, ProductionPositions.g3Pos);
    }
}