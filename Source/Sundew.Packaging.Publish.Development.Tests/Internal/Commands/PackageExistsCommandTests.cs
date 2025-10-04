// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageExistsCommandTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Development.Tests.Internal.Commands;

using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Versioning;
using Sundew.Packaging.Versioning.Commands;
using Xunit;

public class PackageExistsCommandTests
{
    [Theory]
    [InlineData("Newtonsoft.Json", true)]
    [InlineData("NonExistingPackage1234", false)]
    public async Task ExistsAsync_Then_ResultShouldBeExpectedResult(string packageId, bool expectedResult)
    {
        var testee = new PackageExistsCommand(New.Mock<ILogger>());

        var result = await testee.ExistsAsync(packageId, new SemanticVersion(12, 0, 3), new[] { new PackageSource(NuGetConstants.V3FeedUrl) });

        result.Should().Be(expectedResult);
    }
}