using ProfanityFilter.Extensions;

namespace ProfanityFilter;

internal static class CacheKeys
{
    public static string GetKeyForExtractWords(string input)
        => $"{nameof(StringExtensions.ExtractWords)}:{input.GetHashCode()}";
    
    public static string GetKeyForNormalizeInput(string input)
        => $"{nameof(ProfanityBase.NormalizeInput)}:{input.GetHashCode()}";
}