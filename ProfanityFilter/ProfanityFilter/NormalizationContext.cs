using System;
using System.Collections.Generic;
using System.Globalization;
using ProfanityFilter.Extensions;

namespace ProfanityFilter;

internal class NormalizationContext
{
    public NormalizationContext(string input)
    {
        Original = input;
        ExtractedWords = new Lazy<IEnumerable<string>>(() => ExtractWords(input));
        Value = new Lazy<string>(GetValue);
    }

    public Lazy<string> Value { get; }
    public string Original { get; }
    public Lazy<IEnumerable<string>> ExtractedWords { get; }

    protected IEnumerable<string> ExtractWords(string input) =>
        input.ExtractWords();

    private string GetValue() =>
        string.Join(' ', ExtractedWords.Value)
            .Trim()
            .ToLower(CultureInfo.InvariantCulture);
}