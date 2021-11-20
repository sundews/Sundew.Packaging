// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseDateTimeProviderTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal;

using System;
using System.Globalization;
using FluentAssertions;
using Moq;
using Sundew.Base.Primitives.Time;
using Sundew.Packaging.Publish.Internal;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;
using Xunit;

public class PrereleaseDateTimeProviderTests
{
    private const string AnyFilePath = "APath";
    private const string AnyDateTimeString = "2016-01-08T17:36:13.0470440Z";
    private static readonly DateTime ExpectedDateTime = DateTime.ParseExact(AnyDateTimeString, PrereleaseDateTimeProvider.UniversalDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    private readonly IFileSystem fileSystem;
    private readonly IDateTime dateTime;
    private readonly PrereleaseDateTimeProvider testee;

    public PrereleaseDateTimeProviderTests()
    {
        this.dateTime = New.Mock<IDateTime>();
        this.fileSystem = New.Mock<IFileSystem>();
        this.testee = new PrereleaseDateTimeProvider(this.fileSystem, this.dateTime, New.Mock<ILogger>());
        this.dateTime.Setup(x => x.UtcNow).Returns(ExpectedDateTime);
    }

    [Fact]
    public void SaveBuildDateTime_Then_DateTimeShouldBeWrittenToFile()
    {
        this.testee.SaveBuildDateTime(AnyFilePath);

        this.fileSystem.Verify(x => x.WriteAllText(AnyFilePath, AnyDateTimeString), Times.Once);
    }

    [Fact]
    public void GetBuildDateTime_Then_ResultShouldBeExpectedDateTime()
    {
        var result = this.testee.GetBuildDateTime(AnyFilePath);

        result.Should().Be(ExpectedDateTime);
    }

    [Fact]
    public void GetBuildDateTime_When_PreviouslySaved_Then_ResultShouldBeExpectedResult()
    {
        string dateTimeText = string.Empty;
        this.fileSystem.Setup(x => x.WriteAllText(AnyFilePath, It.IsAny<string>())).Callback<string, string>((s, time) => dateTimeText = time);
        this.fileSystem.Setup(x => x.ReadAllText(AnyFilePath)).Returns<string>(_ => dateTimeText);

        var dateTime = this.testee.SaveBuildDateTime(AnyFilePath);
        var result = this.testee.GetBuildDateTime(AnyFilePath);

        dateTime.Should().Be(ExpectedDateTime);
        result.Should().Be(ExpectedDateTime);
        result.Should().Be(dateTime);
    }
}