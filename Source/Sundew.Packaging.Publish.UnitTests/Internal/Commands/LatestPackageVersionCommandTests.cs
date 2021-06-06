// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestPackageVersionCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal.Commands
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Packaging.Versioning.Commands;
    using Xunit;

    public class LatestPackageVersionCommandTests
    {
        private readonly LatestPackageVersionCommand testee;

        public LatestPackageVersionCommandTests()
        {
            this.testee = new LatestPackageVersionCommand(New.Mock<Versioning.Logging.ILogger>(), New.Mock<ILogger>());
        }

        [Theory]
        [InlineData("Serilog", "2.7", true, false, "2.7.2-dev-01041")]
        [InlineData("Serilog", "2.7", false, false, "2.7.1")]
        [InlineData("Serilog", "2.2", false, false, "2.2.1")]
        [InlineData("Serilog", "2.2.0", false, true, "2.2.0")]
        public async Task GetLatestVersion_Then_ResultShouldBeExpectedResult(string packageId, string version, bool allowPrerelease, bool includePatchInMatch, string expectedVersion)
        {
            var result = await this.testee.GetLatestMajorMinorVersion(
                packageId,
                new[] { NuGetConstants.V3FeedUrl },
                NuGetVersion.Parse(version),
                includePatchInMatch,
                allowPrerelease);

            result.Should().Be(NuGetVersion.Parse(expectedVersion));
        }
    }
}