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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProfanityFilter.Tests.Unit
{
    [TestClass]
    public class ProfanityTests
    {
        [TestMethod]
        public void ConstructorSetsAllowList()
        {
            var filter = new ProfanityFilter();
            Assert.IsNotNull(filter.AllowList);
        }

        [DataTestMethod]
        [DataRow("arsehole")]
        [DataRow("shitty")]
        public void IsProfanity_ReturnsTrue_ForSwearWord(string swearWord)
        {
            var filter = new ProfanityFilter();
            Assert.IsTrue(filter.IsProfanity(swearWord));
        }

        [DataTestMethod]
        [DataRow("fluffy")]
        [DataRow("")]
        [DataRow(null)]
        public void IsProfanity_ReturnsFalse_ForNonSwearWord(string nonSwearWord)
        {
            var filter = new ProfanityFilter();
            Assert.IsFalse(filter.IsProfanity(nonSwearWord));
        }

        [DataTestMethod]
        [DataRow("shitty")]
        [DataRow("лох")]
        [DataRow("Лох")]
        [DataRow("лоХ")]
        public void IsProfanity_ReturnsFalse_ForWordOnTheAllowList(string word)
        {
            var filter = new ProfanityFilter();
            Assert.IsTrue(filter.IsProfanity(word));

            filter.AllowList.Add(word);

            Assert.IsFalse(filter.IsProfanity(word));
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void DetectAllProfanities_ReturnsEmptyList_ForEmptyInput(string input)
        {
            var filter = new ProfanityFilter();
            var swearList = filter.DetectAllProfanities(string.Empty);

            Assert.AreEqual(0, swearList.Count);
        }

        [DataTestMethod]
        [DataRow("You are a complete twat and a dick.", "twat", "dick")]
        [DataRow("You are, a complete twat, and a @dick:", "twat", "dick")]
        [DataRow("You are a complete tWat and a DiCk.", "twat", "dick")]
        public void DetectAllProfanities_Returns2SwearWords(string input, params string[] expected)
        {
            var filter = new ProfanityFilter();
            var swearList = filter.DetectAllProfanities(input);

            Assert.AreEqual(expected.Length, swearList.Count);
            Assert.IsFalse(expected.Except(swearList).Any());
        }

        [DataTestMethod]
        [DataRow("2 girls 1 cup is my favourite video", "2 girls 1 cup")]
        [DataRow("2 girls 1 cup is my favourite twatting video", "2 girls 1 cup", "twatting")]
        public void DetectAllProfanities_ReturnsSwearPhrases(string input, params string[] expected)
        {
            var filter = new ProfanityFilter();
            var swearList = filter.DetectAllProfanities(input);

            Assert.AreEqual(expected.Length, swearList.Count);
            Assert.IsFalse(expected.Except(swearList).Any());
        }

        [TestMethod]
        public void DetectAllProfanities_Returns2SwearPhrase_BecauseOfMatchDeduplication(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var swearList = filter.DetectAllProfanities("2 girls 1 cup is my favourite twatting video", true);

            Assert.AreEqual(2, swearList.Count);
            Assert.AreEqual("2 girls 1 cup", swearList[0]);
            Assert.AreEqual("twatting", swearList[1]);
        }

        [TestMethod]
        public void DetectAllProfanities_Scunthorpe()
        {
            var filter = new ProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var swearList = filter.DetectAllProfanities(
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.");

            Assert.AreEqual(4, swearList.Count);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fuck", swearList[2]);
            Assert.AreEqual("shit", swearList[3]);
        }

        [TestMethod]
        public void DetectAllProfanities_ScunthorpeWithDuplicatesTurnedOff()
        {
            var filter = new ProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var swearList = filter.DetectAllProfanities(
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
                true);

            Assert.AreEqual(3, swearList.Count);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("shit", swearList[2]);
        }

        [TestMethod]
        public void DetectAllProfanities_BlocksAllowList()
        {
            var filter = new ProfanityFilter();
            filter.AllowList.Add("tit");

            var swearList = filter.DetectAllProfanities("You are a complete twat and a total tit.", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("twat", swearList[0]);
        }

        [TestMethod]
        public void DetectAllProfanities_ScunthorpeWithDuplicatesTurnedOffAndNoAllowList()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities(
                "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
                true);

            Assert.AreEqual(3, swearList.Count);
            Assert.AreEqual("cock", swearList[1]);
            Assert.AreEqual("fucking", swearList[0]);
            Assert.AreEqual("shit", swearList[2]);
        }

        [TestMethod]
        public void DetectAllProfanitiesMultipleScunthorpes()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe Scunthorpe", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [TestMethod]
        public void DetectAllProfanitiesMultipleScunthorpesSingleCunt()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe cunt Scunthorpe", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cunt", swearList[0]);
        }

        [TestMethod]
        public void DetectAllProfanitiesMultipleScunthorpesMultiCunt()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("Scunthorpe cunt Scunthorpe cunt", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cunt", swearList[0]);
        }

        [TestMethod]
        public void DetectAllProfanitiesScunthorpePenistone()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("ScUnThOrPePeNiStOnE", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [TestMethod]
        public void DetectAllProfanitiesScunthorpePenistoneOneKnob()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("ScUnThOrPePeNiStOnE KnOb", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("knob", swearList[0]);
        }

        [TestMethod]
        public void DetectAllProfanitiesLongerSentence()
        {
            var filter = new ProfanityFilter();

            var swearList =
                filter.DetectAllProfanities(
                    "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.", true);

            Assert.AreEqual(4, swearList.Count);
            Assert.AreEqual("alaskan pipeline", swearList[0]);
            Assert.AreEqual("blow your load", swearList[1]);
            Assert.AreEqual("stupid", swearList[2]);
            Assert.AreEqual("twat", swearList[3]);
        }

        [TestMethod]
        public void DetectAllProfanities_ForSingleWord()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("cock", true);

            Assert.AreEqual(1, swearList.Count);
            Assert.AreEqual("cock", swearList[0]);
        }

        [TestMethod]
        public void DetectAllProfanitiesFor_EmptyString()
        {
            var filter = new ProfanityFilter();

            var swearList = filter.DetectAllProfanities("", true);

            Assert.AreEqual(0, swearList.Count);
        }

        [TestMethod]
        public void CensoredString_ReturnsStringWithProfanities_BleepedOut()
        {
            var filter = new ProfanityFilter();
            filter.AllowList.Add("scunthorpe");
            filter.AllowList.Add("penistone");

            var censored = filter.CensorString("Выдать заказ лоху", '*');
            var result = "Выдать заказ ****";

            Assert.AreEqual(result, censored);
        }

        [DataTestMethod]
        [DataRow(
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            "I ******* live in Scunthorpe and it is a **** place to live. I would much rather live in penistone you great big **** ****.")]
        [DataRow(
            "I Fucking Live In Scunthorpe And It Is A Shit Place To Live. I Would Much Rather Live In Penistone You Great Big Cock Fuck.",
            "I ******* Live In Scunthorpe And It Is A **** Place To Live. I Would Much Rather Live In Penistone You Great Big **** ****.")]
        [DataRow(
            "2 girls 1 cup, is my favourite twatting video.",
            "* ***** * ***, is my favourite ******** video.")]
        [DataRow(
            "Mary had a little shit lamb who was a little fucker.",
            "Mary had a little **** lamb who was a little ******.")]
        [DataRow(
            "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.",
            "You are a ****** little ****, and you like to **** **** **** in an ******* ********.")]
        [DataRow("Scunthorpe", "Scunthorpe")]
        [DataRow("ScUnThOrPe", "ScUnThOrPe")]
        [DataRow("ScUnThOrPePeNiStOnE", "ScUnThOrPePeNiStOnE")]
        [DataRow("scunthorpe", "scunthorpe")]
        public void CensoredString_ReturnsStringWithProfanities_BleepedOutNoAllowList(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input, '*');

            Assert.AreEqual(expected, censored);
        }

        [TestMethod]
        public void CensoredString_ReturnsStringDoubleCunt()
        {
            var filter = new ProfanityFilter();

            var censored = filter.CensorString("cunt cunt");
            var result = "**** ****";

            Assert.AreEqual(censored, result);
        }

        [DataTestMethod]
        [DataRow("Ебаный рот этого казино, блять. Ты кто такой сука?", "****** рот этого казино, *****. Ты кто такой ****?")]
        [DataRow("Выдать заказ лоху", "Выдать заказ ****")]
        [DataRow("Выдать лоху из лохнесса заказ", "Выдать **** из лохнесса заказ")]
        // [DataRow("scunthorpe cunt", "scunthorpe ****")]
        // [DataRow("cunt scunthorpe cunt scunthorpe cunt", "**** scunthorpe **** scunthorpe ****")]
        public void CensoredString_ReturnsString_WithDoubleScunthorpeBasedDoubleCunt(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(expected, censored);
        }

        [DataTestMethod]
        [DataRow("2 girls 1 cup, is my favourite twatting video.", '@',
            "@ @@@@@ @ @@@, is my favourite @@@@@@@@ video.")]
        [DataRow("2 girls 1 cup, is my favourite twatting video.", '/',
            "/ ///// / ///, is my favourite //////// video.")]
        [DataRow("2 girls 1 cup, is my favourite twatting video.", '\"',
            "\" \"\"\"\"\" \" \"\"\", is my favourite \"\"\"\"\"\"\"\" video.")]
        [DataRow("2 girls 1 cup, is my favourite twatting video.", '!',
            "! !!!!! ! !!!, is my favourite !!!!!!!! video.")]
        public void CensoredString_ReturnsStringWithProfanities_WithDifferentCharacter(string input,
            char censorCharacter, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input, censorCharacter);

            Assert.AreEqual(expected, censored);
        }

        [TestMethod]
        public void CensoredString_ReturnsEmptyString()
        {
            var filter = new ProfanityFilter();

            var censored = filter.CensorString("", '@');
            var result = "";

            Assert.AreEqual(censored, result);
        }

        [DataTestMethod]
        [DataRow("     ", "     ")]
        [DataRow("Hello, I am a fish.", "Hello, I am a fish.")]
        public void CensoredString_ReturnsString_WithNoCensorship(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(censored, expected);
        }

        [DataTestMethod]
        [DataRow("!@£$*&^&$%^$£%£$@£$£@$£$%%^", "!@£$*&^&$%^$£%£$@£$£@$£$%%^")]
        [DataRow("     ", "     ")]
        [DataRow("Hello you little fuck ", "Hello you little **** ")]
        public void CensoredString_ReturnsCensoredString(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input);

            Assert.AreEqual(censored, expected);
        }

        [DataTestMethod]
        [DataRow("You are a motherfucker1", "You are a *************")]
        [DataRow("You are a motherfucker123", "You are a ***************")]
        [DataRow("You are a 1motherfucker", "You are a *************")]
        [DataRow("You are a 123motherfucker", "You are a ***************")]
        [DataRow("You are a 123motherfucker123", "You are a ******************")]
        [DataRow("motherfucker1", "*************")]
        [DataRow("motherfucker1  ", "*************  ")]
        [DataRow("  motherfucker1", "  *************")]
        [DataRow("  motherfucker1  ", "  *************  ")]
        [DataRow("You are a motherfucker1 and a fucking twat3.", "You are a ************* and a ******* *****.")]
        public void CensorString_ReturnsCensoredString_WithNumbers(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input, '*', true);

            Assert.AreEqual(censored, expected);
        }

        [DataTestMethod]
        [DataRow("You are a motherfucker1 and a 'fucking twat3'.", "You are a ************* and a '******* *****'.")]
        [DataRow("I've had 10 beers, and you are a motherfucker1 and a 'fucking twat3'.",
            "I've had 10 beers, and you are a ************* and a '******* *****'.")]
        public void CensorString_ReturnsCensoredString_WithQuotes(string input, string expected)
        {
            var filter = new ProfanityFilter();
            var censored = filter.CensorString(input, '*', true);

            Assert.AreEqual(censored, expected);
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        public void CensoredString_ReturnsEmptyString(string input)
        {
            var filter = new ProfanityFilter();

            var censored = filter.CensorString("");
            const string expected = "";

            Assert.AreEqual(expected, censored);
        }
        
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("  ")]
        public void ContainsProfanity_ReturnsFalse_IfNullOrEmptyInputString(string input)
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity(input);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsTrue_WhenProfanityExists()
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity("Scunthorpe");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsTrue_WhenMultipleProfanitiesExist()
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity("Scuntarsefuckhorpe");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsFalse_WhenMultipleProfanitiesExistAndAreAllowed()
        {
            var filter = new ProfanityFilter();
            filter.AllowList.Add("cunt");
            filter.AllowList.Add("arse");

            var result = filter.ContainsProfanity("Scuntarsehorpe");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsFalse_WhenProfanityDoesNotExist()
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity("Ireland");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsFalse_WhenProfanityIsAaa()
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity("aaa");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsProfanity_ReturnsTrue_WhenProfanityIsADollarDollar()
        {
            var filter = new ProfanityFilter();
            var result = filter.ContainsProfanity("a$$");

            Assert.IsTrue(result);
        }
    }
}