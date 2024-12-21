using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using ZoneRV.Models;
using ZoneRV.Models.Enums;

namespace ZoneRV.Tests;

public class VanNameRegexTests
{
    [Fact]
    public void AllModelsReturnVanName()
    {
        List<(VanModel model, string input, string output)> vanNames 
            = Enum.GetValues<VanModel>().Select(x => 
                (
                    x, 
                    $"{x}100".ToLower(), 
                    $"{x}100".ToLower()
                ) 
            ).ToList();

        foreach (var name in vanNames)
        {
            ShouldReturnVanName(name.input, name.model, name.output);
        }
    }
    
    [Fact]
    public void AllNumberFormatsReturnVanName()
    {
        List<(VanModel model, string input, string output)> vanNames 
            = Utils.NumberFormats.Select(x => 
                (
                    VanModel.Zpp, 
                    $"zpp{Regex.Replace(x, @"\\d", "5")}".ToLower(), 
                    $"zpp{Regex.Replace(x, @"\\d", "5")}".ToLower()
                ) 
            ).ToList();

        foreach (var name in vanNames)
        {
            ShouldReturnVanName(name.input, name.model, name.output);
        }
    }
    
    [Theory]
    [InlineData("ZpP-145", VanModel.Zpp, "zpp145")]
    [InlineData("ZpP.145", VanModel.Zpp, "zpp145")]
    [InlineData("ZpP 145", VanModel.Zpp, "zpp145")]
    [InlineData(" ZpP145 ", VanModel.Zpp, "zpp145")]
    [InlineData("(exp010)", VanModel.Exp, "exp010")]
    [InlineData("(exp010", VanModel.Exp, "exp010")]
    [InlineData("exp010)", VanModel.Exp, "exp010")]
    [InlineData("zss100 and zss100 should still return zss100", VanModel.Zss, "zss100")]
    [InlineData("this is zpp100r, so amazing we just had to build it twice", VanModel.Zpp, "zpp100r")]
    public void ShouldReturnVanName(string input, VanModel model, string expectedVanName)
    {
        var pass = Utils.TryGetVanName(input, out VanModel? vanModel, out string? vanNameResult);
        
        Assert.True(pass);
        
        Assert.NotNull(vanModel);
        Assert.Equal(model, vanModel);
        
        Assert.NotNull(vanNameResult);
        Assert.Equal(expectedVanName, vanNameResult.ToLower());
    }
    
    [Theory]
    [InlineData("zsp100 and zpp401")]
    public void ShouldThrow(string input)
    {
        Assert.Throws<ArgumentException>(() => Utils.TryGetVanName(input, out _, out _));
    }

    [Theory]
    [InlineData("zpp100 and zsp500, dont forget exp091", "zpp100", "zsp500", "exp091")]
    [InlineData("zpp100 and zpp100r, are the same van", "zpp100")]
    public void ShouldFindMultipleNames(string input, params string[] expectedResults)
    {
        var results = Utils.GetAllMentionedVans(input);
        
        Assert.Equivalent(expectedResults, results);
    }
}