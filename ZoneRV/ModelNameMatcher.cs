using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Serilog;

namespace ZoneRV;

public class ModelNameMatcher
{
    private Regex VanRegex { get; init; }
    public readonly IReadOnlyList<string> NumberFormats = [@"\d\d\dr", @"\d\d\d", @"sr\d"];
    public List<Model> Models;
    
    public ModelNameMatcher(IEnumerable<Model> models)
    {
        Models = models.ToList();
                
        string vanRegexPattern 
            = @"(?:\b(?=\w)|\(|-|:)(" + string.Join('|', Models.Select(x => x.Prefix.ToLower()))  + @")[-.\s-:]?(" + string.Join('|', NumberFormats) + @")(?:\b(?<=\w)|\)|-|:)";
        
        VanRegex = new Regex(vanRegexPattern, RegexOptions.Compiled);
    }
    
    /// <exception cref="ArgumentException">
    ///     Throws exception if input string contains multiple distinct van names.
    ///     <see cref="GetAllMentionedVans">Use this for strings with multiple vans.</see>
    /// </exception>
    public bool TryGetSingleVanName(
        string input,
        [NotNullWhen(true)] out Model? vanType, 
        [NotNullWhen(true)] out string? result)
    {
        input = input.ToLower();
        
        vanType = null;
        result = null;
        MatchCollection matches = VanRegex.Matches(input);
        
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Count > 1 && matches.DistinctBy(x => string.Join(null, x.Groups.Values.Skip(1))).Count() != 1)
        {
            Log.Logger.Warning("Van name {input} contains multiple van names", input);
            return false;
        }
        
        result = string.Join(null, matches[0].Groups.Values.Skip(1));

        vanType = Models
            .First(x => x.Prefix.ToLower() == matches[0].Groups[1].Value);
        
        return true; 
    }

    public IEnumerable<string> GetAllMentionedVans(string input)
    {
        input = input.ToLower();
        List<string> results = [];
        
        MatchCollection matches = VanRegex.Matches(input);

        foreach (var match in matches.Select(x => string.Join(null, x.Groups.Values.Skip(1))).Distinct())
        {
            results.Add(match);
        }

        return results;
    }
}