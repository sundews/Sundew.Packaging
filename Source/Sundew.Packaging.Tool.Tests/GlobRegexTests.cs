﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobRegexTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests;

using FluentAssertions;
using NUnit.Framework;
using Sundew.Packaging.RegularExpression;
using Sundew.Packaging.Testing;

[TestFixture]
public class GlobRegexTests
{
    [TestCase(@"c:\a\b\c\d\file.txt", true)]
    [TestCase(@"c:\a\b\d\file.txt", true)]
    [TestCase(@"c:\a\d\file.txt", true)]
    [TestCase(@"c:\d\file.txt", false)]
    [TestCase(@"c:\a\file.txt", false)]
    public void IsMatch_When_MatchingWithDoubleStar_Then_ResultShouldBeAsExpected(string input, bool expectedResult)
    {
        var regex = GlobRegex.Create(Paths.EnsurePlatformPath(@"c:\a\**\d\file.txt"));

        var result = regex.IsMatch(Paths.EnsurePlatformPath(input));

        result.Should().Be(expectedResult);
    }

    [TestCase(@"c:\a\b\c\d\file.txt", true)]
    [TestCase(@"c:\a\b\c\d\file.bmp", false)]
    [TestCase(@"c:\a\b\d\file.txt", true)]
    [TestCase(@"c:\a\d\file.txt", true)]
    [TestCase(@"c:\d\file.txt", false)]
    [TestCase(@"c:\a\file.txt", true)]
    [TestCase(@"c:/a/b/c/d/file.txt", true)]
    [TestCase(@"c:/a/b/c/d/file.bmp", false)]
    [TestCase(@"c:/a/b/d/file.txt", true)]
    [TestCase(@"c:/a/d/file.txt", true)]
    [TestCase(@"c:/d/file.txt", false)]
    [TestCase(@"c:/a/file.txt", true)]
    public void IsMatch_When_MatchingWithDoubleAndSingleStarAndInputUsesSlash_Then_ResultShouldBeAsExpected(string input, bool expectedResult)
    {
        var regex = GlobRegex.Create(Paths.EnsurePlatformPath(@"c:\a\**\*.txt"));

        var result = regex.IsMatch(Paths.EnsurePlatformPath(input));

        result.Should().Be(expectedResult);
    }

    [TestCase(@"c:\a\b\c\d\file.txt", true)]
    [TestCase(@"c:\a\b\c\d\file.bmp", false)]
    [TestCase(@"c:\a\b\d\file.txt", true)]
    [TestCase(@"c:\a\d\file.txt", true)]
    [TestCase(@"c:\d\file.txt", false)]
    [TestCase(@"c:\a\file.txt", true)]
    [TestCase(@"c:/a/b/c/d/file.txt", true)]
    [TestCase(@"c:/a/b/c/d/file.bmp", false)]
    [TestCase(@"c:/a/b/d/file.txt", true)]
    [TestCase(@"c:/a/d/file.txt", true)]
    [TestCase(@"c:/d/file.txt", false)]
    [TestCase(@"c:/a/file.txt", true)]
    public void IsMatch_When_MatchingWithDoubleAndSingleStarAndInputUsesBackslash_Then_ResultShouldBeAsExpected(string input, bool expectedResult)
    {
        var regex = GlobRegex.Create(Paths.EnsurePlatformPath(@"c:/a/**/*.txt"));

        var result = regex.IsMatch(Paths.EnsurePlatformPath(input));

        result.Should().Be(expectedResult);
    }

    [TestCase(@"c:\a\b\c\d\file.txt", false)]
    [TestCase(@"c:\a\b\c\d\file.bmp", false)]
    [TestCase(@"c:\a\b\d\file.txt", false)]
    [TestCase(@"c:\a\d\file.txt", false)]
    [TestCase(@"c:\d\file.txt", false)]
    [TestCase(@"c:\a\file.txt", true)]
    public void IsMatch_When_MatchingWithNoWildcards_Then_ResultShouldBeAsExpected(string input, bool expectedResult)
    {
        var regex = GlobRegex.Create(Paths.EnsurePlatformPath(@"c:\a\file.txt"));

        var result = regex.IsMatch(Paths.EnsurePlatformPath(input));

        result.Should().Be(expectedResult);
    }
}