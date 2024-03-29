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

namespace ProfanityFilter
{
    public class ProfanityBase
    {
        protected readonly List<string> ProfanityPatterns;

        /// <summary>
        /// Constructor that initializes the standard profanity list.
        /// </summary>
        public ProfanityBase()
        {
            ProfanityPatterns = new List<string>(ProfanitiesDictionary.Patterns);
        }

        /// <summary>
        /// Constructor that allows you to insert a custom array or profanities.
        /// This list will replace the default list.
        /// </summary>
        /// <param name="profanityList">Array of words considered profanities.</param>
        protected ProfanityBase(string[] profanityList)
        {
            if (profanityList == null) throw new ArgumentNullException(nameof(profanityList));

            ProfanityPatterns = new List<string>(profanityList);
        }

        /// <summary>
        /// Constructor that allows you to insert a custom list or profanities.
        /// This list will replace the default list.
        /// </summary>
        /// <param name="profanityList">List of words considered profanities.</param>
        protected ProfanityBase(List<string> profanityList)
        {
            ProfanityPatterns = profanityList ?? throw new ArgumentNullException(nameof(profanityList));
        }

        /// <summary>
        /// Add a custom profanity to the list.
        /// </summary>
        /// <param name="profanity">The profanity to add.</param>
        public void AddProfanity(string profanity)
        {
            if (string.IsNullOrEmpty(profanity)) throw new ArgumentNullException(nameof(profanity));

            var pattern = WordToPattern(profanity);
            ProfanityPatterns.Add(pattern);
        }

        /// <summary>
        /// Adds a list of profanity words.
        /// </summary>
        /// <param name="words">The list of profanities to add</param>
        public void AddProfanityWords(IEnumerable<string> words)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));

            var patterns = words.Select(WordToPattern);
            ProfanityPatterns.AddRange(patterns);
        }

        private static string WordToPattern(string word) => 
            $@"(\b{word.ToLower(CultureInfo.InvariantCulture)}\b)";

        /// <summary>
        /// Adds a list of profanity patterns.
        /// </summary>
        /// <param name="patterns">The list of profanity patterns to add</param>
        public void AddProfanityPatterns(IEnumerable<string> patterns)
        {
            if (patterns == null) throw new ArgumentNullException(nameof(patterns));

            ProfanityPatterns.AddRange(patterns);
        }

        /// <summary>
        /// Remove a profanity from the current loaded list of profanities.
        /// </summary>
        /// <param name="profanity">The profanity to remove from the list.</param>
        /// <returns>True of the profanity was removed. False otherwise.</returns>
        public bool RemoveProfanity(string profanity)
        {
            if (string.IsNullOrEmpty(profanity)) throw new ArgumentNullException(nameof(profanity));

            return ProfanityPatterns.Remove(profanity.ToLower(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Remove an array of profanities from the current loaded list of profanities.
        /// </summary>
        /// <param name="profanities">The array of profanities to remove from the list.</param>
        /// <returns>True if the profanities were removed. False otherwise.</returns>
        public bool RemoveProfanity(IEnumerable<string> profanities)
        {
            if (profanities == null) throw new ArgumentNullException(nameof(profanities));

            foreach (var naughtyWord in profanities)
                if (!RemoveProfanity(naughtyWord))
                    return false;

            return true;
        }

        /// <summary>
        /// Remove all profanities from the current loaded list.
        /// </summary>
        public void Clear()
        {
            ProfanityPatterns.Clear();
        }

        /// <summary>
        /// Return the number of profanities in the system.
        /// </summary>
        public int Count => ProfanityPatterns.Count;
    }
}