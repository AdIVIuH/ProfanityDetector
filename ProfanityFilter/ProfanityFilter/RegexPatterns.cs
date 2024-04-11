using JetBrains.Annotations;

namespace ProfanityFilter;

public static class RegexPatterns
{
    /// <summary>
    /// Pattern to find all possible separators
    /// </summary>
    /// <see cref="T:System.Text.RegularExpressions.Regex" />
    /// <see href="https://learn.microsoft.com/en-gb/dotnet/standard/base-types/character-classes-in-regular-expressions#supported-unicode-general-categories">Supported Unicode general categories</see>
    /// <para> <c>\p{}</c> - Unicode block </para>
    /// <para> P - All punctuation characters. This includes the Pc, Pd, Ps, Pe, Pi, Pf, and Po categories. </para>
    /// <para> Z - All separator characters. This includes the Zs, Zl, and Zp categories. </para>
    /// <para> C - All other characters. This includes the Cc, Cf, Cs, Co, and Cn categories </para>
    /// <para> M - All combining marks. This includes the Mn, Mc, and Me categories. </para>
    /// <para> Sk - Symbol, Modifier</para>
    [RegexPattern] 
    internal const string WordsSeparatorsPattern = @"[\p{Pc}\p{Pd}\p{Ps}\p{Pe}\p{Pi}\p{Pf}\p{Po}\p{Z}\p{C}\p{M}\p{Sk}]+";
    
    [RegexPattern] 
    internal const string WordsWithConnectorsPattern = @"[^\p{Pc}\p{Pd}\p{Ps}\p{Pe}\p{Pi}\p{Pf}\p{Po}\p{Z}\p{C}\p{M}\p{Sk}]+";
}