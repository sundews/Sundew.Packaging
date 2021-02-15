// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceSelectorTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal
{
    using FluentAssertions;
    using Moq;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Base.Text;
    using Sundew.Packaging.Publish.Internal;
    using Xunit;

    public class SourceSelectorTests
    {
        private const string ExpectedUri = @"https://uri.com";
        private const string ExpectedSymbolUri = @"https://uri.com/symbols";

        [Theory]
        [InlineData(@"refs/heads/release/(?<Postfix>.+)=>int|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, Strings.Empty, "int", "branch")]
        [InlineData(@"refs/heads/release/(?<Prefix>.+)=>int|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, "branch", "int", Strings.Empty)]
        [InlineData(@"refs/heads/release/.+=>int|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+=>int|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+=>|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, Strings.Empty, Strings.Empty, Strings.Empty)]
        [InlineData(@".+|https://uri.com|https://uri.com/symbols", ExpectedSymbolUri, Strings.Empty, SourceSelector.DefaultIntegrationPackagePrefix, Strings.Empty)]
        [InlineData(@".+|https://uri.com|", null, Strings.Empty, SourceSelector.DefaultIntegrationPackagePrefix, Strings.Empty)]
        [InlineData(@".+|https://uri.com", null, Strings.Empty, SourceSelector.DefaultIntegrationPackagePrefix, Strings.Empty)]
        public void SelectSource_When_MatchingIntegrationSource_Then_ResultShouldAsExpected(
            string source,
            string expectedSymbolUri,
            string expectedPackagePrefix,
            string expectedStage,
            string expectedPackagePostfix)
        {
            var result = SourceSelector.SelectSource(
                "refs/heads/release/branch",
                string.Empty,
                source,
                string.Empty,
                string.Empty,
                New.Mock<ISettings>(),
                false);

            result.Uri.Should().Be(ExpectedUri);
            result.SymbolsUri.Should().Be(expectedSymbolUri);
            result.PackagePrefix.Should().Be(expectedPackagePrefix);
            result.Stage.Should().Be(expectedStage);
            result.PackagePostfix.Should().Be(expectedPackagePostfix);
        }
    }
}