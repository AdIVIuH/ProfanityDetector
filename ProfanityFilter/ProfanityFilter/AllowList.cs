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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ProfanityFilter.Interfaces;

namespace ProfanityFilter
{
    public class AllowList : IAllowList
    {
        private readonly List<string> _allowList;

        public AllowList()
        {
            _allowList = new List<string>
            {
                "олеговн.*",
                "гребля",
                ".*(С|с)ергей.*",
                ".*к(о|а)манд.*",
                ".*л(о|а)х(о|а)трон.*",
                "хул(е|и)ган",
                ".*м(а|о)нд(а|о)рин.*",
            };
        }

        /// <summary>
        /// Add a word to the profanity allow list. This means a word that is in the allow list
        /// can be ignored. All words are treated as case insensitive.
        /// </summary>
        /// <param name="wordToAllowList">The word that you want to allow list.</param>
        public void Add(string wordToAllowList)
        {
            if (string.IsNullOrEmpty(wordToAllowList)) throw new ArgumentNullException(nameof(wordToAllowList));

            if (!_allowList.Contains(wordToAllowList.ToLower(CultureInfo.InvariantCulture)))
                _allowList.Add(wordToAllowList.ToLower(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordToCheck"></param>
        /// <returns></returns>
        public bool Contains(string wordToCheck)
        {
            if (string.IsNullOrEmpty(wordToCheck)) throw new ArgumentNullException(nameof(wordToCheck));

            return _allowList.Any(allowWordPattern => Regex.IsMatch(wordToCheck.ToLower(CultureInfo.InvariantCulture), allowWordPattern));
        }

        /// <summary>
        /// Return the number of items in the allow list.
        /// </summary>
        /// <returns>The number of items in the allow list.</returns>
        public int Count => _allowList.Count;

        /// <summary>
        /// Remove all words from the allow list.
        /// </summary>  
        public void Clear()
        {
            _allowList.Clear();
        }

        /// <summary>
        /// Remove a word from the profanity allow list. All words are treated as case insensitive.
        /// </summary>
        /// <param name="wordToRemove">The word that you want to use</param>
        /// <returns>True if the word is successfuly removes, False otherwise.</returns>
        public bool Remove(string wordToRemove)
        {
            if (string.IsNullOrEmpty(wordToRemove)) throw new ArgumentNullException(nameof(wordToRemove));

            return _allowList.Remove(wordToRemove.ToLower(CultureInfo.InvariantCulture));
        }
    }
}