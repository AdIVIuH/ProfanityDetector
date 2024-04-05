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
using ProfanityFilter.Extensions;

namespace ProfanityFilter;

public class ProfanityBase
{
    private readonly HashSet<string> _profanityPatterns;
    private readonly HashSet<string> _profanityWords;

    /// <summary>
    /// Constructor that initializes the standard profanity list.
    /// </summary>
    public ProfanityBase()
    {
        _profanityPatterns = new HashSet<string>();
        _profanityWords = new HashSet<string>();
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="profanityWord">The profanity word to add.</param>
    public void AddProfanityWord(string profanityWord)
    {
        if (string.IsNullOrEmpty(profanityWord)) throw new ArgumentNullException(nameof(profanityWord));

        _profanityWords.Add(profanityWord);
    }

    /// <summary>
    /// Add a custom profanity to the list.
    /// </summary>
    /// <param name="pattern">The profanity pattern to add.</param>
    public void AddProfanityPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

        _profanityPatterns.Add(pattern);
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

    protected static string NormalizeInput(string input)
    {
        // TODO add gomogliths }|{ -> ж
        return input.Trim()
            .RemovePunctuation()
            .ToLower(CultureInfo.InvariantCulture);
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

        var normalizedInput = NormalizeInput(wordOrPattern);
        return _profanityWords.Remove(normalizedInput) || _profanityPatterns.Remove(normalizedInput);
    }

    /// <summary>
    /// Remove all profanities from the current loaded list.
    /// </summary>
    public void Clear()
    {
        _profanityPatterns.Clear();
        _profanityWords.Clear();
    }

    /// <summary>
    /// Return the number of profanities in the system.
    /// </summary>
    public int Count => _profanityPatterns.Count + _profanityWords.Count;

    /// <summary>
    /// Check whether a given term matches an entry in the profanity list. ContainsProfanity will first
    /// check if the word exists on the allow list. If it is on the allow list, then false
    /// will be returned.
    /// </summary>
    /// <param name="input">Pattern to check.</param>
    /// <returns>True if the term contains a profanity, False otherwise.</returns>
    public virtual bool HasProfanityByPattern(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalizedInput = NormalizeInput(input);

        return _profanityPatterns.Any(p => HasProfanityByPattern(normalizedInput, p));
    }

    public virtual bool HasProfanityByTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return false;
        var normalizedInput = NormalizeInput(term);

        return _profanityWords.Contains(normalizedInput);
    }

    protected bool HasProfanityByPattern(string term, string pattern)
    {
        var normalizedInput = NormalizeInput(term);
        return _profanityPatterns.Contains(pattern) && Regex.IsMatch(normalizedInput, pattern);
    }

    protected IReadOnlyList<string> GetMatchedProfanities(string sentence, bool includePartialMatch = true,
        bool includePatterns = true)
    {
        var matchedProfanities = new List<string>();
        var normalizedInput = NormalizeInput(sentence);
        var partialMatchedProfanityWords = _profanityWords
            .Where(profanityWord => normalizedInput.Contains(profanityWord))
            .ToList();
        matchedProfanities.AddRange(partialMatchedProfanityWords);


        // ReSharper disable once InvertIf
        if (includePatterns)
        {
            var matchedPatterns = _profanityPatterns
                .Where(pattern => HasProfanityByPattern(normalizedInput, pattern))
                .ToList();
            matchedProfanities.AddRange(matchedPatterns);
        }

        // TODO Возможно тут есть проблема с тем, что не будем обрабатывать все необходимые плохие слова
        if (!includePartialMatch)
            matchedProfanities.RemoveAll(x => matchedProfanities.Any(y => x != y && y.Contains(x)));

        return matchedProfanities
            .Distinct()
            .ToList()
            .AsReadOnly();
    }
}