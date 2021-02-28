// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreparePublishTaskTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Base.Text;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class PreparePublishTaskTests
    {
        private const string APackageId = "Package.Id";
        private const string LatestVersion = "1.0.5";
        private const string LatestPrereleaseVersion = "1.0.5-{0}-u20140101-101010";
        private const string ExpectedDefaultPushSource = "http://nuget.org";
        private const string FallbackApiKey = "FallbackKey";
        private const string FallbackSymbolsApiKey = "SymbolsFallbackKey";
        private static readonly Dictionary<string, string> UriPrereleasePrefixMap = new()
        {
            { @"c:\temp", "pre" },
            { @"c:\temp\packages", string.Empty },
            { @"c:\dev\packages", "dev" },
            { PreparePublishTask.DefaultLocalSource, "pre" },
            { ExpectedDefaultPushSource, "pre" },
        };

        private readonly PreparePublishTask testee;
        private readonly IPackageExistsCommand packageExistsCommand = New.Mock<IPackageExistsCommand>();
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand = New.Mock<ILatestPackageVersionCommand>();
        private readonly IDateTime dateTime = New.Mock<IDateTime>();
        private readonly ISettings settings = New.Mock<ISettings>();
        private readonly IFileSystem fileSystem = New.Mock<IFileSystem>();
        private readonly ISettingsFactory settingsFactory = New.Mock<ISettingsFactory>();

        public PreparePublishTaskTests()
        {
            this.testee = new PreparePublishTask(
                this.settingsFactory,
                this.fileSystem,
                new PackageVersioner(this.dateTime, this.packageExistsCommand, this.latestPackageVersionCommand),
                New.Mock<ICommandLogger>())
            {
                AllowLocalSource = true,
                PackageId = APackageId,
                SolutionDir = @"Any\LocalSourcePath",
            };

            this.dateTime.SetupGet(x => x.UtcTime).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
            this.fileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            this.latestPackageVersionCommand.Setup(
                    x => x.GetLatestMajorMinorVersion(APackageId, It.IsAny<IReadOnlyList<string>>(), It.IsAny<NuGetVersion>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync<string, IReadOnlyList<string>, NuGetVersion, bool, bool, ILogger, ILatestPackageVersionCommand, NuGetVersion?>(
                    (_, sourceUris, _, _, allowPrerelease, _) => NuGetVersion.Parse(string.Format(allowPrerelease ? LatestPrereleaseVersion : LatestVersion, UriPrereleasePrefixMap[sourceUris.First()])));
            this.settingsFactory.Setup(x => x.LoadDefaultSettings(It.IsAny<string>())).Returns(this.settings);
            this.settingsFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(New.Mock<ISettings>());
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_Then_PackageVersionShouldBeExpectedResult(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(PreparePublishTask.DefaultLocalSource);
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_When_DefaultIsSet_Then_PackageVersionShouldBeExpectedPreVersion(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.SourceName = "default";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedDefaultPushSource);
            this.testee.SymbolsSource.Should().BeNull();
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0.1.1", VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.0")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_SourceNameIsDefaultStable_Then_PackageVersionShouldBeTesteeVersion(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.SourceName = "default-stable";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedDefaultPushSource);
            this.testee.SymbolsSource.Should().BeNull();
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.0")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_SourceNameIsLocalStable_Then_PackageVersionShouldBeTesteeVersion(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.SourceName = "local-stable";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(PreparePublishTask.DefaultLocalSource);
            this.testee.SymbolsSource.Should().BeNull();
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_When_LocalPushSourceIsSet_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.LocalSource = @"c:\temp";
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(this.testee.LocalSource);
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.0")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_ProductionPushSourceIsSetAndPushSourceSelectorMatches_Then_SourceShouldBeExpectedSource(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            const string ExpectedPushSource = @"c:\temp\packages";
            const string ExpectedSymbolsPushSource = @"c:\temp\symbols";
            this.testee.Version = packageVersion;
            this.testee.VersioningMode = versioningMode.ToString();
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);

            this.testee.ProductionSource = $"master=>{ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "master";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Fact]
        public void Execute_When_LocalSourceIsDisabledAndOthersDidNotMatch_Then_IsPublishEnabledShouldBeFalse()
        {
            this.testee.Version = "1.0";
            this.testee.ProductionSource = @"master => c:\temp\packages|c:\temp\symbols";
            this.testee.AllowLocalSource = false;
            this.testee.SourceName = "feature/featureX";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishPackages.Should().BeFalse();
        }

        [Theory]
        [InlineData("1ApiKey@", null, "1SymbolsApiKey@", null, "1ApiKey", "1SymbolsApiKey")]
        [InlineData("@", null, "@", null, null, null)]
        [InlineData("1ApiKey@", null, "@", FallbackSymbolsApiKey, "1ApiKey", null)]
        [InlineData("1ApiKey@", null, "@", null, "1ApiKey", null)]
        [InlineData("@", FallbackApiKey, "1SymbolsApiKey@", null, null, "1SymbolsApiKey")]
        [InlineData("1ApiKey@", null, Strings.Empty, null, "1ApiKey", "1ApiKey")]
        [InlineData(Strings.Empty, FallbackApiKey, "1SymbolsApiKey@", null, FallbackApiKey, "1SymbolsApiKey")]
        public void Execute_When_SourceIsConfiguredWithApiKeys_Then_SourceApiKeyShouldBeExpected(
            string apiKeySetup,
            string fallbackApiKey,
            string symbolsApiKeySetup,
            string fallbackSymbolsApiKey,
            string expectedApiKey,
            string expectedSymbolsApiKey)
        {
            this.testee.Version = "1.0";
            this.testee.ProductionSource = $@"master => {apiKeySetup}{ExpectedDefaultPushSource} | {symbolsApiKeySetup}{ExpectedDefaultPushSource}";
            this.testee.SourceName = "master";
            this.testee.ApiKey = fallbackApiKey;
            this.testee.SymbolsApiKey = fallbackSymbolsApiKey;

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishPackages.Should().BeTrue();
            this.testee.SourceApiKey.Should().Be(expectedApiKey);
            this.testee.SymbolsSourceApiKey.Should().Be(expectedSymbolsApiKey);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-dev-u20160108-173613")]
        public void Execute_When_DeveloperPushSourceIsSetAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            const string ExpectedPushSource = @"c:\dev\packages";
            const string ExpectedSymbolsPushSource = @"c:\dev\symbols";
            this.testee.ProductionSource = "master => https://production.com | https://production.com/symbols";
            this.testee.DevelopmentSource = $@"\w+ => {ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/NewFeature";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-New_Feature_WithNumber123-dev-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-New_Feature_WithNumber123-dev-u20160108-173613")]
        public void Execute_When_DeveloperPushSourceIsSetWithPrefixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            const string ExpectedPushSource = @"c:\dev\packages";
            const string ExpectedSymbolsPushSource = @"c:\dev\symbols";
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.ProductionSource = "master => https://production.com|https://production.com/symbols";
            this.testee.DevelopmentSource = $@"/feature/(?<Prefix>.+) => {ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-dev-u20160108-173613-New_Feature_WithNumber123")]
        public void Execute_When_DeveloperPushSourceIsSetWithPostfixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            const string ExpectedPushSource = @"c:\dev\packages";
            const string ExpectedSymbolsPushSource = @"c:\dev\symbols";
            this.testee.ProductionSource = "master => https://production.com|https://production.com/symbols";
            this.testee.DevelopmentSource = $@"/feature/(?<Postfix>.+) => {ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-TN12-dev-u20160108-173613-New_Feature_WithNumber123")]
        public void Execute_When_DeveloperPushSourceIsSetWithPreAndPostfixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            const string ExpectedPushSource = @"c:\dev\packages";
            const string ExpectedSymbolsPushSource = @"c:\dev\symbols";
            this.testee.ProductionSource = "master => https://production.com|https://production.com/symbols";
            this.testee.DevelopmentSource = $@"/feature/(?<Prefix>TN\d\d)-(?<Postfix>.+) => {ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/TN12-New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedPackageVersion);
        }

        [Fact]
        public void Execute_When_WorkingDirectoryIsUndefinedAndCurrentDirectoryIsARoot_Then_ArgumentExceptionShouldBeThrown()
        {
            this.fileSystem.Setup(x => x.GetCurrentDirectory()).Returns("c:\\");
            this.testee.SolutionDir = "*Undefined*";

            Action act = () => this.testee.Execute();

            act.Should().ThrowExactly<ArgumentException>();
        }

        private void ArrangeDefaultPushSource()
        {
            this.settings.Setup(x => x.GetSection(Source.ConfigText)).Returns(
                (SettingSection)typeof(VirtualSettingSection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .OrderByDescending(x => x.GetParameters().Length).First().Invoke(
                        new object[]
                        {
                            Source.ConfigText,
                            new Dictionary<string, string>(),
                            new List<SettingItem> { new AddItem(SourceSelector.DefaultPushSourceText, ExpectedDefaultPushSource) },
                        }));
        }
    }
}