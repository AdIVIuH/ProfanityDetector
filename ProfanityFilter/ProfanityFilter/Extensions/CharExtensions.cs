using System;
using System.Globalization;

namespace ProfanityFilter.Extensions;

internal static class CharExtensions
{
    internal static bool IsSeparatorOrPunctuation(this char symbol) =>
        char.IsSeparator(symbol) || char.IsPunctuation(symbol);

    internal static bool IsWordsSeparator(this char symbol)
    {
        if (symbol.IsSeparatorOrPunctuation())
            return true;
        
        var unicodeCategory = char.GetUnicodeCategory(symbol);
        
        switch (unicodeCategory)
        {
            case UnicodeCategory.SpaceSeparator:
            case UnicodeCategory.LineSeparator:
            case UnicodeCategory.ParagraphSeparator:
            case UnicodeCategory.Control:
            case UnicodeCategory.Surrogate:
            case UnicodeCategory.DashPunctuation:
            case UnicodeCategory.OpenPunctuation:
            case UnicodeCategory.ClosePunctuation:
            case UnicodeCategory.InitialQuotePunctuation:
            case UnicodeCategory.FinalQuotePunctuation:
            case UnicodeCategory.OtherPunctuation:
            case UnicodeCategory.ModifierSymbol:
            case UnicodeCategory.OtherNotAssigned:
                return true;
            default:
                return false;
        }
    }
}