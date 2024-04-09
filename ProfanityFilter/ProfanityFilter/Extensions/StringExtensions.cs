using System;
using System.Collections.Generic;
using System.Text;

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
        for (var i = 0; i < text.Length; i++)
        {
            var symbol = text[i];
            var isLastSymbol = i == text.Length - 1;
            
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

            if (isLastSymbol && wordBuilder.Length > 0)
                yield return wordBuilder.ToString();
        }
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