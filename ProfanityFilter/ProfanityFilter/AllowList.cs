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

namespace ProfanityFilter;

public class AllowList
{
    /// <summary>
    /// The storage for strings added in the allow list
    /// </summary>
    private readonly HashSet<string> _allowListHashSet = new();

    /// <summary>
    /// Adds a input to the profanity allow list. This means a input that is in the allow list
    /// can be ignored. All inputs are treated as case insensitive.
    /// </summary>
    /// <param name="inputToAllowList">The input that you want to add to allow list.</param>
    public void Add(string inputToAllowList)
    {
        if (string.IsNullOrEmpty(inputToAllowList))
            throw new ArgumentNullException(nameof(inputToAllowList));

        _allowListHashSet.Add(NormalizeString(inputToAllowList));
    }


    /// <summary>
    /// Adds a list of inputs to the profanity allow list. This means a input that is in the allow list
    /// can be ignored. All inputs are treated as case insensitive.
    /// </summary>
    /// <param name="allowedWords">The input that you want to allow list.</param>
    public void AddRange(string[] allowedWords)
    {
        if (allowedWords is null)
            throw new ArgumentNullException(nameof(allowedWords));

        foreach (var allowedWord in allowedWords)
            Add(allowedWord);
    }

    /// <summary>
    /// Checks input existence in the allow list
    /// </summary>
    /// <param name="termToCheck">The term to check on the existence in the allow list</param>
    /// <returns></returns>
    public bool Contains(string termToCheck)
    {
        if (string.IsNullOrEmpty(termToCheck)) throw new ArgumentNullException(nameof(termToCheck));

        return _allowListHashSet.Contains(NormalizeString(termToCheck));
    }

    /// <summary>
    /// Returns the number of items in the allow list.
    /// </summary>
    /// <returns>The number of items in the allow list.</returns>
    public int Count => _allowListHashSet.Count;

    /// <summary>
    /// Removes all inputs from the allow list.
    /// </summary>  
    public void Clear()
    {
        _allowListHashSet.Clear();
    }

    /// <summary>
    /// Removes a input from the profanity allow list. All inputs are treated as case insensitive.
    /// </summary>
    /// <param name="termToRemove">The input that you want to use</param>
    /// <returns>True if the input is successfully removes, False otherwise.</returns>
    public bool Remove(string termToRemove)
    {
        if (string.IsNullOrEmpty(termToRemove)) throw new ArgumentNullException(nameof(termToRemove));

        return _allowListHashSet.Remove(NormalizeString(termToRemove));
    }

    private string NormalizeString(string input) =>
        input.ToLower(CultureInfo.InvariantCulture);
}