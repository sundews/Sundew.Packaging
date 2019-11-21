// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomaticPackageVersionerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using NSubstitute;
    using NuGet.Common;
    using NuGet.Versioning;
    using Sundew.Build.Publish.Internal;
    using Sundew.Build.Publish.Internal.Commands;
    using Xunit;

    public class AutomaticPackageVersionerTests
    {
        private const string AnyPackageId = "Package.Id";
        private const string AnyLocalSourcePath = @"c:\temp\NuGet\packages";
        private const string AnyRemoteSourcePath = @"http://nuget.org";
        private readonly AutomaticPackageVersioner testee;
        private readonly IPackageExistsCommand localPackageExistsCommand = Substitute.For<IPackageExistsCommand>();
        private readonly IPackageExistsCommand remotePackageExistsCommand = Substitute.For<IPackageExistsCommand>();
        private readonly ILogger logger = Substitute.For<ILogger>();

        public AutomaticPackageVersionerTests()
        {
            this.testee = new AutomaticPackageVersioner(this.localPackageExistsCommand, this.remotePackageExistsCommand);
        }

        [Theory]
        [InlineData("4.1.0", false, "4.1.0")]
        [InlineData("4.1.0", true, "4.1.1")]
        public async Task GetLatestStableVersion_When_UsingRemoteSource_Then_ResultShouldBeExpectedResult(string currentVersion, bool packageExists, string expectedResult)
        {
            this.remotePackageExistsCommand
                .ExistsAsync(AnyPackageId, Arg.Any<SemanticVersion>(), AnyRemoteSourcePath, this.logger)
                .Returns(Task.FromResult(packageExists));

            var result = await this.testee.GetSemanticVersion(AnyPackageId, SemanticVersion.Parse(currentVersion), AnyRemoteSourcePath, this.logger);

            result.Should().Be(SemanticVersion.Parse(expectedResult));
        }

        [Theory]
        [InlineData("4.1.0", false, "4.1.0")]
        [InlineData("4.1.0", true, "4.1.1")]
        public async Task GetLatestStableVersion_When_UsingLocalSource_Then_ResultShouldBeExpectedResult(string currentVersion, bool packageExists, string expectedResult)
        {
            this.localPackageExistsCommand
                .ExistsAsync(AnyPackageId, Arg.Any<SemanticVersion>(), AnyLocalSourcePath, this.logger)
                .Returns(Task.FromResult(packageExists));

            var result = await this.testee.GetSemanticVersion(AnyPackageId, SemanticVersion.Parse(currentVersion), AnyLocalSourcePath, this.logger);

            result.Should().Be(SemanticVersion.Parse(expectedResult));
        }
    }
}