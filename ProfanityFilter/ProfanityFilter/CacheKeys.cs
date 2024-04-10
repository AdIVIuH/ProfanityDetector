using ProfanityFilter.Extensions;

namespace ProfanityFilter;

internal static class CacheKeys
{
    public static string GetKeyForExtractWords(string input)
        => $"{nameof(StringExtensions.ExtractWords)}:{input}";
    
    public static string GetKeyForNormalizeInput(string input)
        => $"{nameof(ProfanityBase.NormalizeInput)}:{input}";
}