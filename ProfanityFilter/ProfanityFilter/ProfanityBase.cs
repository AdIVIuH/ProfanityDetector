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
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using ProfanityFilter.Extensions;
using ProfanityFilter.Models;

namespace ProfanityFilter;

public class ProfanityBase
{
    private readonly HashSet<string> _profanityPatterns;
    private readonly HashSet<string> _profanities;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Return the allow list;
    /// </summary>
    public AllowList AllowList { get; }

    /// <summary>
    /// Constructor that initializes the standard profanity list.
    /// </summary>
    public ProfanityBase()
    {
        _profanityPatterns = new HashSet<string>();
        _profanities = new HashSet<string>();
        var memoryCacheOptions = new MemoryCacheOptions();
        _cache = new MemoryCache(memoryCacheOptions);
        AllowList = new AllowList();
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="profanityWord">The profanity word to add.</param>
    public void AddProfanityWord(string profanityWord)
    {
        if (string.IsNullOrEmpty(profanityWord)) throw new ArgumentNullException(nameof(profanityWord));

        _profanities.Add(profanityWord);
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="pattern">The profanity pattern to add.</param>
    public void AddProfanityPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentNullException(nameof(pattern));

        if (!pattern.IsValidRegex())
            throw new ArgumentException($"The value of {nameof(pattern)} param is not a valid regex", pattern);
        _profanityPatterns.Add(pattern);
    }

    /// <summary>
    /// Adds a list of profanity patterns.
    /// </summary>
    /// <param name="patterns">The list of profanity patterns to add</param>
    public void AddProfanityPatterns(IEnumerable<string> patterns)
    {
        if (patterns == null)
            throw new ArgumentNullException(nameof(patterns));

        foreach (var pattern in patterns)
            AddProfanityPattern(pattern);
    }

    /// <summary>
    /// Adds a list of profanity words.
    /// </summary>
    /// <param name="words">The list of profanities to add</param>
    public void AddProfanityWords(IEnumerable<string> words)
    {
        if (words == null) throw new ArgumentNullException(nameof(words));

        foreach (var word in words)
            AddProfanityWord(word);
    }

    /// <summary>
    /// Remove a profanity from the current loaded list of profanities.
    /// </summary>
    /// <param name="wordOrPattern">The profanity to remove from the list.</param>
    /// <returns>True of the profanity was removed. False otherwise.</returns>
    public bool RemoveProfanityWord(string wordOrPattern)
    {
        if (string.IsNullOrEmpty(wordOrPattern))
            throw new ArgumentNullException(nameof(wordOrPattern));

        var normalizedInput = GetNormalizedInputOrCache(wordOrPattern, ignoreNumbers: true);
        return _profanities.Remove(normalizedInput) || _profanityPatterns.Remove(wordOrPattern);
    }

    /// <summary>
    /// Remove all profanities from the current loaded list.
    /// </summary>
    public void Clear()
    {
        _profanityPatterns.Clear();
        _profanities.Clear();
    }

    /// <summary>
    /// Return the number of profanities in the system.
    /// </summary>
    public int Count => _profanityPatterns.Count + _profanities.Count;

    /// <summary>
    /// Check whether a given pattern matches an entry in the profanity list. <see cref="HasProfanityByPattern"/> will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="input">Pattern to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public bool HasProfanityByPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;
        return _profanityPatterns.Any(p => HasProfanityByPattern(normalizedInput, p));
    }

