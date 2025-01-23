using ZoneRV.Models;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests;

public class VanNameRegexTests
{
    private ModelNameMatcher _nameMatcher;

    public VanNameRegexTests()
    {
        _nameMatcher = new ModelNameMatcher(ProductionTestData.VanModels);
    }
    
    [Theory]
    [InlineData("ZpP-145", "zpp", "zpp145")]
    [InlineData("ZpP145-", "zpp", "zpp145")]
    [InlineData("ZpP.145", "zpp", "zpp145")]
    [InlineData("ZpP:145", "zpp", "zpp145")]
    [InlineData(":ZpP145:", "zpp", "zpp145")]
    [InlineData("ZpP 145", "zpp", "zpp145")]
    [InlineData(" ZpP145 ", "zpp", "zpp145")]
    [InlineData("(exp010)", "exp", "exp010")]
    [InlineData("(exp010", "exp", "exp010")]
    [InlineData("exp010)", "exp", "exp010")]
    [InlineData("zss100 and zss100 should still return zss100", "zss", "zss100")]
    [InlineData("this is zpp100r, so amazing we just had to build it twice", "zpp", "zpp100r")]
    public void ShouldReturnVanName(string input, string modelPrefix, string expectedVanName)
    {
        var pass = _nameMatcher.TryGetSingleVanName(input, out Model? vanModel, out string? vanNameResult);
        
        Assert.True(pass);
        
        Assert.NotNull(vanModel);
        Assert.Equal(modelPrefix, vanModel.Prefix);
        
        Assert.NotNull(vanNameResult);
        Assert.Equal(expectedVanName, vanNameResult.ToLower());
    }
    
    [Theory]
    [InlineData("zsp100 and zpp401")]
    public void ShouldReturnFalse(string input)
    {
        Assert.False(_nameMatcher.TryGetSingleVanName(input, out _, out _));
    }

    [Theory]
    [InlineData("zpp100 and zsp500, dont forget exp091", "zpp100", "zsp500", "exp091")]
    [InlineData("zpp100 and zpp100r, are not the same van", "zpp100", "zpp100r")]
    [InlineData("zpp110 and zpp110, are the same van", "zpp110")]
    public void ShouldFindMultipleNames(string input, params string[] expectedResults)
    {
        var results = _nameMatcher.GetAllMentionedVans(input);
        
        Assert.Equal(expectedResults, results.ToArray());
    }
}