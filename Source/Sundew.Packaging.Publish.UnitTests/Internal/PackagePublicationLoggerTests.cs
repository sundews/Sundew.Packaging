﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagePublicationLoggerTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal;

using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Sundew.Base.Text;
using Sundew.Packaging.Publish.Internal;
using Sundew.Packaging.Versioning;
using Sundew.Packaging.Versioning.Logging;
using Xunit;

public class PackagePublicationLoggerTests
{
    private const string ExpectedPackageId = "PackageId";
    private const string ExpectedVersion = "1.0.0";
    private const string Source = @"http://nuget.org";
    private const string PackagePath = @"c:\PackageId.nupkg";
    private readonly ILogger logger;
    private readonly PackagePublicationLogger testee;

    public PackagePublicationLoggerTests()
    {
        this.logger = New.Mock<ILogger>();
        this.testee = new PackagePublicationLogger(this.logger);
    }

    [Theory]
    [InlineData(null, new string[0])]
    [InlineData(Strings.Empty, new string[0])]
    [InlineData("|", new string[0])]
    [InlineData("||", new[] { "|" })]
    [InlineData("T|", new[] { "T" })]
    [InlineData("T||", new[] { "T|", })]
    [InlineData("{15}vso[task.setvariable package_{0}={0}]{3}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
    [InlineData("{Parameter}vso[task.setvariable package_{0}={0}]{3}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
    [InlineData("{15}vso[task.setvariable package_{0}={0}]{3}|##vso[task.setvariable source_{0}={0}]{7}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
    [InlineData("{Parameter}vso[task.setvariable package_{0}={0}]{3}|##vso[task.setvariable source_{0}={0}]{7}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
    [InlineData("MessageWithSemiColon||ShouldNotSplitWhenEscaped", new[] { @"MessageWithSemiColon|ShouldNotSplitWhenEscaped" })]
    [InlineData("1|2|3", new[] { @"1", "2", "3" })]
    public void Log_Then_ActualMessageShouldBeExpectedResult(string? packagePushFormats, string[] expectedResult)
    {
        var actualMessages = new List<string>();
        this.logger.Setup(x => x.LogImportant(It.IsAny<string>())).Callback<string>(x => actualMessages.Add(x));
        var publishInfo = new PublishInfo(string.Empty, string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion, ExpectedVersion, null);

        this.testee.Log(packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, string.Empty, "##");

        actualMessages.Should().Equal(expectedResult);
    }

    [Theory]
    [InlineData("{8}{10}{11}|{1}", 1)]
    [InlineData("{ApiKey}{SymbolsPath}{SymbolsPushSource}|{Version}", 1)]
    [InlineData("{8}|{10}|{11}", 0)]
    [InlineData("{{6}}", 1)]
    [InlineData("{{{1}", 1)]
    [InlineData("{1}}}", 1)]
    [InlineData(@"{15}{3}{15},{15}{7}{15},{8},{15}{10}{15},{15}{11}{15},{15}{13}{15}{16}", 0)]
    [InlineData(@"{15}{3}{15},{15}{7}{15},{15}{12}{15}{16}", 1)]
    [InlineData(@"{DQ}{3}{DQ},{DQ}{7}{DQ},{DQ}{12}{DQ}{NL}", 1)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    public void Log_When_FormatMayReferenceANullValue_Then_LogImportantShouldCalledExpectedNumberOfTimes(string? packagePushFormats, int numberOfCalls)
    {
        var publishInfo = new PublishInfo(string.Empty, string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion, ExpectedVersion, null);

        this.testee.Log(packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, string.Empty, "##");

        this.logger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Exactly(numberOfCalls));
    }
}