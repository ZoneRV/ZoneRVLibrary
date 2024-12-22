using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ZoneRV;

public static class Utils
{
    private static readonly Regex VanRegex;
    public static readonly List<string> NumberFormats = [@"\d\d\dr", @"\d\d\d", @"sr\d"];
    
        
    static Utils()
    {
        var models = Enum.GetValues<VanModel>().Select(x => Enum.GetName(x)!.ToLower());
        
        string vanRegexPattern 
            = @"(?:\b(?=\w)|\()(" + string.Join('|', models) + @")[-.\s]?(" + string.Join('|', NumberFormats) + @")(?:\b(?<=\w)|\))";
        
        VanRegex = new Regex(vanRegexPattern, RegexOptions.Compiled);
    }
   
    /// <exception cref="ArgumentException">
    ///     Throws exception if input string contains multiple distinct van names.
    ///     <see cref="GetAllMentionedVans">Use this for strings with multiple vans.</see>
    /// </exception>
    public static bool TryGetVanName(
        string input, 
        [NotNullWhen(true)] out VanModel? vanType, 
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
            throw new ArgumentException($"Van name \"{input}\" contains multiple van names", nameof(input));
        }
        
        result = string.Join(null, matches[0].Groups.Values.Skip(1));

        vanType = Enum.GetValues<VanModel>()
            .First(x => Enum.GetName(x)!.ToLower() == matches[0].Groups[1].Value);
        
        return true; 
    }

    public static IEnumerable<string> GetAllMentionedVans(string input)
    {
        input = input.ToLower();
        List<string> results = [];
        
        MatchCollection matches = VanRegex.Matches(input);

        foreach (var match in matches.Select(x => string.Join(null, x.Groups.Values.Skip(1))).Distinct() )
        {
            results.Add(match);
        }

        return results;
    }
}