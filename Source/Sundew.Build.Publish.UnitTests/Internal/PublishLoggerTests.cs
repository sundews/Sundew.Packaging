// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishLoggerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal
{
    using System.Collections.Generic;
    using FluentAssertions;
    using NSubstitute;
    using Sundew.Base.Text;
    using Sundew.Build.Publish.Internal;
    using Sundew.Build.Publish.Internal.Commands;
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
        [InlineData("##vso[task.setvariable package_{0}={0}]{3}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg" })]
        [InlineData("##vso[task.setvariable package_{0}={0}]{3}|##vso[task.setvariable source_{0}={0}]{2}", new[] { @"##vso[task.setvariable package_PackageId=PackageId]c:\PackageId.nupkg", "##vso[task.setvariable source_PackageId=PackageId]http://nuget.org" })]
        [InlineData("MessageWithSemiColon||ShouldNotSplitWhenEscaped", new[] { @"MessageWithSemiColon|ShouldNotSplitWhenEscaped" })]
        [InlineData("1|2|3", new[] { @"1", "2", "3" })]
        public void Log_Then_ActualMessageShouldBeExpectedResult(string packagePushFormats, string[] expectedResult)
        {
            var commandLogger = Substitute.For<ICommandLogger>();
            var actualMessages = new List<string>();
            commandLogger.When(x => x.LogImportant(Arg.Any<string>())).Do(x => actualMessages.Add(x.Arg<string>()));

            PublishLogger.Log(commandLogger, packagePushFormats, ExpectedPackageId, ExpectedVersion, Source, PackagePath);

            actualMessages.Should().Equal(expectedResult);
        }
    }
}