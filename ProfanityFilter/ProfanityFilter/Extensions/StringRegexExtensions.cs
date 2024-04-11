using System;
using System.Text.RegularExpressions;

namespace ProfanityFilter.Extensions;

internal static class StringRegexExtensions
{
    internal static bool IsValidRegex(this string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;
    
        try
        {
            Regex.Match("", pattern);
        }
        catch (ArgumentException)
        {
            return false;
        }
    
        return true;
    }
}