#nullable enable
using System;
using System.Globalization;
using ProfanityFilter.Extensions;

namespace ProfanityFilter.Models;

internal class WordInSentence
{
    private readonly Lazy<string> _normalizedWord;

    public WordInSentence(int startIndex,
        string originalWord)
    {
        StartIndex = startIndex;
        OriginalWord = originalWord;
        _normalizedWord = new Lazy<string>(Normalize);
    }

    public int StartIndex { get; }
    public string OriginalWord { get; }
    public string NormalizedWord => _normalizedWord.Value;

    private string Normalize() => 
        OriginalWord
            .ToLower(CultureInfo.InvariantCulture)
            .ReplaceHomoglyphs()
            .SelectOnlyLetters()
            .Trim();
}