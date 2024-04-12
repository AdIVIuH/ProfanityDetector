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
using FluentAssertions;
using NUnit.Framework;

namespace ProfanityFilter.Tests.Unit;

[TestFixture]
public class CensorStringTests : BaseTest
{
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
    [TestCase("ScUnThOrPePeNiStOnE", "ScUnThOrPePeNiStOnE")] //TODO не верный тест? тут есть cunt и penis
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

    [TestCase("Ебаный рот этого казино, блять. Ты кто такой сука?",
        "****** рот этого казино, *****. Ты кто такой ****?")]
    [TestCase("Выдать заказ лоху", "Выдать заказ ****")]
    [TestCase("Выдать лоху из лохнесса заказ", "Выдать **** из лохнесса заказ")]
    [TestCase("scunthorpe cunt", "scunthorpe ****")]
    [TestCase("scunthorpe cunt1", "scunthorpe *****")]
    [TestCase("scunthorpe $cunt", "scunthorpe *****")]
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
        var censored = filter.CensorString(input);

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
        var censored = filter.CensorString(input);

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

    [TestCase("FrOnT EnD", "FrOnT ***")]
    [TestCase("EMPIRE", "******")]
    public void CensorString_ReturnsCensoredString_IgnoreCase(string input, string expected)
    {
        var filter = CreatePoliteFilter();
        var censored = filter.CensorString(input);

        Assert.AreEqual(expected, censored);
    }

    [TestCase("жаба", "****")]
    [TestCase("жаба<>трава", "***********")]
    [TestCase("home<>трава", "***********")]
    public void CensorString_ReturnsCensoredString_WithLessGreaterSign(string input, string expected)
    {
        var filter = CreatePoliteFilter();
        var censored = filter.CensorString(input);

        Assert.AreEqual(expected, censored);
    }

    [TestCase("h12ome", "******")]
    public void CensorString_ReturnsCensoredString_WithDoubleNumbers(string input, string expected)
    {
        var filter = CreatePoliteFilter();
        var censored = filter.CensorString(input);

        Assert.AreEqual(expected, censored);
    }

    private ProfanityFilter CreatePoliteFilter()
    {
        var filter = new ProfanityFilter();
        filter.AddProfanityWords(new[] { "luggage", "empire", "home", "end", "козёл", "жаба" });
        return filter;
    }
}