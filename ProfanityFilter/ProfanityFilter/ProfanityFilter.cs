/*
MIT License
Copyright (c) 2019
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProfanityFilter.Extensions;
using ProfanityFilter.Models;

namespace ProfanityFilter;

/// <summary>
///
/// This class will detect profanity and racial slurs contained within some text and return an indication flag.
/// All words are treated as case insensitive.
///
/// </summary>
public class ProfanityFilter : ProfanityBase
{
    public const char DefaultCensorString = '*';

    /// <summary>
    /// Default constructor that loads up the default profanity list.
    /// </summary>
    public ProfanityFilter()
    {
        AllowList = new AllowList();
    }

    /// <summary>
    /// Return the allow list;
    /// </summary>
    public AllowList AllowList { get; }

    /// <summary>
    /// For a given sentence, return a list of all the detected profanities.
    /// </summary>
    /// <param name="sentence">The sentence to check for profanities.</param>
    /// <param name="removePartialMatches">Remove duplicate partial matches.</param>
    /// <returns>A read only list of detected profanities.</returns>
    public IReadOnlyList<string> DetectAllProfanities(string sentence, bool removePartialMatches = false)
    {
        if (string.IsNullOrEmpty(sentence))
            return new List<string>().AsReadOnly();

        var normalizedInput = NormalizeInput(sentence);
        var matchedProfanities = GetMatchedProfanities(
            normalizedInput,
            includePartialMatch: !removePartialMatches,
            includePatterns: true);
        var excludedAllowList = FilterByAllowList(matchedProfanities);
        var censorResult = CensorStringByProfanityList(sentence, excludedAllowList,
            censorCharacter: DefaultCensorString,
            ignoreNumbers: true);

        return censorResult.AppliedProfanities;
    }

    /// <summary>
    /// For any given string, censor any profanities from the list using the specified
    /// censoring character.
    /// </summary>
    /// <param name="sentence">The string to censor.</param>
    /// <param name="censorCharacter">The character to use for censoring.</param>
    /// <param name="ignoreNumbers">Ignore any numbers that appear in a word.</param>
    /// <returns></returns>
    public string CensorString(string sentence, char censorCharacter = DefaultCensorString,
        bool ignoreNumbers = false)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        if (string.IsNullOrEmpty(sentence) || string.IsNullOrWhiteSpace(sentence))
            return sentence;

        var profanities = DetectAllProfanities(sentence, removePartialMatches: false);
        var censorResult = CensorStringByProfanityList(sentence, profanities, censorCharacter, ignoreNumbers);
        return censorResult.CensoredSentence;
    }

    /// <summary>
    /// Check whether a given pattern matches an entry in the profanity list. <see cref="HasProfanityByPattern"/> will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="pattern">Pattern to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public override bool HasProfanityByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;
        var normalizedInput = NormalizeInput(pattern);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;

        return base.HasProfanityByPattern(normalizedInput);
    }

    /// <summary>
    /// Check whether a given term matches an entry in the profanity list. <see cref="HasProfanityByTerm"/> will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="term">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public override bool HasProfanityByTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return false;

        var normalizedInput = NormalizeInput(term);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;

        return base.HasProfanityByTerm(normalizedInput);
    }

    /// <summary>
    /// Check whether a given input matches an entry in the profanity list. 
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public bool HasAnyProfanities(string input) =>
        HasProfanityByTerm(input) || HasProfanityByPattern(input);

    /// <summary>
    /// Check whether a given input matches an entry in the profanity list. 
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <param name="profanity">Profanity to check</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    private bool HasProfanity(string input, string profanity) =>
        HasProfanityByTerm(input) || HasProfanityByPattern(input, profanity);

    /// <summary>
    /// For a given sentence, look for the specified profanity. If it is found, look to see
    /// if it is part of a containing word. If it is, then return the containing work and the start
    /// and end positions of that word in the string.
    ///
    /// For example, if the string contains "scunthorpe" and the passed in profanity is "cunt",
    /// then this method will find "cunt" and work out that it is part of an enclosed word.
    /// </summary>
    /// <param name="toCheck">Sentence to check.</param>
    /// <param name="profanity">Profanity to look for.</param>
    /// <returns>Tuple of the following format (start character, end character, found enclosed word).
    /// If no enclosed word is found then return null.</returns>
    private static IEnumerable<CompleteWord> GetCompleteWords(
        string toCheck,
        string profanity)
    {
        if (string.IsNullOrEmpty(toCheck)) yield break;

        var words = toCheck.ExtractWords();
        var handledWords = new HashSet<string>();

        foreach (var word in words)
        {
            var normalizedWord = NormalizeInput(word);
            if (!handledWords.Add(normalizedWord)) continue;

            var isMatched = Regex.IsMatch(normalizedWord, profanity);
            if (!isMatched) continue;
            var indexes = toCheck.FindFirstOccurrenceIndexes(word);
            foreach (var j in indexes)
            {
                var startWordIndex = FindStartWordIndex(toCheck, j);
                var endWordIndex = FindEndWordIndex(toCheck, j);
                var wholeWordLength = endWordIndex - startWordIndex;
                var wholeWord = toCheck.Substring(startWordIndex, wholeWordLength);

                yield return new CompleteWord(
                    StartWordIndex: startWordIndex,
                    EndWordIndex: endWordIndex,
                    WholeWord: wholeWord);
            }
        }
    }

    private static int FindStartWordIndex(string wordPart, int pointerIndex)
    {
        var startIndex = pointerIndex;
        while (startIndex > 0)
        {
            if (wordPart[startIndex - 1].IsWordsSeparator()) break;

            startIndex -= 1;
        }

        return startIndex;
    }

    private static int FindEndWordIndex(string sentence, int pointerIndex)
    {
        var endIndex = pointerIndex;
        while (endIndex < sentence.Length)
        {
            if (sentence[endIndex].IsWordsSeparator()) break;

            endIndex += 1;
        }

        return endIndex;
    }

    private CensorProfanityResult CensorStringByProfanityList(string sentence,
        IEnumerable<string> profanities,
        char censorCharacter = DefaultCensorString,
        bool ignoreNumbers = false)
    {
        var appliedProfanitiesResult = new List<string>();
        var censored = sentence;

        foreach (var profanity in profanities.OrderByDescending(x => x.Length))
        {
            var (censoredSentence, appliedProfanities) =
                CensorProfanity(censored, profanity, censorCharacter, ignoreNumbers);
            censored = censoredSentence;
            appliedProfanitiesResult.AddRange(appliedProfanities);
        }

        var distinctProfanities = appliedProfanitiesResult
            .Distinct()
            .ToList()
            .AsReadOnly();

        return new CensorProfanityResult(censored, distinctProfanities);
    }

    private CensorProfanityResult CensorProfanity(string sentence, string profanity, char censorCharacter,
        bool ignoreNumbers = false)
    {
        var censored = sentence;
        var filteredProfanityList = new List<string>();
        var profanityParts = profanity.Split(' ');

        if (profanityParts.Length == 1)
        {
            var (censoredSentence, appliedProfanities) =
                CensorBySingleWordProfanity(censored, profanity, censorCharacter, ignoreNumbers);
            censored = censoredSentence;
            filteredProfanityList.AddRange(appliedProfanities);
        }
        else
        {
            filteredProfanityList.Add(profanity);
            censored = censored.Replace(profanity, CreateCensoredString(profanity, censorCharacter));
        }

        return new CensorProfanityResult(censored, filteredProfanityList.AsReadOnly());
    }

    private CensorProfanityResult CensorBySingleWordProfanity(string sentence, string profanity,
        char censorCharacter,
        bool ignoreNumbers = false)
    {
        var censored = new StringBuilder(sentence);
        var appliedProfanities = new HashSet<string>();
        var findNumbersRegex = new Regex(@"[\d-]");

        foreach (var result in GetCompleteWords(toCheck: censored.ToString(), profanity))
        {
            var (startWordIndex, endWordIndex, wholeWord) = result;
            var filteredWord = wholeWord;
            if (ignoreNumbers)
                filteredWord = findNumbersRegex.Replace(wholeWord, string.Empty);

            if (!HasProfanity(filteredWord, profanity)) continue;

            appliedProfanities.Add(profanity);
            for (var i = startWordIndex; i < endWordIndex; i++)
                censored[i] = censorCharacter;
        }

        return new CensorProfanityResult(censored.ToString(), appliedProfanities.ToList().AsReadOnly());
    }

    private IReadOnlyList<string> FilterByAllowList(IEnumerable<string> profanities) =>
        profanities.Where(word => !AllowList.Contains(word))
            .ToList()
            .AsReadOnly();

    private static string CreateCensoredString(string word, char censorCharacter)
    {
        var censoredWordBuilder = new StringBuilder();

        foreach (var t in word)
            censoredWordBuilder.Append(t.IsSeparatorOrPunctuation() ? t : censorCharacter);

        return censoredWordBuilder.ToString();
    }
}