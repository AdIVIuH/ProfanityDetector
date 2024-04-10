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
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class ProfanityTests
{
    [Test]
    public void Constructor_SetsAllowList()
    {
        var filter = new ProfanityFilter();
        Assert.IsNotNull(filter.AllowList);
    }

    [TestCase("")]
    [TestCase(null)]
    public void DetectWordsWithProfanities_ReturnsEmptyList_ForEmptyInput(string input)
    {
        var filter = CreateProfanityFilter();
        var profanities = filter.DetectWordsWithProfanities(input);

        Assert.AreEqual(0, profanities.Count);
    }

    [TestCase("You are a complete twat and a dick.", "twat", "dick")]
    [TestCase("You are, a complete twat, and a @dick:", "twat", "dick")]
    [TestCase("You are a complete tWat and a DiCk.", "twat", "dick")]
    public void DetectWordsWithProfanities_Returns2SwearWords(string input, params string[] expected)
    {
        var filter = CreateProfanityFilter();
        var profanities = filter.DetectWordsWithProfanities(input);

        Assert.AreEqual(expected.Length, profanities.Count);
        Assert.IsFalse(expected.Except(profanities).Any());
    }

    [TestCase("2 girls 1 cup and son of a bitch", "2 girls 1 cup", "son of a bitch")]
    [TestCase("2 girls 1 cup is my favourite video", "2 girls 1 cup")]
    [TestCase("2 girls 1 cup is my favourite twatting video", "2 girls 1 cup", "twatting")]
    public void DetectWordsWithProfanities_ReturnsSwearPhrases(string input, params string[] expected)
    {
        var filter = CreateProfanityFilter();
        var swearList = filter.DetectWordsWithProfanities(input);

        swearList.Count.Should().Be(expected.Length);
        swearList.Except(expected).Should().BeEmpty();
    }

    [Test]
    public void DetectWordsWithProfanities_Returns2SwearPhrase_BecauseOfMatchDeduplication()
    {
        var filter = CreateProfanityFilter();
        var swearList = filter.DetectWordsWithProfanities("2 girls 1 cup is my favourite twatting video", true);

        Assert.AreEqual(2, swearList.Count);
        Assert.AreEqual("2 girls 1 cup", swearList[0]);
        Assert.AreEqual("twatting", swearList[1]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithoutAllowList()
    {
        var filter = CreateProfanityFilter();

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: false);

        Assert.AreEqual(4, profanities.Count);
        Assert.AreEqual("fucking", profanities[0]);
        Assert.AreEqual("cock", profanities[1]);
        Assert.AreEqual("fuck", profanities[2]);
        Assert.AreEqual("shit", profanities[3]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithAllowList()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("scunthorpe");
        filter.AllowList.Add("penistone");

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: false);

        Assert.AreEqual(4, profanities.Count);
        Assert.AreEqual("fucking", profanities[0]);
        Assert.AreEqual("cock", profanities[1]);
        Assert.AreEqual("fuck", profanities[2]);
        Assert.AreEqual("shit", profanities[3]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithDuplicatesTurnedOff()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("scunthorpe");
        filter.AllowList.Add("penistone");

        var profanities = filter.DetectWordsWithProfanities(
            sentence:
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            removePartialMatches: true);

        Assert.AreEqual(3, profanities.Count);
        Assert.AreEqual("cock", profanities[1]);
        Assert.AreEqual("fucking", profanities[0]);
        Assert.AreEqual("shit", profanities[2]);
    }

    [Test]
    public void DetectWordsWithProfanities_BlocksAllowList()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("tit");

        var swearList = filter.DetectWordsWithProfanities("You are a complete twat and a total tit.", true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("twat", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_Scunthorpe_WithDuplicatesTurnedOffAndNoAllowList()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities(
            "I fucking live in Scunthorpe and it is a shit place to live. I would much rather live in penistone you great big cock fuck.",
            true);

        Assert.AreEqual(3, swearList.Count);
        Assert.AreEqual("cock", swearList[1]);
        Assert.AreEqual("fucking", swearList[0]);
        Assert.AreEqual("shit", swearList[2]);
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpes()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("Scunthorpe Scunthorpe", true);

        Assert.AreEqual(0, swearList.Count);
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpes_SingleCunt()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("Scunthorpe cunt Scunthorpe");

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("cunt", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_MultipleScunthorpesMultiCunt()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("Scunthorpe cunt Scunthorpe cunt");

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("cunt", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_ScunthorpePenistone()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("ScUnThOrPePeNiStOnE", true);

        Assert.AreEqual(0, swearList.Count);
    }

    [Test]
    public void DetectWordsWithProfanities_ScunthorpePenistone_OneKnob()
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities("ScUnThOrPePeNiStOnE KnOb", true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual("knob", swearList[0]);
    }

    [Test]
    public void DetectWordsWithProfanities_LongerSentence()
    {
        var filter = CreateProfanityFilter();

        var swearList =
            filter.DetectWordsWithProfanities(
                "You are a stupid little twat, and you like to blow your load in an alaskan pipeline.", true);

        Assert.AreEqual(4, swearList.Count);
        Assert.AreEqual("alaskan pipeline", swearList[0]);
        Assert.AreEqual("blow your load", swearList[1]);
        Assert.AreEqual("stupid", swearList[2]);
        Assert.AreEqual("twat", swearList[3]);
    }

    [TestCase("cock")]
    public void DetectWordsWithProfanities_ForSingleWord(string word)
    {
        var filter = CreateProfanityFilter();

        var swearList = filter.DetectWordsWithProfanities(word, true);

        Assert.AreEqual(1, swearList.Count);
        Assert.AreEqual(word, swearList[0]);
    }

    [Test]
    public void CensorString_ReturnsStringWithProfanities_BleepedOut()
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
    public void CensorString_ReturnsStringWithProfanities_BleepedOutNoAllowList(string input, string expected)
    {
        var filter = CreateProfanityFilter();
        var censored = filter.CensorString(input);

        Assert.AreEqual(expected, censored);
    }

    [TestCase("cunt cunt", "**** ****")]
    [TestCase("a cunt a cunt", "a **** a ****")]
    [TestCase("a лох a лоха", "a *** a ****")]
    [TestCase("'лоха, для &лоха2", "'****, для &*****")]
    public void CensorString_ReturnsString_WithMultipleEqualProfanities(string input, string expected)
    {
        var filter = CreateProfanityFilter();

        var censored = filter.CensorString(input);

        Assert.AreEqual(expected, censored);
    }

    [TestCase("Ебаный рот этого казино, блять. Ты кто такой сука?", "****** рот этого казино, *****. Ты кто такой ****?")]
    [TestCase("Выдать заказ лоху", "Выдать заказ ****")]
    [TestCase("Выдать лоху из лохнесса заказ", "Выдать **** из лохнесса заказ")]
    [TestCase("scunthorpe cunt", "scunthorpe ****")]
    [TestCase("cunt scunthorpe cunt scunthorpe cunt", "**** scunthorpe **** scunthorpe ****")]
    public void CensorString_ReturnsString_WithDoubleScunthorpeBasedDoubleCunt(string input, string expected)
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
    public void CensorString_ReturnsStringWithProfanities_WithDifferentCharacter(string input,
        char censorCharacter, string expected)
    {
        var filter = CreateProfanityFilter();
        var censored = filter.CensorString(input, censorCharacter);

        Assert.AreEqual(expected, censored);
    }

    [Test]
    public void CensorString_ThrowsArgumentNullException_ForNullInput()
    {
        var filter = CreateProfanityFilter();
        var action = () => filter.CensorString(null);

        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CensorString_ReturnsEmptyString()
    {
        var filter = CreateProfanityFilter();

        var censored = filter.CensorString("");
        var result = "";

        Assert.AreEqual(censored, result);
    }

    [TestCase("     ", "     ")]
    [TestCase("Hello, I am a fish.", "Hello, I am a fish.")]
    public void CensorString_ReturnsString_WithNoCensorship(string input, string expected)
    {
        var filter = CreateProfanityFilter();
        var censored = filter.CensorString(input);

        Assert.AreEqual(censored, expected);
    }

    [TestCase("!@£$*&^&$%^$£%£$@£$£@$£$%%^", "!@£$*&^&$%^$£%£$@£$£@$£$%%^")]
    [TestCase("     ", "     ")]
    [TestCase("Hello you little fuck ", "Hello you little **** ")]
    public void CensorString_ReturnsCensoredString(string input, string expected)
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
    [TestCase("motherfucker1.", "*************.")]
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

    [TestCase("a 'fucking twat3'.",
        "a '******* *****'.")]
    [TestCase("a \"fucking twat3\".",
        "a \"******* *****\".")]
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
    public void CensorString_ReturnsEmptyString(string input)
    {
        var filter = CreateProfanityFilter();

        var censored = filter.CensorString("");
        const string expected = "";

        Assert.AreEqual(expected, censored);
    }

    [TestCase("fluffy")]
    [TestCase("")]
    [TestCase(null)]
    public void HasAnyProfanities_ReturnsFalse_ForNonSwearWord(string nonSwearWord)
    {
        var filter = CreateProfanityFilter();
        Assert.IsFalse(filter.HasAnyProfanities(nonSwearWord));
    }

    [TestCase("shitty")]
    [TestCase("лох")]
    [TestCase("Лох")]
    [TestCase("лоХ")]
    public void HasAnyProfanities_ReturnsFalse_ForWordOnTheAllowList(string word)
    {
        var filter = CreateProfanityFilter();
        Assert.IsTrue(filter.HasAnyProfanities(word));

        filter.AllowList.Add(word);

        Assert.IsFalse(filter.HasAnyProfanities(word));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("  ")]
    public void HasAnyProfanities_ReturnsFalse_IfNullOrEmptyInputString(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenMultipleProfanitiesExistAndAreAllowed()
    {
        var filter = CreateProfanityFilter();
        filter.AllowList.Add("cunt");
        filter.AllowList.Add("arse");

        var result = filter.HasAnyProfanities("Scuntarsehorpe");

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityDoesNotExist()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("Ireland");

        Assert.IsFalse(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityAddedAsWord_AndInAllowList()
    {
        var filter = new ProfanityFilter();
        const string profanity = "test";

        filter.AddProfanityWord(profanity);
        Assert.IsTrue(filter.HasAnyProfanities(profanity));

        filter.AllowList.Add(profanity);
        Assert.IsFalse(filter.HasAnyProfanities(profanity));
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityAddedAsPattern_AndInAllowList()
    {
        var filter = new ProfanityFilter();
        const string profanity = "test";

        filter.AddProfanityPattern(profanity);
        Assert.IsTrue(filter.HasAnyProfanities(profanity));

        filter.AllowList.Add(profanity);
        Assert.IsFalse(filter.HasAnyProfanities(profanity));
    }

    [Test]
    public void HasAnyProfanities_ReturnsFalse_WhenProfanityIsAaa()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("aaa");

        Assert.IsFalse(result);
    }

    [TestCase("arsehole")]
    [TestCase("shitty")]
    public void HasAnyProfanities_ReturnsTrue_ForSwearWord(string swearWord)
    {
        var filter = CreateProfanityFilter();
        Assert.IsTrue(filter.HasAnyProfanities(swearWord));
    }

    [Test]
    public void HasAnyProfanities_ReturnsTrue_WhenProfanityExists()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("Scunthorpe");

        Assert.IsTrue(result);
    }

    [TestCase("Scuntarsefuckhorpe")]
    [TestCase("fuckлох")]
    [TestCase("fuckingfuck")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InWordsList(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [TestCase("лохfuck")]
    [TestCase("ПРЕФИКСлохfuck")]
    [TestCase("ПРЕФИКСлохfuckПОСТФИКС")]
    [TestCase("лохfuckПОСТФИКС")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InPatterns(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [TestCase("лохfuck")]
    [TestCase("ПРЕФИКСлохfuck")]
    [TestCase("ПРЕФИКСлохfuckПОСТФИКС")]
    [TestCase("лохfuckПОСТФИКС")]
    public void HasAnyProfanities_ReturnsTrue_WhenMultipleProfanitiesExist_InWordsOrPatterns(string input)
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities(input);

        Assert.IsTrue(result);
    }

    [Test]
    public void HasAnyProfanities_ReturnsTrue_WhenProfanityIsADollarDollar()
    {
        var filter = CreateProfanityFilter();
        var result = filter.HasAnyProfanities("a$$");

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