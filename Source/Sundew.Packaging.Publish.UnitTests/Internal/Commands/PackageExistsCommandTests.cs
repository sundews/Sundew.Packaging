// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageExistsCommandTests.cs" company="Hukano">
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
    using Sundew.Packaging.Publish.Internal.Commands;
    using Xunit;

    public class PackageExistsCommandTests
    {
        [Theory]
        [InlineData("Newtonsoft.Json", true)]
        [InlineData("NonExistingPackage1234", false)]
        public async Task ExistsAsync_Then_ResultShouldBeExpectedResult(string packageId, bool expectedResult)
        {
            var testee = new PackageExistsCommand();

            var result = await testee.ExistsAsync(packageId, new SemanticVersion(12, 0, 3), NuGetConstants.V3FeedUrl, New.Mock<ILogger>());

            result.Should().Be(expectedResult);
        }
    }
}