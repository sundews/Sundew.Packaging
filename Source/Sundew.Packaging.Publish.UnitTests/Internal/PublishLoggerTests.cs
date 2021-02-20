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
    using Sundew.Packaging.Publish.Internal.Commands;
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
        [InlineData("##vso[task.setvariable package_{0}={0}]{2}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
        [InlineData("##vso[task.setvariable package_{0}={0}]{2}|##vso[task.setvariable source_{0}={0}]{3}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
        [InlineData("MessageWithSemiColon||ShouldNotSplitWhenEscaped", new[] { @"MessageWithSemiColon|ShouldNotSplitWhenEscaped" })]
        [InlineData("1|2|3", new[] { @"1", "2", "3" })]
        public void Log_Then_ActualMessageShouldBeExpectedResult(string packagePushFormats, string[] expectedResult)
        {
            var commandLogger = New.Mock<ICommandLogger>();
            var actualMessages = new List<string>();
            commandLogger.Setup(x => x.LogImportant(It.IsAny<string>())).Callback<string>(x => actualMessages.Add(x));

            PublishLogger.Log(commandLogger, packagePushFormats, ExpectedPackageId, ExpectedVersion, PackagePath, Source, null, null, null, null);

            actualMessages.Should().Equal(expectedResult);
        }
    }
}