﻿/*
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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ProfanityFilter.Interfaces;

namespace ProfanityFilter
{
    /// <summary>
    ///
    /// This class will detect profanity and racial slurs contained within some text and return an indication flag.
    /// All words are treated as case insensitive.
    ///
    /// </summary>
    public class ProfanityFilter : ProfanityBase
    {
        public const char DefaultCensorCharacter = '*';

        /// <summary>
        /// Default constructor that loads up the default profanity list.
        /// </summary>
        public ProfanityFilter()
        {
            AllowList = new AllowList();
        }

        /// <summary>
        /// Constructor overload that allows you to construct the filter with a customer
        /// profanity list.
        /// </summary>
        /// <param name="profanityList">Array of words to add into the filter.</param>
        public ProfanityFilter(string[] profanityList) : base(profanityList)
        {
            AllowList = new AllowList();
        }

        /// <summary>
        /// Constructor overload that allows you to construct the filter with a customer
        /// profanity list.
        /// </summary>
        /// <param name="profanityList">List of words to add into the filter.</param>
        public ProfanityFilter(List<string> profanityList) : base(profanityList)
        {
            AllowList = new AllowList();
        }

        /// <summary>
        /// Return the allow list;
        /// </summary>
        public IAllowList AllowList { get; }

        /// <summary>
        /// TODO есть еще 2 похожих метода
        /// Check whether a specific word is in the profanity list. IsProfanity will first
        /// check if the word exists on the allow list. If it is on the allow list, then false
        /// will be returned.
        /// </summary>
        /// <param name="word">The word to check in the profanity list.</param>
        /// <returns>True if the word is considered a profanity, False otherwise.</returns>
        public bool IsProfanity(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            var trimmedWord = word.Trim().ToLower(CultureInfo.InvariantCulture);
            // Check if the word is in the allow list.
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (AllowList.Contains(trimmedWord)) return false;

            return ProfanityPatterns.Any(profanity => IsSwearWord(trimmedWord, profanity));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public IReadOnlyList<string> DetectAllProfanities(string sentence) =>
            DetectAllProfanities(sentence, false);

        /// <summary>
        /// For a given sentence, return a list of all the detected profanities.
        /// </summary>
        /// <param name="sentence">The sentence to check for profanities.</param>
        /// <param name="removePartialMatches">Remove duplicate partial matches.</param>
        /// <returns>A read only list of detected profanities.</returns>
        public IReadOnlyList<string> DetectAllProfanities(string sentence, bool removePartialMatches)
        {
            if (string.IsNullOrEmpty(sentence))
                return new List<string>().AsReadOnly();

            sentence = sentence
                .ToLower()
                .Replace(".", "")
                .Replace(",", "");

            var words = sentence.Split(' ');
            var postAllowList = FilterWordListByAllowList(words);

            // Catch whether multi-word profanities are in the allow list filtered sentence.
            var swearList = GetMultiWordProfanities(ConvertWordListToSentence(postAllowList));

            // Deduplicate any partial matches, ie, if the word "twatting" is in a sentence, don't include "twat" if part of the same word.
            if (removePartialMatches)
                swearList.RemoveAll(x =>
                    swearList.Any(y => x != y && IsSwearWord(inputWord: y, swearWord: x, useAsPattern: true)));

            return FilterSwearListForCompleteWordsOnly(sentence, swearList)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// For any given string, censor any profanities from the list using the default
        /// censoring character of an asterix.
        /// </summary>
        /// <param name="sentence">The string to censor.</param>
        /// <returns></returns>
        public string CensorString(string sentence) =>
            CensorString(sentence, DefaultCensorCharacter);

        /// <summary>
        /// For any given string, censor any profanities from the list using the specified
        /// censoring character.
        /// </summary>
        /// <param name="sentence">The string to censor.</param>
        /// <param name="censorCharacter">The character to use for censoring.</param>
        /// <returns></returns>
        public string CensorString(string sentence, char censorCharacter) =>
            CensorString(sentence, censorCharacter, false);

        /// <summary>
        /// For any given string, censor any profanities from the list using the specified
        /// censoring character.
        /// </summary>
        /// <param name="sentence">The string to censor.</param>
        /// <param name="censorCharacter">The character to use for censoring.</param>
        /// <param name="ignoreNumbers">Ignore any numbers that appear in a word.</param>
        /// <returns></returns>
        public string CensorString(string sentence, char censorCharacter, bool ignoreNumbers)
        {
            if (string.IsNullOrEmpty(sentence)) return string.Empty;

            var noPunctuation = sentence.Trim().ToLower();
            noPunctuation = Regex.Replace(noPunctuation, @"[^\w\s]", "");

            var words = noPunctuation.Split(' ');
            var postAllowList = FilterWordListByAllowList(words);

            // Catch whether multi-word profanities are in the allow list filtered sentence.
            var swearList = GetMultiWordProfanities(ConvertWordListToSentence(postAllowList));

            return CensorStringByProfanityList(sentence, censorCharacter, swearList, ignoreNumbers);
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
        private (int startWordIndex, int endWordIndex, string wholeWord)? GetCompleteWord(string toCheck, string profanity)
        {
            if (string.IsNullOrEmpty(toCheck)) return null;

            var profanityLowerCase = profanity.ToLower(CultureInfo.InvariantCulture);
            var toCheckLowerCase = toCheck.ToLower(CultureInfo.InvariantCulture);
            var regexMatch = Regex.Match(toCheckLowerCase, profanityLowerCase);

            if (!regexMatch.Success) return null;

            // TODO Этот алгоритм кажется полностью решается Regex.Match
            var startIndex = regexMatch.Index;
            var endIndex = startIndex;

            // Work backwards in string to get to the start of the word.
            while (startIndex > 0)
            {
                if (toCheck[startIndex - 1] == ' ' || char.IsPunctuation(toCheck[startIndex - 1])) break;

                startIndex -= 1;
            }

            // Work forwards to get to the end of the word.
            while (endIndex < toCheck.Length)
            {
                if (toCheck[endIndex] == ' ' || char.IsPunctuation(toCheck[endIndex])) break;

                endIndex += 1;
            }

            var resultWord = toCheckLowerCase.Substring(startIndex, endIndex - startIndex);

            return (startIndex, endIndex, resultWord);
        }

        /// <summary>
        /// Check whether a given term matches an entry in the profanity list. ContainsProfanity will first
        /// check if the word exists on the allow list. If it is on the allow list, then false
        /// will be returned.
        /// </summary>
        /// <param name="term">Term to check.</param>
        /// <returns>True if the term contains a profanity, False otherwise.</returns>
        public bool ContainsProfanity(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return false;

            var potentialProfanities = ProfanityPatterns.Where(word => word.Length <= term.Length).ToList();

            // We might have a very short phrase coming in, resulting in no potential matches even before the regex
            if (potentialProfanities.Count == 0) return false;

            var regex = new Regex($"(?:{string.Join("|", potentialProfanities).Replace("$", "\\$")})");

            foreach (Match profanity in regex.Matches(term))
                // if any matches are found and aren't in the allowed list, we can return true here without checking further
                if (!AllowList.Contains(profanity.Value.ToLower(CultureInfo.InvariantCulture)))
                    return true;

            return false;
        }

        private string CensorStringByProfanityList(string sentence, char censorCharacter, IEnumerable<string> swearList,
            bool ignoreNumeric)
        {
            var censored = new StringBuilder(sentence);
            var tracker = new StringBuilder(sentence);

            foreach (var word in swearList.OrderByDescending(x => x.Length))
            {
                (int startWordIndex, int endWordIndex, string wholeWord)? result;
                var multiWord = word.Split(' ');

                if (multiWord.Length == 1)
                    do
                    {
                        result = GetCompleteWord(tracker.ToString(), word);

                        if (result == null) continue;

                        // TODO нужна ли вся эта логика. Возможно просто нужна чуть чуть сложная регулярка, которая будет заменять все, что нужно
                        var filtered = result.Value.wholeWord;

                        if (ignoreNumeric) filtered = Regex.Replace(result.Value.Item3, @"[\d-]", string.Empty);

                        if (IsSwearWord(filtered, word))
                            for (var i = result.Value.startWordIndex; i < result.Value.endWordIndex; i++)
                            {
                                censored[i] = censorCharacter;
                                tracker[i] = censorCharacter;
                            }
                        else
                            for (var i = result.Value.startWordIndex; i < result.Value.endWordIndex; i++)
                                tracker[i] = censorCharacter;
                    } while (result != null);
                else
                    censored = censored.Replace(word, CreateCensoredString(word, censorCharacter));
            }

            return censored.ToString();
        }

        private IReadOnlyCollection<string> FilterSwearListForCompleteWordsOnly(string sentence,
            IEnumerable<string> swearList)
        {
            var filteredSwearList = new List<string>();
            var tracker = new StringBuilder(sentence);

            foreach (var word in swearList.OrderByDescending(x => x.Length))
            {
                var multiWord = word.Split(' ');

                if (multiWord.Length == 1)
                {
                    (int, int, string)? result;
                    do
                    {
                        result = GetCompleteWord(tracker.ToString(), word);

                        if (result != null)
                        {
                            if (result.Value.Item3 == word)
                            {
                                filteredSwearList.Add(word);

                                for (var i = result.Value.Item1; i < result.Value.Item2; i++)
                                    tracker[i] = DefaultCensorCharacter;
                                break;
                            }

                            for (var i = result.Value.Item1; i < result.Value.Item2; i++)
                                tracker[i] = DefaultCensorCharacter;
                        }
                    } while (result != null);
                }
                else
                {
                    filteredSwearList.Add(word);
                    tracker.Replace(word, " ");
                }
            }

            return filteredSwearList;
        }

        private List<string> FilterWordListByAllowList(string[] words)
        {
            return words
                .Where(word =>
                    !string.IsNullOrEmpty(word) &&
                    !AllowList.Contains(word.ToLower(CultureInfo.InvariantCulture))
                )
                .ToList();
        }

        private static bool IsSwearWord(string inputWord, string swearWord, bool useAsPattern = true)
        {
            var trimmedInput = inputWord.Trim().ToLower(CultureInfo.InvariantCulture);
            return useAsPattern ? Regex.IsMatch(trimmedInput, swearWord) : trimmedInput == inputWord;
        }

        private static string ConvertWordListToSentence(IEnumerable<string> postAllowList) =>
            string.Join(' ', postAllowList);

        private List<string> GetMultiWordProfanities(string postAllowListSentence)
        {
            return ProfanityPatterns
                .Where(profanity => IsSwearWord(postAllowListSentence, profanity, useAsPattern: true))
                .ToList();
        }

        private static string CreateCensoredString(string word, char censorCharacter)
        {
            var censoredWordBuilder = new StringBuilder();

            foreach (var t in word)
                censoredWordBuilder.Append(t != ' ' ? censorCharacter : ' ');

            return censoredWordBuilder.ToString();
        }
    }
}