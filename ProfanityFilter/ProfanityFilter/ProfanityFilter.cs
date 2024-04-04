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
    /// TODO rewrite it
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
        // var words = ExtractWordsFromSentence(normalizedInput);
        // var postAllowList = FilterWordListByAllowList(words);

        // Catch whether multi-word profanities are in the allow list filtered sentence.
        // var wordsOnlySentence = string.Join(' ', postAllowList);
        var matchedProfanities = GetMatchedProfanities(
            normalizedInput,
            includePartialMatch: !removePartialMatches,
            includePatterns: true);
        var excludedAllowList = FilterProfanitiesByAllowList(matchedProfanities);
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
        // if (string.IsNullOrEmpty(sentence))
        //     return string.Empty;
        //
        // var normalizedInput = NormalizeInput(sentence);
        // var words = ExtractWordsFromSentence(normalizedInput);
        // var postAllowList = FilterProfanitiesByAllowList(words);
        //
        // // Catch whether multi-word profanities are in the allow list filtered sentence.
        // var wordsOnlySentence = string.Join(' ', postAllowList);
        // var profanities = GetMatchedProfanities(wordsOnlySentence, includePartialMatch: true, includePatterns: true);
        //
        // return CensorStringByProfanityList(sentence, censorCharacter, profanities, ignoreNumbers);
        if (string.IsNullOrEmpty(sentence))
            return string.Empty;

        var profanities = DetectAllProfanities(sentence, removePartialMatches: false);
        var censorResult = CensorStringByProfanityList(sentence, profanities, censorCharacter, ignoreNumbers);
        return censorResult.CensoredSentence;
    }

    private static string[] ExtractWordsFromSentence(string sentence)
    {
        // TODO сделать полный список!!
        return sentence.Split(new[] { ' ', '.', ',', '!', '?', '\'', '\"' }, StringSplitOptions.RemoveEmptyEntries);
    }

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
    private static IEnumerable<(int StartWordIndex, int EndWordIndex, string WholeWord)> GetCompleteWords(
        string toCheck,
        string profanity)
    {
        if (string.IsNullOrEmpty(toCheck)) yield break;

        var profanityLowerCase = profanity.ToLower(CultureInfo.InvariantCulture);
        var toCheckArray = ExtractWordsFromSentence(toCheck);

        // !!!Кристина!!! сделала заказ 5
        // ******** сделала заказ 5

        // normalizedInputN.Length == toCheckN.Length
        // "выдать, ЛоXу заказ ЛоXу,"

        // "выдать ЛоXу заказ ЛоXу"
        // "Выдать , авыаылoхувпфыорв заказ():?*: @@@л0}{у@@@"
        // "Выдать, **************** заказ():?*: *****"
        var handledWords = new HashSet<string>();
        foreach (var word in toCheckArray)
        {
            var normalizedWord = NormalizeInput(word);
            if (handledWords.Contains(normalizedWord)) continue;

            var isMatched = Regex.IsMatch(normalizedWord, profanityLowerCase);
            if (!isMatched) continue;

            handledWords.Add(normalizedWord);
            var indexes = FindFirstOccurrenceIndexes(toCheck, word);
            foreach (var j in indexes)
                yield return (j, j + word.Length, word);
        }
    }

    private static int[] FindFirstOccurrenceIndexes(string input, string searchString)
    {
        var indexes = new List<int>();
        var startSearchIndex = 0;

        while (startSearchIndex < input.Length)
        {
            var currentIndex = input.IndexOf(searchString, startSearchIndex, StringComparison.Ordinal);
            if (currentIndex == -1)
                break;

            indexes.Add(currentIndex);
            startSearchIndex = currentIndex + searchString.Length;
        }

        return indexes.ToArray();
    }

    private static int FindStartWordIndex(string sentence, int pointerIndex)
    {
        var startIndex = pointerIndex;
        while (startIndex > 0)
        {
            if (IsEmptyOrPunctuation(sentence[startIndex - 1])) break;

            startIndex -= 1;
        }

        return startIndex;
    }

    private static int FindEndWordIndex(string sentence, int pointerIndex)
    {
        var endIndex = pointerIndex;
        while (endIndex < sentence.Length)
        {
            if (IsEmptyOrPunctuation(sentence[endIndex])) break;

            endIndex += 1;
        }

        return endIndex;
    }

    private static bool IsEmptyOrPunctuation(char termSymbol) =>
        termSymbol == ' ' || char.IsPunctuation(termSymbol);

    /// <summary>
    /// Check whether a given term matches an entry in the profanity list. ContainsProfanity will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="pattern">Pattern to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public override bool IsMatchedByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;
        var normalizedInput = NormalizeInput(pattern);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;

        return base.IsMatchedByPattern(normalizedInput);
    }

    public override bool IsMatchedByWord(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalizedInput = NormalizeInput(input);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;

        return base.IsMatchedByWord(normalizedInput);
    }

    public bool IsMatched(string input, string profanity)
    {
        return IsMatchedByWord(input) || IsMatchedByPattern(input, profanity);
    }

    public bool IsMatched(string input)
    {
        return IsMatchedByWord(input) || IsMatchedByPattern(input);
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

        return new(censored, distinctProfanities);
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

        return new(censored, filteredProfanityList.AsReadOnly());
    }

    private CensorProfanityResult CensorBySingleWordProfanity(string sentence, string profanity,
        char censorCharacter,
        bool ignoreNumbers = false)
    {
        var censored = new StringBuilder(sentence);
        var appliedProfanities = new HashSet<string>();
        foreach (var result in GetCompleteWords(censored.ToString(), profanity))
        {
            var (startWordIndex, endWordIndex, wholeWord) = result;
            var filteredWord = wholeWord;
            if (ignoreNumbers)
                filteredWord = Regex.Replace(wholeWord, @"[\d-]", string.Empty);

            if (!IsMatched(filteredWord, profanity)) continue;

            appliedProfanities.Add(profanity);
            for (var i = startWordIndex; i < endWordIndex; i++)
                censored[i] = censorCharacter;
        }

        return new(censored.ToString(), appliedProfanities.ToList().AsReadOnly());
    }

    private record CensorProfanityResult(string CensoredSentence, IReadOnlyList<string> AppliedProfanities);

    private IReadOnlyList<string> FilterProfanitiesByAllowList(IEnumerable<string> words) =>
        words.Where(word => !AllowList.Contains(word))
            .ToList()
            .AsReadOnly();

    private static string CreateCensoredString(string word, char censorCharacter)
    {
        var censoredWordBuilder = new StringBuilder();

        foreach (var t in word)
            censoredWordBuilder.Append(IsEmptyOrPunctuation(t) ? t : censorCharacter);

        return censoredWordBuilder.ToString();
    }
}