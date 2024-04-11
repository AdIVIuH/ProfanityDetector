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
using System.Collections.Concurrent;
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
    protected readonly HashSet<string> ProfanityPatterns;
    protected readonly HashSet<string> Profanities;
    private readonly ConcurrentDictionary<string, string[]> _extractedWordsDictionary = new();
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Constructor that initializes the standard profanity list.
    /// </summary>
    public ProfanityBase()
    {
        ProfanityPatterns = new HashSet<string>();
        Profanities = new HashSet<string>();
        var memoryCacheOptions = new MemoryCacheOptions();
        _cache = new MemoryCache(memoryCacheOptions);
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="profanityWord">The profanity word to add.</param>
    public void AddProfanityWord(string profanityWord)
    {
        if (string.IsNullOrEmpty(profanityWord)) throw new ArgumentNullException(nameof(profanityWord));

        Profanities.Add(profanityWord);
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="pattern">The profanity pattern to add.</param>
    public void AddProfanityPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) 
            throw new ArgumentNullException(nameof(pattern));

        ProfanityPatterns.Add(pattern);
    }

    /// <summary>
    /// Adds a list of profanity patterns.
    /// </summary>
    /// <param name="patterns">The list of profanity patterns to add</param>
    public void AddProfanityPatterns(IEnumerable<string> patterns)
    {
        if (patterns == null) throw new ArgumentNullException(nameof(patterns));

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

    protected internal string NormalizeInput(string input, bool ignoreNumbers = false)
    {
        // TODO add gomogliths }|{ -> ж

        var extractedWords = GetExtractedWordsOrCache(input);
        var resultWords = ReplaceGomogliths(extractedWords.Select(cw=>cw.WholeWord));
         
        if (ignoreNumbers)
            resultWords = SelectOnlyLetters(resultWords);
        
        var joinedWords = string.Join(' ', resultWords);
        var result = joinedWords.Trim().ToLower(CultureInfo.InvariantCulture);

        return result;
    }

    protected string GetNormalizedInputOrCache(string input, bool ignoreNumbers = false)
    {
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

    protected CompleteWord[] GetExtractedWordsOrCache(string input)
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

    private IEnumerable<string> ReplaceGomogliths(IEnumerable<string> words)
    {
        foreach (var word in words)
            // TODO add logic here
            yield return word;
    }

    private IEnumerable<string> SelectOnlyLetters(IEnumerable<string> words) =>
        words.Select(SelectOnlyLetters);

    private string SelectOnlyLetters(string word)
    {
        // TODO add logic here
        var isContainsLettersRegex = new Regex("[a-zA-Z]+");
        return isContainsLettersRegex.IsMatch(word) 
            ? isContainsLettersRegex.Match(word).Value 
            : word;
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
        return Profanities.Remove(normalizedInput) || ProfanityPatterns.Remove(normalizedInput);
    }

    /// <summary>
    /// Remove all profanities from the current loaded list.
    /// </summary>
    public void Clear()
    {
        ProfanityPatterns.Clear();
        Profanities.Clear();
    }

    /// <summary>
    /// Return the number of profanities in the system.
    /// </summary>
    public int Count => ProfanityPatterns.Count + Profanities.Count;

    /// <summary>
    /// Check whether a given term matches an entry in the profanity list. ContainsProfanity will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="input">Term to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public virtual bool HasProfanityByPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);

        return ProfanityPatterns.Any(p => HasProfanityByPattern(normalizedInput, p));
    }

    public virtual bool HasProfanityByTerm(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);

        return Profanities.Any(profanity => HasProfanityByTerm(normalizedInput, profanity));
    }

    private bool HasProfanityByTerm(string input, string targetTermProfanity)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        // TODO "son of a bitch" -> "son.of,a?bitch"
        if (!Profanities.Contains(targetTermProfanity))
            return false;

        var normalizedInput = GetNormalizedInputOrCache(input, ignoreNumbers: true);

        if (normalizedInput == targetTermProfanity)
            return true;

        var inputContainsProfanity = normalizedInput.Contains(targetTermProfanity);
        var profanityParts = targetTermProfanity.Split(' ');
        if (profanityParts.Length == 1)
            return inputContainsProfanity &&
                   // TODO это может сильно влиять на производительность!
                   GetExtractedWordsOrCache(normalizedInput)
                       .Select(cw => cw.WholeWord)
                       .Contains(targetTermProfanity);
            

        return inputContainsProfanity;
    }

    protected bool HasProfanityByPattern(string term, string pattern)
    {
        if (string.IsNullOrWhiteSpace(term))
            return false;
        var normalizedInput = GetNormalizedInputOrCache(term, ignoreNumbers: true);
        return ProfanityPatterns.Contains(pattern) && Regex.IsMatch(normalizedInput, pattern);
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
    protected bool HasProfanity(string input, string profanity) =>
        HasProfanityByTerm(input, profanity) || HasProfanityByPattern(input, profanity);

    protected virtual IReadOnlyList<string> GetMatchedProfanities(string sentence, bool includePartialMatch = true,
        bool includePatterns = true)
    {
        var matchedProfanities = new List<string>();
        var normalizedInput = GetNormalizedInputOrCache(sentence, ignoreNumbers: true);
        var partialMatchedProfanityWords = Profanities
            .Where(profanity => HasProfanityByTerm(normalizedInput, profanity))
            .ToList();
        matchedProfanities.AddRange(partialMatchedProfanityWords);

        if (includePatterns)
        {
            var matchedPatterns = ProfanityPatterns
                .Where(pattern => HasProfanityByPattern(normalizedInput, pattern))
                .ToList();
            matchedProfanities.AddRange(matchedPatterns);
        }

        // TODO Нужны тесты
        // TODO Возможно тут есть проблема с тем, что не будем обрабатывать все необходимые плохие слова
        if (!includePartialMatch && matchedProfanities.Count > 1)
            matchedProfanities.RemoveAll(x => matchedProfanities.Any(y => x != y && y.Contains(x)));

        return matchedProfanities
            .Distinct()
            .ToList()
            .AsReadOnly();
    }
}