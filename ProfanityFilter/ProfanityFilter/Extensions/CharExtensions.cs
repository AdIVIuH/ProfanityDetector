using System.Globalization;

namespace ProfanityFilter.Extensions;

internal static class CharExtensions
{
    internal static bool IsSeparatorOrPunctuation(this char symbol) =>
        char.IsSeparator(symbol) || char.IsPunctuation(symbol);

    internal static bool IsWordsSeparator(this char symbol) =>
        symbol.IsSeparatorOrPunctuation() || char.GetUnicodeCategory(symbol) == UnicodeCategory.ModifierSymbol;
}