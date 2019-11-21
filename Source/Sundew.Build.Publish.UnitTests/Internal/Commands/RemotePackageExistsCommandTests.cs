// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePackageExistsCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal.Commands
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using NSubstitute;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Build.Publish.Internal.Commands;
    using Xunit;

    public class RemotePackageExistsCommandTests
    {
        [Theory]
        [InlineData("Newtonsoft.Json", true)]
        [InlineData("NonExistingPackage1234", false)]
        public async Task ExistsAsync_Then_ResultShouldBeExpectedResult(string packageId, bool expectedResult)
        {
            var testee = new RemotePackageExistsCommand();

            var result = await testee.ExistsAsync(packageId, new SemanticVersion(12, 0, 3), NuGetConstants.V3FeedUrl, Substitute.For<ILogger>());

            result.Should().Be(expectedResult);
        }
    }
}