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
        /// Return the allow list;
        /// </summary>
        public IAllowList AllowList { get; }

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

            var lowerCaseSentence = sentence.ToLower(CultureInfo.InvariantCulture);
            var words = ExtractWordsFromSentence(lowerCaseSentence);
            var postAllowList = FilterWordListByAllowList(words);

            // Catch whether multi-word profanities are in the allow list filtered sentence.
            var wordsOnlySentence = string.Join(' ', postAllowList);
            var profanities = GetMatchedProfanities(wordsOnlySentence, includePatterns: true);

            return FilterForCompleteWordsOnly(sentence, profanities)
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
        /// <param name="ignoreNumbers">Ignore any numbers that appear in a word.</param>
        /// <returns></returns>
        public string CensorString(string sentence, char censorCharacter = DefaultCensorCharacter,
            bool ignoreNumbers = false)
        {
            if (string.IsNullOrEmpty(sentence)) return string.Empty;

            var lowerCaseSentence = sentence.ToLower(CultureInfo.InvariantCulture);
            var words = ExtractWordsFromSentence(lowerCaseSentence);
            var postAllowList = FilterWordListByAllowList(words);

            // Catch whether multi-word profanities are in the allow list filtered sentence.
            var wordsOnlySentence = string.Join(' ', postAllowList);
            var profanities = GetMatchedProfanities(wordsOnlySentence, onlyExactMatch: false, includePatterns: true);

            return CensorStringByProfanityList(sentence, censorCharacter, profanities, ignoreNumbers);
        }

        private static string[] ExtractWordsFromSentence(string sentence)
        {
            var noPunctuation = sentence.Trim().ToLower();
            noPunctuation = Regex.Replace(noPunctuation, @"[^\w\s]", "");

            var words = noPunctuation.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words;
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
            var toCheckLowerCase = toCheck.ToLower(CultureInfo.InvariantCulture);
            var regexMatches = Regex.Matches(toCheckLowerCase, profanityLowerCase);

            for (var index = 0; index < regexMatches.Count; index++)
            {
                var regexMatch = regexMatches[index];
                var startIndex = FindStartWordIndex(toCheck, regexMatch.Index);
                var endIndex = FindEndWordIndex(toCheck, regexMatch.Index);
                var wordLength = endIndex - startIndex;
                var wholeWord = toCheckLowerCase.Substring(startIndex, wordLength);

                yield return (startIndex, endIndex, wholeWord);
            }
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
        public override bool ContainsByPattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;
            var lowerCasePattern = pattern.ToLower(CultureInfo.InvariantCulture);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (AllowList.Contains(lowerCasePattern))
                return false;

            return base.ContainsByPattern(lowerCasePattern);
        }

        public override bool ContainsByWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word)) return false;
            var lowerCaseWord = word.ToLower(CultureInfo.InvariantCulture);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (AllowList.Contains(lowerCaseWord))
                return false;

            return base.ContainsByWord(lowerCaseWord);
        }

        private string CensorStringByProfanityList(string sentence, char censorCharacter,
            IEnumerable<string> profanities,
            bool ignoreNumeric)
        {
            var censored = new StringBuilder(sentence);

            foreach (var profanity in profanities.OrderByDescending(x => x.Length))
            {
                var profanityParts = profanity.Split(' ');

                if (profanityParts.Length == 1)
                {
                    var matchedWordsByProfanity = GetCompleteWords(sentence, profanity);
                    foreach (var (startWordIndex, endWordIndex, wholeWord) in matchedWordsByProfanity)
                    {
                        var filteredWord = wholeWord;
                        if (ignoreNumeric)
                            filteredWord = Regex.Replace(wholeWord, @"[\d-]", string.Empty);
                        for (var i = startWordIndex; i < endWordIndex; i++)
                            if (ContainsByWord(filteredWord)
                                || (ProfanityPatterns.Contains(profanity)
                                    && Regex.IsMatch(filteredWord, profanity)))
                                censored[i] = censorCharacter;
                    }
                }
                else
                {
                    censored = censored.Replace(profanity, CreateCensoredString(profanity, censorCharacter));
                }
            }

            return censored.ToString();
        }

        private IReadOnlyCollection<string> FilterForCompleteWordsOnly(string sentence,
            IEnumerable<string> profanities)
        {
            var filteredProfanityList = new List<string>();
            var tracker = new StringBuilder(sentence);

            foreach (var profanity in profanities.OrderByDescending(x => x.Length))
            {
                var profanityParts = profanity.Split(' ');

                if (profanityParts.Length == 1)
                {
                    var matchedWordsByProfanity = GetCompleteWords(tracker.ToString(), profanity);
                    foreach (var (startWordIndex, endWordIndex, _) in matchedWordsByProfanity)
                        for (var i = startWordIndex; i < endWordIndex; i++)
                            tracker[i] = DefaultCensorCharacter;
                    // (int StartWordIndex, int EndWordIndex, string WholeWord)? result;
                    // // TODO выглядит как будто это все что делает, это несколько раз пробегается по одмун и тому же предложеию и по порядку цензурит встреченные слова
                    // do
                    // {
                    //     result = GetCompleteWords(tracker.ToString(), profanity);
                    //     if (result == null) continue;
                    //     // выглядит как будто это не нужно тут
                    //     // if (result.Value.WholeWord == profanity)
                    //     // {
                    //     //     filteredProfanityList.Add(profanity);
                    //     //
                    //     //     for (var i = result.Value.StartWordIndex; i < result.Value.EndWordIndex; i++)
                    //     //         tracker[i] = DefaultCensorCharacter;
                    //     //     break;
                    //     // }
                    //
                    //     for (var i = result.Value.StartWordIndex; i < result.Value.EndWordIndex; i++)
                    //         tracker[i] = DefaultCensorCharacter;
                    // } while (result != null);
                }
                else
                {
                    filteredProfanityList.Add(profanity);
                    tracker.Replace(profanity, " ");
                }
            }

            return filteredProfanityList;
        }

        private IReadOnlyList<string> FilterWordListByAllowList(IEnumerable<string> words) =>
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
}