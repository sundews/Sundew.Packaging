// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishLoggerTests.cs" company="Hukano">
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
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Logging;
    using Xunit;

    public class PublishLoggerTests
    {
        private const string ExpectedPackageId = "PackageId";
        private const string ExpectedVersion = "1.0.0";
        private const string Source = @"http://nuget.org";
        private const string PackagePath = @"c:\PackageId.nupkg";

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(Strings.Empty, new string[0])]
        [InlineData("|", new string[0])]
        [InlineData("||", new[] { "|" })]
        [InlineData("T|", new[] { "T" })]
        [InlineData("T||", new[] { "T|", })]
        [InlineData("{10}vso[task.setvariable package_{0}={0}]{2}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
        [InlineData("{10}vso[task.setvariable package_{0}={0}]{2}|##vso[task.setvariable source_{0}={0}]{4}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
        [InlineData("MessageWithSemiColon||ShouldNotSplitWhenEscaped", new[] { @"MessageWithSemiColon|ShouldNotSplitWhenEscaped" })]
        [InlineData("1|2|3", new[] { @"1", "2", "3" })]
        public void Log_Then_ActualMessageShouldBeExpectedResult(string packagePushFormats, string[] expectedResult)
        {
            var commandLogger = New.Mock<ILogger>();
            var actualMessages = new List<string>();
            commandLogger.Setup(x => x.LogImportant(It.IsAny<string>())).Callback<string>(x => actualMessages.Add(x));
            var publishInfo = new PublishInfo(string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion);

            PublishLogger.Log(commandLogger, packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, "##");

            actualMessages.Should().Equal(expectedResult);
        }

        [Theory]
        [InlineData("{5}{7}{8}|{1}", 1)]
        [InlineData("{5}|{7}|{8}", 0)]
        [InlineData("{{5}}", 1)]
        [InlineData("{5}}", 0)]
        [InlineData("{{5}", 0)]
        [InlineData("{{{1}", 1)]
        [InlineData("{1}}}", 1)]
        [InlineData(@"{11}{2}{11},{11}{4}{11},{5},{11}{7}{11},{11}{8}{11},{11}{9}{11}{12}", 0)]
        [InlineData(@"{11}{2}{11},{11}{4}{11},{11}{9}{11}{12}", 1)]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        public void Log_When_FormatMayReferenceANullValue_Then_LogImportantShouldCalledExpectedNumberOfTimes(string packagePushFormats, int numberOfCalls)
        {
            var commandLogger = New.Mock<ILogger>();
            var publishInfo = new PublishInfo(string.Empty, Source, Source, null, null, string.Empty, true, ExpectedVersion);

            PublishLogger.Log(commandLogger, packagePushFormats, ExpectedPackageId, PackagePath, null, publishInfo, "##");

            commandLogger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Exactly(numberOfCalls));
        }
    }
}