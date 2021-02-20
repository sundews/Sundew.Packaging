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
    using Sundew.Base.Text;
    using Sundew.Packaging.Publish.Internal;
    using Xunit;

    public class SourceSelectorTests
    {
        private const string ExpectedUri = @"https://uri.com";
        private const string ExpectedSymbolUri = @"https://uri.com/symbols";

        [Theory]
        [InlineData(@"refs/heads/release/(?<Postfix>.+) # int => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", "branch")]
        [InlineData(@"refs/heads/release/(?<Prefix>.+) # int => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, "branch", "int", Strings.Empty)]
        [InlineData(@"refs/heads/release/.+ #int => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+ #int => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+ # => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, Strings.Empty, Strings.Empty)]
        [InlineData(@".+ => https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+ => https://uri.com|", null, null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+ => https://uri.com", null, null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@"refs/heads/release/(?<Postfix>.+) #int => 1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, "int", "branch")]
        [InlineData(@"refs/heads/release/(?<Prefix>.+) #int => 1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, "branch", "int", Strings.Empty)]
        [InlineData(@"refs/heads/release/.+ #int => 1A-K@https://uri.com|2A-K@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, "2A-K", Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+ # int => 1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+ # => 1A-K@https://uri.com|2A-K@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, "2A-K", Strings.Empty, Strings.Empty, Strings.Empty)]
        [InlineData(@".+ => 1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+ => 1A-K@https://uri.com|@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, Strings.Empty, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+ => 1A-K@https://uri.com|", "1A-K", null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+ => 1A-K@https://uri.com", "1A-K", null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        public void SelectSource_When_MatchingIntegrationSource_Then_ResultShouldBeAsExpected(
            string source,
            string expectedSourceApiKey,
            string expectedSymbolUri,
            string expectedSymbolSourceApiKey,
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
            result.ApiKey.Should().Be(expectedSourceApiKey);
            result.SymbolsApiKey.Should().Be(expectedSymbolSourceApiKey);
        }

        [Theory]
        [InlineData(@"refs/heads/release/(?<Postfix>.+)#int=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", "branch")]
        [InlineData(@"refs/heads/release/(?<Prefix>.+)#int=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, "branch", "int", Strings.Empty)]
        [InlineData(@"refs/heads/release/.+#int=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+#int=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+#=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, Strings.Empty, Strings.Empty)]
        [InlineData(@".+=>https://uri.com|https://uri.com/symbols", null, ExpectedSymbolUri, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+=>https://uri.com|", null, null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+=>https://uri.com", null, null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@"refs/heads/release/(?<Postfix>.+)#int=>1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, "int", "branch")]
        [InlineData(@"refs/heads/release/(?<Prefix>.+)#int=>1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, "branch", "int", Strings.Empty)]
        [InlineData(@"refs/heads/release/.+#int=>1A-K@https://uri.com|2A-K@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, "2A-K", Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+#int=>1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, "int", Strings.Empty)]
        [InlineData(@".+#=>1A-K@https://uri.com|2A-K@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, "2A-K", Strings.Empty, Strings.Empty, Strings.Empty)]
        [InlineData(@".+=>1A-K@https://uri.com|https://uri.com/symbols", "1A-K", ExpectedSymbolUri, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+=>1A-K@https://uri.com|@https://uri.com/symbols", "1A-K", ExpectedSymbolUri, Strings.Empty, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+=>1A-K@https://uri.com|", "1A-K", null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        [InlineData(@".+=>1A-K@https://uri.com", "1A-K", null, null, Strings.Empty, SourceSelector.DefaultIntegrationPackageStage, Strings.Empty)]
        public void SelectSource_When_MatchingIntegrationSourceAndSourceDoesNotContainSpace_Then_ResultShouldBeAsExpected(
            string source,
            string expectedSourceApiKey,
            string expectedSymbolUri,
            string expectedSymbolSourceApiKey,
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
            result.ApiKey.Should().Be(expectedSourceApiKey);
            result.SymbolsApiKey.Should().Be(expectedSymbolSourceApiKey);
        }
    }
}