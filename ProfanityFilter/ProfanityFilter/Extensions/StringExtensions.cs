using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProfanityFilter.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// From a given text finds all words without punctuation 
    /// </summary>
    /// <param name="text">The input text</param>
    /// <returns>A list of words in sentence</returns>
    internal static IEnumerable<string> ExtractWords(this string text)
    {
        var wordBuilder = new StringBuilder();
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var symbol in text)
        {
            if (symbol.IsWordsSeparator() && wordBuilder.Length == 0) continue;
            if (symbol.IsWordsSeparator() && wordBuilder.Length > 0)
            {
                yield return wordBuilder.ToString();
                wordBuilder.Clear();
            }
            else
            {
                wordBuilder.Append(symbol);
            }
        }

        yield return wordBuilder.ToString();
    }

    internal static string RemovePunctuation(this string input)
    {
        var findPunctuationRegex = new Regex(@"[^\w\s]");
        var noPunctuation = findPunctuationRegex.Replace(input, string.Empty);
        return noPunctuation;
    }

    /// <summary>
    /// Finds the list of indexes from input text by search string 
    /// </summary>
    /// <param name="input">The original text which where the method will search</param>
    /// <param name="searchString">The string which we want to find indexes of first latter in a given input</param>
    /// <returns>An indexes array of first symbol in search string</returns>
    internal static IEnumerable<int> FindFirstOccurrenceIndexes(this string input, string searchString)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));
        if (string.IsNullOrEmpty(searchString))
            throw new ArgumentNullException(nameof(searchString));

        var startSearchIndex = 0;

        while (startSearchIndex < input.Length)
        {
            var currentIndex = input.IndexOf(searchString, startSearchIndex, StringComparison.Ordinal);
            if (currentIndex == -1)
                break;

            yield return currentIndex;
            startSearchIndex = currentIndex + searchString.Length;
        }
    }
}