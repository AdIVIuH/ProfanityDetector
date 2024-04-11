using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProfanityFilter.Models;

namespace ProfanityFilter.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// From a given text finds all words without punctuation 
    /// </summary>
    /// <param name="text">The input text</param>
    /// <returns>A list of words in sentence</returns>
    internal static IEnumerable<CompleteWord> ExtractWords(this string text)
    {
        var regexMatches = Regex.Matches(text, RegexPatterns.WordsWithConnectorsPattern);

        return regexMatches.Select(m => new CompleteWord(
            StartWordIndex: m.Index,
            EndWordIndex: m.Index + m.Length,
            WholeWord: m.Value)
        );
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

    internal static int FindStartWordIndex(this string wordPart, int pointerIndex)
    {
        var startIndex = pointerIndex;
        while (startIndex > 0)
        {
            if (wordPart[startIndex - 1].IsWordsSeparator()) break;

            startIndex -= 1;
        }

        return startIndex;
    }

    internal static int FindEndWordIndex(this string sentence, int pointerIndex)
    {
        var endIndex = pointerIndex;
        while (endIndex < sentence.Length)
        {
            if (sentence[endIndex].IsWordsSeparator()) break;

            endIndex += 1;
        }

        return endIndex;
    }

    internal static string ReplaceHomoglyphs(this string word)
    {
        // TODO not implemented yet: Homoglyphs }|{ -> ж
        return word;
    }
    
    /// <summary>
    /// Вернет только буквы в слове
    /// </summary>
    /// <param name="word"></param>
    /// <returns></returns>
    internal static string SelectOnlyLetters(this string word)
    {
        var isContainsLettersRegex = new Regex(RegexPatterns.OnlyLettersPattern);
        var match = isContainsLettersRegex.Match(word);
        return match.Success
            ? match.Value
            : word;
    }
}