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
    public bool TryGetSingleName(
        string input,
        [NotNullWhen(true)] out Model? model, 
        [NotNullWhen(true)] out string? number, 
        [NotNullWhen(true)] out string? result)
    {
        input = input.ToLower();
        
        model  = null;
        result = null;
        number = null;
        
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

        model = Models
            .First(x => x.Prefix.ToLower() == matches[0].Groups[1].Value);

        number = matches[0].Groups[2].Value;

        result = (model.Prefix + number).ToLower();
        
        return true; 
    }

    public bool TryGetSingleName(string input, [NotNullWhen(true)] out string? result)
        => TryGetSingleName(input, out _, out _, out result);

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