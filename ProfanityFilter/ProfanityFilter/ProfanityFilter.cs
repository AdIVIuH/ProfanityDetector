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
using System.Linq;
using System.Text;
using ProfanityFilter.Extensions;

namespace ProfanityFilter;

/// <summary>
///
/// This class will detect profanity and racial slurs contained within some text and return an indication flag.
/// All words are treated as case-insensitive.
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
    public IReadOnlyList<string> DetectWordsWithProfanities(string sentence, bool removePartialMatches = false)
    {
        if (string.IsNullOrEmpty(sentence))
            return new List<string>().AsReadOnly();

        var normalizedInput = GetNormalizedInputOrCache(sentence, ignoreNumbers: true);
        var matchedProfanities = GetMatchedProfanities(
            normalizedInput,
            includePartialMatch: !removePartialMatches,
            includePatterns: true);

        var result = new List<string>();
        var extractedWords = sentence.ExtractWords().ToArray();
        
        // TODO при добавлении слов в словарь делать их нормализацию
        var profanityPhrases = matchedProfanities
            .Where(p => p.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
            .ToArray();
        result.AddRange(profanityPhrases);
        var profanityWords = matchedProfanities
            .Where(p => p.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1)
            .ToArray();
        
        foreach (var (_, _, wholeWord) in extractedWords)
        {
            foreach (var profanity in profanityWords)
                if (HasProfanity(wholeWord, profanity))
                {
                    result.Add(wholeWord);
                }
        }
            

        return result
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    protected override IReadOnlyList<string> GetMatchedProfanities(string sentence,
        bool includePartialMatch = true,
        bool includePatterns = true)
    {
        var matchedProfanities = base.GetMatchedProfanities(
            sentence,
            includePartialMatch: includePartialMatch,
            includePatterns: includePatterns);

        matchedProfanities = FilterByAllowList(matchedProfanities);

        return matchedProfanities;
    }

    /// <summary>
    /// For any given string, censor any profanities from the list using the specified
    /// censoring character.
    /// </summary>
    /// <param name="sentence">The string to censor.</param>
    /// <param name="censorCharacter">The character to use for censoring.</param>
    /// <returns></returns>
    public string CensorString(string sentence, char censorCharacter = DefaultCensorString)
    {
        ArgumentNullException.ThrowIfNull(sentence);
        if (string.IsNullOrEmpty(sentence) || string.IsNullOrWhiteSpace(sentence))
            return sentence;

        var profanities = DetectWordsWithProfanities(sentence, removePartialMatches: false);
        var censoredString = CensorStringByProfanities(sentence, profanities, censorCharacter);
        return censoredString;
    }

    /// <summary>
    /// Check whether a given pattern matches an entry in the profanity list. <see cref="HasProfanityByPattern"/> will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="input">Pattern to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public override bool HasProfanityByPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
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
    /// <param name="input">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public override bool HasProfanityByTerm(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;

        return base.HasProfanityByTerm(normalizedInput);
    }

    private IReadOnlyList<string> FilterByAllowList(IEnumerable<string> profanities) =>
        profanities.Where(word => !AllowList.Contains(word))
            .ToList()
            .AsReadOnly();

    private string CensorStringByProfanities(string sentence, IReadOnlyList<string> profanities,
        char censorCharacter = DefaultCensorString)
    {
        var censored = sentence;
        censored = CensorByProfanityPhrases(censored, profanities, censorCharacter);
        censored = CensorByProfanityWords(censored, profanities, censorCharacter);

        return censored;
    }

    private static string CensorByProfanityWords(string sentence, IReadOnlyList<string> profanities,
        char censorCharacter)
    {
        var profanityWords = profanities
            .Where(p => p.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1)
            .ToHashSet();
        var censoredBuilder = new StringBuilder(sentence);
        var extractedWords = sentence.ExtractWords();
        var foundProfanities = extractedWords.Where(ew => profanityWords.Contains(ew.WholeWord));
        foreach (var result in foundProfanities)
        {
            var (startWordIndex, endWordIndex, _) = result;
            for (var i = startWordIndex; i < endWordIndex; i++)
                censoredBuilder[i] = censorCharacter;
        }

        return censoredBuilder.ToString();
    }

    private string CensorByProfanityPhrases(string sentence, IReadOnlyList<string> profanities, char censorCharacter)
    {
        var profanityPhrases = profanities.Where(p => p.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1);
        return profanityPhrases.Aggregate(sentence, (current, profanityPhrase) =>
            CensorByProfanityPhrase(current, profanityPhrase, censorCharacter));
    }

    private string CensorByProfanityPhrase(string sentence, string profanity, char censorCharacter) =>
        sentence.Replace(profanity, CreateCensoredString(profanity, censorCharacter));

    private static string CreateCensoredString(string sentence, char censorCharacter)
    {
        var censoredWordBuilder = new StringBuilder();

        foreach (var t in sentence)
            censoredWordBuilder.Append(t.IsWordsSeparator() ? t : censorCharacter);

        return censoredWordBuilder.ToString();
    }
}