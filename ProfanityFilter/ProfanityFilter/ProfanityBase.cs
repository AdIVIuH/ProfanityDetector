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

namespace ProfanityFilter
{
    public class ProfanityBase
    {
        protected readonly HashSet<string> ProfanityPatterns;
        protected readonly HashSet<string> ProfanityWords;

        /// <summary>
        /// Constructor that initializes the standard profanity list.
        /// </summary>
        public ProfanityBase()
        {
            ProfanityPatterns = new HashSet<string>();
            ProfanityWords = new HashSet<string>();
        }

        /// <summary>
        /// Add a custom profanity to the list.
        /// </summary>
        /// <param name="profanityWord">The profanity word to add.</param>
        public void AddProfanityWord(string profanityWord)
        {
            if (string.IsNullOrEmpty(profanityWord)) throw new ArgumentNullException(nameof(profanityWord));

            ProfanityWords.Add(profanityWord);
        }

        /// <summary>
        /// Add a custom profanity to the list.
        /// </summary>
        /// <param name="pattern">The profanity pattern to add.</param>
        public void AddProfanityPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern));

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

        protected static string WordToPattern(string word) =>
            $@"(\b{word.ToLower(CultureInfo.InvariantCulture)}\b)";

        protected static string NormalizeInput(string input)
        {
            // }|{ -> ж
            var trimmed = input.Trim();
            var noPunctuation = Regex.Replace(trimmed, @"[^\w\s]", "");
            var lowerCased = noPunctuation.ToLower(CultureInfo.InvariantCulture);
            var result = lowerCased;
            return result;
        }

        /// <summary>
        /// Remove a profanity from the current loaded list of profanities.
        /// </summary>
        /// <param name="word">The profanity to remove from the list.</param>
        /// <returns>True of the profanity was removed. False otherwise.</returns>
        public bool RemoveProfanityWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                throw new ArgumentNullException(nameof(word));
            var normalizedInput = NormalizeInput(word);
            return ProfanityWords.Remove(normalizedInput);
        }

        /// <summary>
        /// Remove a profanity from the current loaded list of profanities.
        /// </summary>
        /// <param name="pattern">The profanity to remove from the list.</param>
        /// <returns>True of the profanity was removed. False otherwise.</returns>
        public bool RemoveProfanityPattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentNullException(nameof(pattern));

            var normalizedInput = NormalizeInput(pattern);
            return ProfanityPatterns.Remove(normalizedInput);
        }

        /// <summary>
        /// Remove all profanities from the current loaded list.
        /// </summary>
        public void Clear()
        {
            ProfanityPatterns.Clear();
            ProfanityWords.Clear();
        }

        /// <summary>
        /// Return the number of profanities in the system.
        /// </summary>
        public int Count => ProfanityPatterns.Count + ProfanityWords.Count;
        
        /// <summary>
        /// Check whether a given term matches an entry in the profanity list. ContainsProfanity will first
        /// check if the word exists on the allow list. If it is on the allow list, then false
        /// will be returned.
        /// </summary>
        /// <param name="pattern">Pattern to check.</param>
        /// <returns>True if the term contains a profanity, False otherwise.</returns>
        public virtual bool IsMatchedByPattern(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            var normalizedInput = NormalizeInput(input);
        
            return ProfanityPatterns.Any(p => IsMatchedByPattern(normalizedInput, p));
        }

        public virtual bool IsMatchedByWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            var normalizedInput = NormalizeInput(input);

            return ProfanityWords.Contains(normalizedInput);
        }

        protected bool IsMatchedByPattern(string term, string pattern)
        {
            var normalizedInput = NormalizeInput(term);
            return ProfanityPatterns.Contains(pattern) && Regex.IsMatch(normalizedInput, pattern);
        }

        protected IReadOnlyList<string> GetMatchedProfanities(string sentence, bool includePartialMatch = true,
            bool includePatterns = true)
        {
            var matchedProfanities = new List<string>();
            var normalizedInput = NormalizeInput(sentence);
            var partialMatchedProfanityWords = ProfanityWords
                .Where(profanityWord => normalizedInput.Contains(profanityWord))
                .ToList();
            matchedProfanities.AddRange(partialMatchedProfanityWords);
            

            // ReSharper disable once InvertIf
            if (includePatterns)
            {
                var matchedPatterns = ProfanityPatterns
                    .Where(pattern => IsMatchedByPattern(normalizedInput, pattern))
                    .ToList();
                matchedProfanities.AddRange(matchedPatterns);
            }

            if (!includePartialMatch)
            {
                matchedProfanities.RemoveAll(x => matchedProfanities.Any(y => x != y && y.Contains(x)));
            }
            
            return matchedProfanities
                .Distinct()
                .ToList()
                .AsReadOnly();
        }
    }
}