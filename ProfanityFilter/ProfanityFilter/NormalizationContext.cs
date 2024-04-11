using System;
using System.Collections.Generic;
using System.Globalization;
using ProfanityFilter.Extensions;
using ProfanityFilter.Models;

namespace ProfanityFilter;

internal class NormalizationContext
{
    public NormalizationContext(string input)
    {
        Original = input;
        ExtractedWords = new Lazy<IEnumerable<CompleteWord>>(() => ExtractWords(input));
        Value = new Lazy<string>(GetValue);
    }

    public Lazy<string> Value { get; }
    public string Original { get; }
    public Lazy<IEnumerable<CompleteWord>> ExtractedWords { get; }

    protected IEnumerable<CompleteWord> ExtractWords(string input) =>
        input.ExtractWords();

    private string GetValue() =>
        string.Join(' ', ExtractedWords.Value)
            .Trim()
            .ToLower(CultureInfo.InvariantCulture);
}