// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackagePublicationLoggerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using Sundew.Base.Text;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.IO;
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
        [InlineData("{13}vso[task.setvariable package_{0}={0}]{2}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
        [InlineData("{Parameter}vso[task.setvariable package_{0}={0}]{2}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
        [InlineData("{13}vso[task.setvariable package_{0}={0}]{2}|##vso[task.setvariable source_{0}={0}]{5}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
        [InlineData("{Parameter}vso[task.setvariable package_{0}={0}]{2}|##vso[task.setvariable source_{0}={0}]{5}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
        [InlineData("MessageWithSemiColon||ShouldNotSplitWhenEscaped", new[] { @"MessageWithSemiColon|ShouldNotSplitWhenEscaped" })]
        [InlineData("1|2|3", new[] { @"1", "2", "3" })]
        public void Log_Then_ActualMessageShouldBeExpectedResult(string packagePushFormats, string[] expectedResult)
        {
            var actualMessages = new List<string>();
            this.logger.Setup(x => x.LogImportant(It.IsAny<string>())).Callback<string>(x => actualMessages.Add(x));
            var publishInfo = new PublishInfo(string.Empty, string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion, null);

            this.testee.Log(packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, string.Empty, "##");

            actualMessages.Should().Equal(expectedResult);
        }

        [Theory]
        [InlineData("{6}{8}{9}|{1}", 1)]
        [InlineData("{ApiKey}{SymbolsPath}{SymbolsPushSource}|{Version}", 1)]
        [InlineData("{6}|{8}|{9}", 0)]
        [InlineData("{{6}}", 1)]
        [InlineData("{{{1}", 1)]
        [InlineData("{1}}}", 1)]
        [InlineData(@"{13}{2}{13},{13}{5}{13},{6},{13}{8}{13},{13}{9}{13},{13}{10}{13}{14}", 0)]
        [InlineData(@"{13}{2}{13},{13}{5}{13},{13}{10}{13}{14}", 1)]
        [InlineData(@"{DQ}{2}{DQ},{DQ}{5}{DQ},{DQ}{10}{DQ}{NL}", 1)]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        public void Log_When_FormatMayReferenceANullValue_Then_LogImportantShouldCalledExpectedNumberOfTimes(string packagePushFormats, int numberOfCalls)
        {
            var publishInfo = new PublishInfo(string.Empty, string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion, null);

            this.testee.Log(packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, string.Empty, "##");

            this.logger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Exactly(numberOfCalls));
        }
    }
}