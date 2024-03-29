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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProfanityFilter.Tests.Unit
{
    [TestClass]
    public class ProfanityBaseTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsArgumentNullExceptionForNullWordListArray()
        {
            _ = new ProfanityFilter((string[])null);
        }

        [TestMethod]
        public void ConstructorOverridesProfanityListWithArray()
        {
            string[] _wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityFilter(_wordList);

            Assert.AreEqual(3, filter.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsArgumentNullExceptionForNullWordList()
        {
            _ = new ProfanityFilter((List<string>)null);
        }

        [TestMethod]
        public void ConstructorOverridesProfanityList()
        {
            string[] _wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityFilter(new List<string>(_wordList));

            Assert.AreEqual(3, filter.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddProfanityThrowsArgumentNullExceptionForNullProfanity()
        {
            var filter = new ProfanityBase();
            filter.AddProfanity(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddProfanityThrowsArgumentNullExceptionForEmptyProfanityString()
        {
            var filter = new ProfanityBase();
            filter.AddProfanity(string.Empty);
        }

        [TestMethod]
        public void AddProfanityAddsToList()
        {
            var filter = new ProfanityFilter();
            Assert.IsFalse(filter.IsProfanity("fluffy"));

            filter.AddProfanity("fluffy");
            Assert.IsTrue(filter.IsProfanity("fluffy"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddProfanityThrowsArgumentNullExceptionForNullProfanityArray()
        {
            var filter = new ProfanityBase();
            filter.AddProfanityWords(null);
        }

        [TestMethod]
        public void AddProfanityAddsToProfanityArray()
        {
            string[] _wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityBase();

            filter.Clear();
            Assert.AreEqual(0, filter.Count);

            filter.AddProfanityWords(new List<string>(_wordList));

            Assert.AreEqual(3, filter.Count);
        }

        [TestMethod]
        public void AddProfanityAddsToProfanityList()
        {
            string[] _wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };

            var filter = new ProfanityBase();

            filter.Clear();
            Assert.AreEqual(0, filter.Count);

            filter.AddProfanityWords(_wordList);

            Assert.AreEqual(3, filter.Count);
        }

        [TestMethod]
        public void ReturnCountForDefaultProfanityList()
        {
            var filter = new ProfanityBase();
            var count = filter.Count;

            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void ClearEmptiesProfanityList()
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

        [TestMethod]
        public void RemoveDeletesAProfanityAndReturnsTrue()
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

            Assert.IsTrue(filter.RemoveProfanity("shit"));

            Assert.AreEqual(wordList.Length - 1, filter.Count);
        }

        [TestMethod]
        public void RemoveDeletesAProfanityAndIsProfanityIgnoresIt()
        {
            string[] wordList =
            {
                "fuck",
                "shit",
                "bollocks"
            };
            var filter = new ProfanityFilter();
            filter.AddProfanityWords(wordList);
            Assert.IsTrue(filter.IsProfanity("shit"));
            filter.RemoveProfanity("shit");

            Assert.IsFalse(filter.IsProfanity("shit"));
        }

        [TestMethod]
        public void RemoveDeletesAProfanityAndReturnsFalseIfProfanityDoesntExist()
        {
            var filter = new ProfanityBase();
            
            Assert.IsFalse(filter.RemoveProfanity("fluffy"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveProfanityThrowsArgumentNullExceptionIfListNull()
        {
            var filter = new ProfanityBase();

            List<string> listOfProfanitiesToRemove = null;

            filter.RemoveProfanity(listOfProfanitiesToRemove);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveProfanityThrowsArgumentNullExceptionIfArrayNull()
        {
            var filter = new ProfanityBase();

            string[] listOfProfanitiesToRemove = null;

            filter.RemoveProfanity(listOfProfanitiesToRemove);
        }
    }
}