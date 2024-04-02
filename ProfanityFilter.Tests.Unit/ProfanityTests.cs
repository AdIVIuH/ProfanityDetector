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

using System.Linq;
using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit
{
    [TestFixture]
    public class ProfanityTests
    {
        [Test]
        public void ConstructorSetsAllowList()
        {
            var filter = new ProfanityFilter();
            Assert.IsNotNull(filter.AllowList);
        }

        [TestCase("arsehole")]
        [TestCase("shitty")]
        public void ContainsProfanity_ReturnsTrue_ForSwearWord(string swearWord)
        {
            var filter = CreateProfanityFilter();
            Assert.IsTrue(filter.ContainsByWord(swearWord));
        }

        [TestCase("fluffy")]
        [TestCase("")]
        [TestCase(null)]
        public void ContainsProfanity_ReturnsFalse_ForNonSwearWord(string nonSwearWord)
        {
            var filter = CreateProfanityFilter();
            Assert.IsFalse(filter.ContainsByWord(nonSwearWord));
        }

        [TestCase("shitty")]
        [TestCase("лох")]
        [TestCase("Лох")]
        [TestCase("лоХ")]
        public void ContainsProfanity_ReturnsFalse_ForWordOnTheAllowList(string word)
        {
            var filter = CreateProfanityFilter();
            Assert.IsTrue(filter.ContainsByWord(word));

            filter.AllowList.Add(word);

            Assert.IsFalse(filter.ContainsByWord(word));
        }

        [TestCase("")]
        [TestCase(null)]
        public void DetectAllProfanities_ReturnsEmptyList_ForEmptyInput(string input)
        {
            var filter = CreateProfanityFilter();
            var swearList = filter.DetectAllProfanities(input);

            Assert.AreEqual(0, swearList.Count);
        }

        [TestCase("You are a complete twat and a dick.", "twat", "dick")]
        [TestCase("You are, a complete twat, and a @dick:", "twat", "dick")]
        [TestCase("You are a complete tWat and a DiCk.", "twat", "dick")]
        public void DetectAllProfanities_Returns2SwearWords(string input, params string[] expected)
        {
            var filter = CreateProfanityFilter();
            var swearList = filter.DetectAllProfanities(input);

            Assert.AreEqual(expected.Length, swearList.Count);
            Assert.IsFalse(expected.Except(swearList).Any());
        }

        [TestCase("2 girls 1 cup is my favourite video", "2 girls 1 cup")]
        [TestCase("2 girls 1 cup is my favourite twatting video", "2 girls 1 cup", "twatting")]
        public void DetectAllProfanities_ReturnsSwearPhrases(string input, params string[] expected)
        {
            var filter = CreateProfanityFilter();
            var swearList = filter.DetectAllProfanities(input);

            Assert.AreEqual(expected.Length, swearList.Count);
            Assert.IsFalse(expected.Except(swearList).Any());
        }

        [Test]
        public void DetectAllProfanities_Returns2SwearPhrase_BecauseOfMatchDeduplication()
        {
            var filter = CreateProfanityFilter();
            var swearList = filter.DetectAllProfanities("2 girls 1 cup is my favourite twatting video", true);

            Assert.AreEqual(2, swearList.Count);
            Assert.AreEqual("2 girls 1 cup", swearList[0]);
            Assert.AreEqual("twatting", swearList[1]);
        }

        [Test]
        public void DetectAllProfanities_Scunthorpe()
        {
            var filter = CreateProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var swearList = filter.DetectAllProfanities(
                sentence:
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
                removePartialMatches: false);

            Assert.AreEqual(4, swearList.Count);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fuck", swearList[2]);
            Assert.AreEqual("shit", swearList[3]);
        }

        [Test]
        public void DetectAllProfanities_Scunthorpe_WithDuplicatesTurnedOff()
        {
            var filter = CreateProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var swearList = filter.DetectAllProfanities(
                sentence:
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
                removePartialMatches: true);

            Assert.AreEqual(3, swearList.Count);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("shit", swearList[2]);
        }

        [Test]
        public void DetectAllProfanities_BlocksAllowList()
        {
            var filter = CreateProfanityFilter();
            filter.AllowList.Add("tit");

            var swearList = filter.DetectAllProfanities("You are a complete twat and a total tit.", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("twat", swearList[0]);
        }

        [Test]
        public void DetectAllProfanities_Scunthorpe_WithDuplicatesTurnedOffAndNoAllowList()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities(
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
                true);

            Assert.AreEqual(3, swearList.Count);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("shit", swearList[2]);
        }

        [Test]
        public void DetectAllProfanitiesMultipleScunthorpes()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe Scunthorpe", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [Test]
        public void DetectAllProfanitiesMultipleScunthorpesSingleCunt()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe cunt Scunthorpe", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cunt", swearList[0]);
        }

        [Test]
        public void DetectAllProfanitiesMultipleScunthorpesMultiCunt()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe cunt Scunthorpe cunt", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cunt", swearList[0]);
        }

        [Test]
        public void DetectAllProfanitiesScunthorpePenistone()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("ScUnThOrPePeNiStOnE", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [Test]
        public void DetectAllProfanitiesScunthorpePenistoneOneKnob()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("ScUnThOrPePeNiStOnE KnOb", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("knob", swearList[0]);
        }

        [Test]
        public void DetectAllProfanitiesLongerSentence()
        {
            var filter = CreateProfanityFilter();

            var swearList =
                filter.DetectAllProfanities(
                    "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.", true);

            Assert.AreEqual(4, swearList.Count);
            Assert.AreEqual("alaskan pipeline", swearList[0]);
            Assert.AreEqual("blow your load", swearList[1]);
            Assert.AreEqual("stupid", swearList[2]);
            Assert.AreEqual("twat", swearList[3]);
        }

        [Test]
        public void DetectAllProfanities_ForSingleWord()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("cock", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cock", swearList[0]);
        }

        [Test]
        public void DetectAllProfanitiesFor_EmptyString()
        {
            var filter = CreateProfanityFilter();

            var swearList = filter.DetectAllProfanities("", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [Test]
        public void CensoredString_ReturnsStringWithProfanities_BleepedOut()
        {
            var filter = CreateProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var censored = filter.CensorString("Выдать заказ лоху");
            var result = "Выдать заказ ****";

            Assert.AreEqual(result, censored);
        }

        [TestCase(
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            "I ******* live in Scunthorpe and it is a **** place to live. I would much rather live in penistone you great big **** ****.")]
        [TestCase(
            "I Fucking Live In Scunthorpe And It Is A Shit Place To Live. I Would Much Rather Live In Penistone You Great Big Cock Fuck.",
            "I ******* Live In Scunthorpe And It Is A **** Place To Live. I Would Much Rather Live In Penistone You Great Big **** ****.")]
        [TestCase(
            "2 girls 1 cup, is my favourite twatting video.",
            "* ***** * ***, is my favourite ******** video.")]
        [TestCase(
            "Mary had a little shit lamb who was a little fucker.",
            "Mary had a little **** lamb who was a little ******.")]
        [TestCase(
            "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.",
            "You are a ****** little ****, and you like to **** **** **** in an ******* ********.")]
        [TestCase("Scunthorpe", "Scunthorpe")]
        [TestCase("ScUnThOrPe", "ScUnThOrPe")]
        [TestCase("ScUnThOrPePeNiStOnE", "ScUnThOrPePeNiStOnE")]
        [TestCase("scunthorpe", "scunthorpe")]
        public void CensoredString_ReturnsStringWithProfanities_BleepedOutNoAllowList(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(expected, censored);
        }

        [Test]
        public void CensoredString_ReturnsStringDoubleCunt()
        {
            var filter = CreateProfanityFilter();

            var censored = filter.CensorString("cunt cunt");
            var result = "**** ****";

            Assert.AreEqual(censored, result);
        }

        [TestCase("Ебаный рот этого казино, блять. Ты кто такой сука?",
            "****** рот этого казино, *****. Ты кто такой ****?")]
        [TestCase("Выдать заказ лоху", "Выдать заказ ****")]
        [TestCase("Выдать лоху из лохнесса заказ", "Выдать **** из лохнесса заказ")]
        // [TestCase("scunthorpe cunt", "scunthorpe ****")]
        // [TestCase("cunt scunthorpe cunt scunthorpe cunt", "**** scunthorpe **** scunthorpe ****")]
        public void CensoredString_ReturnsString_WithDoubleScunthorpeBasedDoubleCunt(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(expected, censored);
        }

        [TestCase("2 girls 1 cup, is my favourite twatting video.", '@',
            "@ @@@@@ @ @@@, is my favourite @@@@@@@@ video.")]
        [TestCase("2 girls 1 cup, is my favourite twatting video.", '/',
            "/ ///// / ///, is my favourite //////// video.")]
        [TestCase("2 girls 1 cup, is my favourite twatting video.", '\"',
            "\" \"\"\"\"\" \" \"\"\", is my favourite \"\"\"\"\"\"\"\" video.")]
        [TestCase("2 girls 1 cup, is my favourite twatting video.", '!',
            "! !!!!! ! !!!, is my favourite !!!!!!!! video.")]
        public void CensoredString_ReturnsStringWithProfanities_WithDifferentCharacter(string input,
            char censorCharacter, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input, censorCharacter);

            Assert.AreEqual(expected, censored);
        }

        [Test]
        public void CensoredString_ReturnsEmptyString()
        {
            var filter = CreateProfanityFilter();

            var censored = filter.CensorString("", '@');
            var result = "";

            Assert.AreEqual(censored, result);
        }

        [TestCase("     ", "     ")]
        [TestCase("Hello, I am a fish.", "Hello, I am a fish.")]
        public void CensoredString_ReturnsString_WithNoCensorship(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(censored, expected);
        }

        [TestCase("!@£$*&^&$%^$£%£$@£$£@$£$%%^", "!@£$*&^&$%^$£%£$@£$£@$£$%%^")]
        [TestCase("     ", "     ")]
        [TestCase("Hello you little fuck ", "Hello you little **** ")]
        public void CensoredString_ReturnsCensoredString(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input);
            Assert.AreEqual(expected, censored);
        }

        [TestCase("You are a motherfucker1", "You are a *************")]
        [TestCase("You are a motherfucker123", "You are a ***************")]
        [TestCase("You are a 1motherfucker", "You are a *************")]
        [TestCase("You are a 123motherfucker", "You are a ***************")]
        [TestCase("You are a 123motherfucker123", "You are a ******************")]
        [TestCase("motherfucker1", "*************")]
        [TestCase("motherfucker1  ", "*************  ")]
        [TestCase("  motherfucker1", "  *************")]
        [TestCase("  motherfucker1  ", "  *************  ")]
        [TestCase("You are a motherfucker1 and a fucking twat3.", "You are a ************* and a ******* *****.")]
        public void CensorString_ReturnsCensoredString_WithNumbers(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input, '*', true);

            Assert.AreEqual(expected, censored);
        }

        [TestCase("You are a motherfucker1 and a 'fucking twat3'.",
            "You are a ************* and a '******* *****'.")]
        [TestCase("I've had 10 beers, and you are a motherfucker1 and a 'fucking twat3'.",
            "I've had 10 beers, and you are a ************* and a '******* *****'.")]
        public void CensorString_ReturnsCensoredString_WithQuotes(string input, string expected)
        {
            var filter = CreateProfanityFilter();
            var censored = filter.CensorString(input, '*', true);

            Assert.AreEqual(censored, expected);
        }

        [TestCase("")]
        [TestCase(null)]
        public void CensoredString_ReturnsEmptyString(string input)
        {
            var filter = CreateProfanityFilter();

            var censored = filter.CensorString("");
            const string expected = "";

            Assert.AreEqual(expected, censored);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public void ContainsProfanity_ReturnsFalse_IfNullOrEmptyInputString(string input)
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord(input);

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsTrue_WhenProfanityExists()
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord("Scunthorpe");

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsTrue_WhenMultipleProfanitiesExist()
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord("Scuntarsefuckhorpe");

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsFalse_WhenMultipleProfanitiesExistAndAreAllowed()
        {
            var filter = CreateProfanityFilter();
            filter.AllowList.Add("cunt");
            filter.AllowList.Add("arse");

            var result = filter.ContainsByWord("Scuntarsehorpe");

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsFalse_WhenProfanityDoesNotExist()
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord("Ireland");

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsFalse_WhenProfanityIsAaa()
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord("aaa");

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsProfanity_ReturnsTrue_WhenProfanityIsADollarDollar()
        {
            var filter = CreateProfanityFilter();
            var result = filter.ContainsByWord("a$$");

            Assert.IsTrue(result);
        }

        private ProfanityFilter CreateProfanityFilter()
        {
            var filter = new ProfanityFilter();
            filter.AddProfanityWords(ProfanitiesDictionary.Words);
            filter.AddProfanityPatterns(ProfanitiesDictionary.Patterns);
            return filter;
        }
    }
}