    /// <summary>
    /// Check whether a given term matches an entry in the profanity list. <see cref="HasProfanityByTerm"/> will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public bool HasProfanityByTerm(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (AllowList.Contains(normalizedInput))
            return false;
        return _profanities.Any(profanity => HasProfanityByTerm(normalizedInput, profanity));
    }

    protected internal string NormalizeInput(string input, bool ignoreNumbers = false)
    {
        var extractedWords = GetExtractedWordsOrCache(input);
        // TODO not implemented yet add gomogliths }|{ -> ж
        var resultWords = extractedWords
            .Select(cw => cw.WholeWord)
            .Select(w => w.ReplaceHomoglyphs());

        if (ignoreNumbers)
            resultWords = resultWords.Select(w => w.SelectOnlyLetters());

        var joinedWords = string.Join(' ', resultWords);
        var result = joinedWords.Trim().ToLower(CultureInfo.InvariantCulture);

        return result;
    }

    protected internal string GetNormalizedInputOrCache(string input, bool ignoreNumbers = false)
    {
        // return NormalizeInput(input, ignoreNumbers);
        var memoryCacheEntryOptions = new MemoryCacheEntryOptions
        {
            // TODO какой интервал экспирации лучше проставить?
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100000)
        };

        return _cache.GetOrCreate(CacheKeys.GetKeyForNormalizeInput(input), entry =>
        {
            entry.SetOptions(memoryCacheEntryOptions);
            var normalizedInput = NormalizeInput(input, ignoreNumbers);
            entry.SetValue(normalizedInput);
            // add cache for normalized string
            _cache.Set(CacheKeys.GetKeyForNormalizeInput(normalizedInput), normalizedInput, memoryCacheEntryOptions);
            return normalizedInput;
        });
    }

    protected bool IsProfanityPhrase(string profanity)
    {
        if (string.IsNullOrWhiteSpace(profanity))
            return false;
        // TODO переписал это с обычного сплита, т.к. в регулярке учитываюстя все разделяющие символы. Добавить кэширование \ компиляцию регулярки 
        return _profanities.Contains(profanity) && Regex.Matches(profanity, RegexPatterns.WordsSeparatorsPattern).Count > 1;
    }

    protected bool IsProfanityWord(string profanity) =>
        !string.IsNullOrWhiteSpace(profanity)
        && _profanities.Contains(profanity)
        && profanity.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 1;

    /// <summary>
    /// Check whether a given input matches an entry in the profanity list. 
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <param name="profanity">Profanity to check</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    protected bool HasProfanity(string input, string profanity) =>
        HasProfanityByTerm(input, profanity) || HasProfanityByPattern(input, profanity);

    protected internal IReadOnlyList<TextWithProfanities> GetMatchedProfanities(string input,
        bool includePartialMatch = true,
        bool includePatterns = true)
    {
        if (string.IsNullOrEmpty(input))
            return Enumerable.Empty<TextWithProfanities>().ToList().AsReadOnly();
        
        var textWithProfanitiesList = new List<TextWithProfanities>();
        var profanityPhrases = FindProfanityPhrases(input);
        var wordsWithProfanities = FindWordsWithProfanities(input);
        textWithProfanitiesList.AddRange(profanityPhrases);
        textWithProfanitiesList.AddRange(wordsWithProfanities);

        if (includePatterns)
        {
            var matchedPatterns = FindProfanitiesByPatterns(input);
            textWithProfanitiesList.AddRange(matchedPatterns);
        }

        var textWithUnAllowedProfanities = textWithProfanitiesList
            .Where(x => !AllowList.Contains(x.Text))
            .ToList();

        var result = includePartialMatch
            ? textWithUnAllowedProfanities
            : FilterPartialMatchedProfanities(textWithUnAllowedProfanities);

        return result
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    // TODO с сайд эффектами написано, но смысл такой
    private static IEnumerable<TextWithProfanities> FilterPartialMatchedProfanities(
        IReadOnlyList<TextWithProfanities> profanities)
    {
        var allFoundProfanities = profanities.SelectMany(x => x.Profanities).ToList();
        foreach (var item in profanities)
        {
            if (profanities.Any(x => x.Text != item.Text && x.Text.Contains(item.Text))) 
                continue;

            if (item.Profanities.Count == 1)
            {
                yield return item;
                continue;
            }

            foreach (var profanity in item.Profanities)
                // TODO Нужны тесты
                // TODO Возможно тут есть проблема с тем, что не будем обрабатывать все необходимые плохие слова
                if (allFoundProfanities.Any(p2 => p2 != profanity && p2.Contains(profanity)))
                    item.RemoveProfanity(profanity);
            
            yield return item;
        }
    }

    private IReadOnlyList<TextWithProfanities> FindProfanitiesByPatterns(string input)
    {
        // TODO нормализация??? 
        var normalizedInput = input.ToLower(CultureInfo.InvariantCulture);
        var result = _profanityPatterns
            .AsParallel()
            // TODO добавить логику для не нормализованной строки
            .Select(pattern => (UsedPattern: pattern, RegexMatches: Regex.Matches(normalizedInput, pattern)))
            .Where(x => x.RegexMatches.Count > 0)
            // TODO а что потом делать с гомоглифами?
            .SelectMany(x => x.RegexMatches.Select(m =>
                TextWithProfanities.Create(input.Substring(m.Index, m.Value.Length), x.UsedPattern)))
            .ToList();

        return result.AsReadOnly();
    }

    private IEnumerable<TextWithProfanities> FindProfanityPhrases(string input)
    {
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
        var profanityPhrases = _profanities.Where(IsProfanityPhrase);
        var matchedProfanityPhrases = profanityPhrases.Where(normalizedInput.Contains);
        // TODO получается, что тут теряется изначальный текст и case
        var result = matchedProfanityPhrases.Select(p => TextWithProfanities.Create(p, p));
        return result;
    }

    private IEnumerable<CompleteWord> GetExtractedWordsOrCache(string input)
    {
        var memoryCacheEntryOptions = new MemoryCacheEntryOptions
        {
            // TODO какой интервал экспирации лучше проставить?
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(100000)
        };

        return _cache.GetOrCreate(CacheKeys.GetKeyForExtractWords(input), entry =>
        {
            entry.SetOptions(memoryCacheEntryOptions);
            var extractedWords = input.ExtractWords().ToArray();
            entry.SetValue(extractedWords);
            return extractedWords;
        });
    }

    private IReadOnlyList<TextWithProfanities> FindWordsWithProfanities(string input)
    {
        var foundContainingProfanities = SearchContainingProfanityWords(input);
        var extractedCompleteWords = GetExtractedWordsOrCache(input);
        var wordsInOriginalInput = extractedCompleteWords.Select(cw => cw.WholeWord);
        var result = wordsInOriginalInput
            .AsParallel()
            .Select(w => (OriginalWord: w, FoundProfanities: FindProfanitiesInWord(w, foundContainingProfanities)))
            .Where(x => x.FoundProfanities.Any())
            .Select(x => TextWithProfanities.Create(x.OriginalWord, x.FoundProfanities))
            .ToList();


        return result.AsReadOnly();
    }

    private IReadOnlyList<string> SearchContainingProfanityWords(string input)
    {
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);
        var result = _profanities
            .AsParallel()
            .Where(p => IsProfanityWord(p) && normalizedInput.Contains(p))
            .Distinct()
            .ToList();
        return result.AsReadOnly();
    }

    private IReadOnlyList<string> FindProfanitiesInWord(string originalWord,
        IEnumerable<string> foundContainingProfanities)
    {
        var resultProfanitiesList = new List<string>();
        var normalizedWholeWord = GetNormalizedInputOrCache(originalWord, ignoreNumbers: true);
        var potentialProfanities = new HashSet<string>();
        var foundProfanitiesCounter = 0;

        foreach (var profanityWord in foundContainingProfanities)
        {
            if (normalizedWholeWord == profanityWord)
            {
                resultProfanitiesList.Add(profanityWord);
                // matchedProfanitiesWords.Add(profanityWord);
                // profanityWordsCounter++;
                // TODO maybe break?
                continue;
            }
            // todo проверить длину слова и вернуть если profanityWord.Length > normalizedWholeWord

            var inputContainsProfanity = normalizedWholeWord.Contains(profanityWord);
            if (!inputContainsProfanity)
                continue;

            var regexMatches = Regex.Matches(normalizedWholeWord, profanityWord);
            foundProfanitiesCounter += regexMatches.Count;
            potentialProfanities.Add(profanityWord);
        }

        if (foundProfanitiesCounter > 1)
            // TODO в теории можем не пробегаться прям по всему и остановиться как только счетчик достиг > 1
            resultProfanitiesList.AddRange(potentialProfanities);

        return resultProfanitiesList;
    }

    private bool HasProfanityByTerm(string input, string targetTermProfanity)
    {
        // TODO "son of a bitch" -> "son.of,a?bitch"
        if (string.IsNullOrWhiteSpace(input) || !_profanities.Contains(targetTermProfanity))
            return false;

        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);

        if (normalizedInput == targetTermProfanity)
            return true;

        var inputContainsProfanity = normalizedInput.Contains(targetTermProfanity);

        if (IsProfanityPhrase(targetTermProfanity))
            return inputContainsProfanity;

        if (!inputContainsProfanity)
            return false;

        var partialMatchedProfanityWords = FindWordsWithProfanities(input);
        return partialMatchedProfanityWords.Any();
    }

    private bool HasProfanityByPattern(string term, string pattern)
    {
        if (string.IsNullOrWhiteSpace(term) || !_profanityPatterns.Contains(pattern))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(term, ignoreNumbers: true);
        return Regex.IsMatch(normalizedInput, pattern);
    }
}