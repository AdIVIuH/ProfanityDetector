using System.Text.RegularExpressions;

namespace ProfanityFilter.Extensions;

internal static class CharExtensions
{
    internal static bool IsWordsSeparator(this char symbol)
    {
        var regex = new Regex(RegexPatterns.WordsSeporatorsPattern);

        return regex.IsMatch(symbol.ToString());
    }
}