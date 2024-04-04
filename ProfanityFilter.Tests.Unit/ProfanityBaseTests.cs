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
using FluentAssertions;
using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit
{
    [TestFixture]
    public class ProfanityBaseTests
    {
        [Test]
        public void AddProfanity_ThrowsArgumentNullExceptionForNullProfanity()
        {
            var filter = new ProfanityBase();
            var act = () => filter.AddProfanityWord(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddProfanity_ThrowsArgumentNullExceptionForEmptyProfanityString()
        {
            var filter = new ProfanityBase();
            var act = () => filter.AddProfanityWord(string.Empty);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddProfanity_AddsToList()
        {
            var filter = new ProfanityFilter();
            Assert.IsFalse(filter.IsMatchedByWord("fluffy"));

            filter.AddProfanityWord("fluffy");
            Assert.IsTrue(filter.IsMatchedByWord("fluffy"));
        }

        [Test]
        public void AddProfanity_ThrowsArgumentNullExceptionForNullProfanityArray()
        {
            var filter = new ProfanityBase();
            var act = () => filter.AddProfanityWords(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddProfanity_AddsToProfanityArray()
        {
            var wordList = new[]
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityBase();

            filter.Clear();
            Assert.AreEqual(0, filter.Count);

            filter.AddProfanityWords(new List<string>(wordList));

            Assert.AreEqual(3, filter.Count);
        }

        [Test]
        public void AddProfanity_AddsToProfanityList()
        {
            var wordList = new[]
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityBase();

            filter.Clear();
            Assert.AreEqual(0, filter.Count);

            filter.AddProfanityWords(wordList);

            Assert.AreEqual(3, filter.Count);
        }

        [Test]
        public void ReturnCountForDefaultProfanityList()
        {
            var filter = new ProfanityBase();
            var count = filter.Count;

            Assert.AreEqual(count, 0);
        }

        [Test]
        public void Clear_EmptiesProfanityList()
        {
            string[] wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityBase();
            filter.AddProfanityWords(wordList);

            Assert.AreEqual(wordList.Length, filter.Count);

            filter.Clear();

            Assert.AreEqual(0, filter.Count);
        }

        [Test]
        public void RemoveProfanity_DeletesAProfanityAndReturnsTrue()
        {
            string[] wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };
            var filter = new ProfanityBase();
            filter.AddProfanityWords(wordList);
            Assert.AreEqual(wordList.Length, filter.Count);

            Assert.IsTrue(filter.RemoveProfanityWord("shit"));

            Assert.AreEqual(wordList.Length - 1, filter.Count);
        }

        [Test]
        public void RemoveProfanity_DeletesAProfanityAndContainsProfanityIgnoresIt()
        {
            string[] wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };
            var filter = new ProfanityFilter();
            filter.AddProfanityWords(wordList);
            Assert.IsTrue(filter.IsMatchedByWord("shit"));
            filter.RemoveProfanityWord("shit");

            Assert.IsFalse(filter.IsMatchedByWord("shit"));
        }

        [Test]
        public void RemoveProfanity_DeletesAProfanityAndReturnsFalseIfProfanityDoesntExist()
        {
            var filter = new ProfanityBase();

            Assert.IsFalse(filter.RemoveProfanityWord("fluffy"));
        }
    }
}