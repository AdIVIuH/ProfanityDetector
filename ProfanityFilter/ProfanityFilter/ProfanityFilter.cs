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
    private const char DefaultCensorString = '*';

    /// <summary>
    /// Default constructor that loads up the default profanity list.
    /// </summary>
    public ProfanityFilter()
    {
    }

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

        var wordsWithProfanities = new List<string>();
        var normalizedInput = GetNormalizedInputOrCache(sentence, ignoreNumbers: true);
        var matchedProfanities = GetMatchedProfanities(
            normalizedInput,
            includePartialMatch: !removePartialMatches,
            includePatterns: true);

        var extractedWords = sentence.ExtractWords()
            .Select(w => w.WholeWord)
            .ToArray();

        // TODO при добавлении слов в словарь делать их нормализацию
        var profanityPhrases = matchedProfanities
            .Where(IsProfanityPhrase)
            .ToArray();
        // TODO тут потеряется Case
        wordsWithProfanities.AddRange(profanityPhrases);

        var profanityWordsOrPatterns = matchedProfanities
            .Where(p => IsProfanityWord(p) || IsProfanityPattern(p))
            .ToArray();

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var wholeWord in extractedWords)
            // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var profanityWordOrPattern in profanityWordsOrPatterns)
            if (HasProfanity(wholeWord, profanityWordOrPattern))
                wordsWithProfanities.Add(wholeWord);

        wordsWithProfanities = FilterByAllowList(wordsWithProfanities).ToList();

        return wordsWithProfanities
            .Distinct()
            .ToList()
            .AsReadOnly();
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
    /// Check whether a given input matches an entry in the profanity list. 
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public bool HasAnyProfanities(string input) =>
        !string.IsNullOrEmpty(input)
        // TODO не надо так =)
        && DetectWordsWithProfanities(
                input,
                removePartialMatches: true)
            .Any();

    private string CensorStringByProfanities(string sentence, IReadOnlyList<string> profanities,
        char censorCharacter = DefaultCensorString)
    {
        var censored = sentence;
        censored = CensorByProfanityPhrases(censored, profanities, censorCharacter);
        censored = CensorByProfanityWords(censored, profanities, censorCharacter);

        return censored;
    }

    private string CensorByProfanityWords(string input, IEnumerable<string> profanities, char censorCharacter)
    {
        var censoredBuilder = new StringBuilder(input);
        var extractedWords = input.ExtractWords();
        var foundWordsWithProfanities = extractedWords.Where(ew => profanities.Contains(ew.WholeWord));

        foreach (var wordWithProfanities in foundWordsWithProfanities)
        {
            var (startWordIndex, endWordIndex, _) = wordWithProfanities;
            for (var i = startWordIndex; i < endWordIndex; i++)
                censoredBuilder[i] = censorCharacter;
        }

        return censoredBuilder.ToString();
    }

    private string CensorByProfanityPhrases(string input, IEnumerable<string> profanities, char censorCharacter)
    {
        var profanityPhrases = profanities.Where(IsProfanityPhrase);
        return profanityPhrases.Aggregate(input, (current, profanityPhrase) =>
            CensorByProfanityPhrase(current, profanityPhrase, censorCharacter));
    }

    private string CensorByProfanityPhrase(string input, string profanity, char censorCharacter) =>
        input.Replace(profanity, CreateCensoredString(profanity, censorCharacter));

    private static string CreateCensoredString(string profanityPhrase, char censorCharacter)
    {
        var censoredWordBuilder = new StringBuilder();

        foreach (var t in profanityPhrase)
            censoredWordBuilder.Append(t.IsWordsSeparator() ? t : censorCharacter);

        return censoredWordBuilder.ToString();
    }
}