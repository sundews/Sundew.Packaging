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
    using System.Globalization;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Base.Primitives.Time;
    using Sundew.Base.Text;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Source;
    using Sundew.Packaging.Staging;
    using Sundew.Packaging.Testing;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.Commands;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.NuGet.Configuration;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;
    using ILogger = Sundew.Packaging.Versioning.Logging.ILogger;

    public class PreparePublishTaskTests
    {
        private const string APackageId = "Package.Id";
        private const string LatestVersion = "1.0.5";
        private const string LatestPrereleaseVersion = "1.0.5-{0}-u20140101-101010";
        private const string ExpectedDefaultPushSource = "http://nuget.org";
        private const string FallbackApiKey = "FallbackKey";
        private const string FallbackSymbolsApiKey = "SymbolsFallbackKey";
        private static readonly string BuildDateTimeFilePath = Paths.EnsurePlatformPath(@"c:\a\BuildDateTime.path");

        private static readonly Dictionary<string, string> UriPrereleasePrefixMap = new()
        {
            { Paths.EnsurePlatformPath(@"c:\temp"), "pre" },
            { Paths.EnsurePlatformPath(@"c:\temp\packages"), string.Empty },
            { Paths.EnsurePlatformPath(@"c:\dev\packages"), "dev" },
            { PackageSources.DefaultLocalSource, "pre" },
            { ExpectedDefaultPushSource, "pre" },
        };

        private readonly PreparePublishTask testee;
        private readonly IPackageExistsCommand packageExistsCommand = New.Mock<IPackageExistsCommand>();
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand = New.Mock<ILatestPackageVersionCommand>();
        private readonly IDateTime dateTime = New.Mock<IDateTime>();
        private readonly ISettings defaultSettings = New.Mock<ISettings>();
        private readonly IFileSystem fileSystem = New.Mock<IFileSystem>();
        private readonly INuGetVersionProvider nuGetVersionProvider = New.Mock<INuGetVersionProvider>();
        private readonly ISettingsFactory settingsFactory = New.Mock<ISettingsFactory>();
        private readonly ILogger logger = New.Mock<ILogger>();

        public PreparePublishTaskTests()
        {
            this.testee = new PreparePublishTask(
                this.settingsFactory,
                this.fileSystem,
                new PackageVersioner(this.packageExistsCommand, this.latestPackageVersionCommand, this.logger),
                this.dateTime,
                this.nuGetVersionProvider,
                this.logger)
            {
                BuildInfoFilePath = BuildDateTimeFilePath,
                PublishInfoFilePath = Paths.EnsurePlatformPath(@"c:\a\PublishInfo.path"),
                VersionFilePath = Paths.EnsurePlatformPath(@"c:\a\Version.path"),
                ReferencedPackageVersionFilePath = Paths.EnsurePlatformPath(@"c:\a\ReferencedPackageVersion.path"),
                AllowLocalSource = true,
                PackageId = APackageId,
                SolutionDir = @"Any/LocalSourcePath",
                IncludeSymbols = true,
            };

            this.testee.Version = "1.0";

            this.dateTime.SetupGet(x => x.UtcNow).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
            this.fileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            this.fileSystem.Setup(x => x.FileExists(BuildDateTimeFilePath)).Returns(true);
            this.fileSystem.Setup(x => x.ReadAllText(BuildDateTimeFilePath)).Returns(new DateTime(2016, 01, 08, 17, 36, 13, DateTimeKind.Utc).ToString(PrereleaseDateTimeProvider.UniversalDateTimeFormat, CultureInfo.InvariantCulture));
            this.latestPackageVersionCommand.Setup(
                    x => x.GetLatestMajorMinorVersion(APackageId, It.IsAny<IReadOnlyList<string>>(), It.IsAny<NuGetVersion>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync<string, IReadOnlyList<string>, NuGetVersion, bool, bool, ILatestPackageVersionCommand, NuGetVersion?>(
                    (_, sourceUris, _, _, allowPrerelease) => NuGetVersion.Parse(string.Format(allowPrerelease ? LatestPrereleaseVersion : LatestVersion, UriPrereleasePrefixMap[sourceUris[0]])));
            this.settingsFactory.Setup(x => x.LoadDefaultSettings(It.IsAny<string>())).Returns(this.defaultSettings);
            this.defaultSettings.Setup(x => x.GetConfigFilePaths()).Returns(Array.Empty<string>());
            this.settingsFactory.Setup(x => x.LoadSpecificSettings(It.IsAny<string>(), It.IsAny<string>())).Returns(New.Mock<ISettings>());
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-u20160108-173613-local")]
        public void Execute_Then_PackageVersionShouldBeExpectedResult(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(PackageSources.DefaultLocalSource);
            this.testee.PublishInfo.Version.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-u20160108-173613-local")]
        public void Execute_When_DefaultIsSet_Then_PackageVersionShouldBeExpectedPreVersion(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.Stage = "default";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(ExpectedDefaultPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().BeNull();
            this.testee.PublishInfo.Version.Should().Be(expectedVersion);
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
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.Stage = "default-stable";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(ExpectedDefaultPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().BeNull();
            this.testee.PublishInfo.Version.Should().Be(expectedVersion);
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
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.Stage = "local-stable";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(PackageSources.DefaultLocalSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().BeNull();
            this.testee.PublishInfo.Version.Should().Be(expectedVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-u20160108-173613-local")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-u20160108-173613-local")]
        public void Execute_When_LocalPushSourceIsSet_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            this.testee.Version = packageVersion;
            this.testee.VersioningMode = versioningMode.ToString();
            this.testee.LocalSource = Paths.EnsurePlatformPath(@"c:\temp");
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(this.testee.LocalSource);
            this.testee.PublishInfo!.Version.Should().Be(expectedVersion);
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
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\temp\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\temp\symbols");
            this.testee.Version = packageVersion;
            this.testee.VersioningMode = versioningMode.ToString();
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);

            this.testee.Production = $"master=> {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "master";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedVersion);
        }

        [Fact]
        public void Execute_When_LocalSourceIsDisabledAndOthersDidNotMatch_Then_IsPublishEnabledShouldBeFalse()
        {
            this.testee.Version = "1.0";
            this.testee.Production = @$"master => {Paths.EnsurePlatformPath(@"c:\temp\packages")}|{Paths.EnsurePlatformPath(@"c:\temp\symbols")}";
            this.testee.AllowLocalSource = false;
            this.testee.Stage = "feature/featureX";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.IsEnabled.Should().BeFalse();
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
            this.testee.Production = $@"master => {apiKeySetup}{ExpectedDefaultPushSource} | {symbolsApiKeySetup}{ExpectedDefaultPushSource}";
            this.testee.Stage = "master";
            this.testee.ApiKey = fallbackApiKey;
            this.testee.SymbolsApiKey = fallbackSymbolsApiKey;

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.IsEnabled.Should().BeTrue();
            this.testee.PublishInfo.ApiKey.Should().Be(expectedApiKey);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(ExpectedDefaultPushSource);
            this.testee.PublishInfo.SymbolsApiKey.Should().Be(expectedSymbolsApiKey);
        }

        [Theory]
        [InlineData(true, ExpectedDefaultPushSource, "ASymbolKey")]
        [InlineData(false, ExpectedDefaultPushSource, "ASymbolKey")]
        public void Execute_Then_SymbolsSourceAndApiKeyShouldBeExpectedResult(bool includeSymbols, string expectedSymbolsSource, string expectedSymbolApiKey)
        {
            this.testee.Version = "1.0";
            this.testee.Production = $@"master => AKey@{ExpectedDefaultPushSource} | ASymbolKey@{ExpectedDefaultPushSource}";
            this.testee.Stage = "master";
            this.testee.IncludeSymbols = includeSymbols;

            this.testee.Execute();

            this.testee.PublishInfo!.SymbolsPushSource.Should().Be(expectedSymbolsSource);
            this.testee.PublishInfo!.SymbolsApiKey.Should().Be(expectedSymbolApiKey);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, null, "1.0.6-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, Strings.Empty, "1.0.6-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "Postfix", "1.0.6-u20160108-173613-dev-Postfix")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, null, "1.0.1-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, null, "1.0.0-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, null, "1.0.1-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.NoChange, false, null, "1.0.0-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, null, "1.0.2-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, null, "1.0.1-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, null, "1.0.2-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, null, "1.0.1-u20160108-173613-dev")]
        public void Execute_When_DeveloperPushSourceIsSetAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string prereleasePostfix, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\dev\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\dev\symbols");
            this.testee.Production = "master => https://production.com | https://production.com/symbols";
            this.testee.Development = $@"\w+ => {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "/feature/NewFeature";
            this.testee.PrereleasePostfix = prereleasePostfix;
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-New-Feature-WithNumber123-u20160108-173613-dev")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-New-Feature-WithNumber123-u20160108-173613-dev")]
        public void Execute_When_DeveloperPushSourceIsSetWithPrefixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\dev\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\dev\symbols");
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            this.testee.Production = "master => https://production.com|https://production.com/symbols";
            this.testee.Development = $@"/feature/(?<Prefix>.+) => {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "/feature/New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-u20160108-173613-dev-New-Feature-WithNumber123")]
        public void Execute_When_DeveloperPushSourceIsSetWithPostfixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\dev\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\dev\symbols");
            this.testee.Production = "master => https://production.com|https://production.com/symbols";
            this.testee.Development = $@"/feature/(?<Postfix>.+) => {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "/feature/New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-feature-TN12-u20160108-173613-dev-New-Feature-WithNumber123")]
        public void Execute_When_DeveloperPushSourceIsSetWithPreAndPostfixAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\dev\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\dev\symbols");
            this.testee.Production = "master => https://production.com|https://production.com/symbols";
            this.testee.Development = $@"/(?<Prefix>feature/TN\d\d)-(?<Postfix>.+) => {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "/feature/TN12-New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedPackageVersion);
        }

        [Theory]
        [InlineData("1.0", VersioningMode.AutomaticLatestPatch, false, "1.0.6-u20160108-173613-00000-feat")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1-u20160108-173613-00000-feat")]
        [InlineData("1.0", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.0-u20160108-173613-00000-feat")]
        [InlineData("1.0", VersioningMode.AlwaysIncrementPatch, false, "1.0.1-u20160108-173613-00000-feat")]
        [InlineData("1.0", VersioningMode.NoChange, false, "1.0.0-u20160108-173613-00000-feat")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-u20160108-173613-00000-feat")]
        [InlineData("1.0.1", VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-u20160108-173613-00000-feat")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, false, "1.0.2-u20160108-173613-00000-feat")]
        [InlineData("1.0.1", VersioningMode.NoChange, false, "1.0.1-u20160108-173613-00000-feat")]
        public void Execute_When_DeveloperPushSourceIsSetWithStagingNameAndPrereleaseFormatAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(string packageVersion, VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.testee.Version = packageVersion;
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>()))
                .ReturnsAsync(stableReleaseExists);
            string expectedPushSource = Paths.EnsurePlatformPath(@"c:\dev\packages");
            string expectedSymbolsPushSource = Paths.EnsurePlatformPath(@"c:\dev\symbols");
            this.testee.Production = "master => https://production.com|https://production.com/symbols";
            this.testee.Development = $@"/feature/TN\d\d-.+=> #feat&u{{2:yyyyMMdd-HHmmss-fffff}}-{{0}} {expectedPushSource}|{expectedSymbolsPushSource}";
            this.testee.Stage = "/feature/TN12-New_Feature_WithNumber123";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishInfo!.PushSource.Should().Be(expectedPushSource);
            this.testee.PublishInfo.SymbolsPushSource.Should().Be(expectedSymbolsPushSource);
            this.testee.PublishInfo.Version.Should().Be(expectedPackageVersion);
        }

        [Fact]
        public void Execute_When_WorkingDirectoryIsUndefinedAndCurrentDirectoryIsARoot_Then_ArgumentExceptionShouldBeThrown()
        {
            this.fileSystem.Setup(x => x.GetCurrentDirectory()).Returns(Paths.EnsurePlatformPath("c:\\"));
            this.testee.SolutionDir = "*Undefined*";

            var result = this.testee.Execute();

            result.Should().BeFalse();
            this.logger.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Execute_Then_SaveShouldBeCalled()
        {
            this.testee.Execute();

            this.nuGetVersionProvider.Verify(x => x.Save(this.testee.VersionFilePath!, this.testee.ReferencedPackageVersionFilePath!, It.IsAny<string>()), Times.Once);
        }

        private void ArrangeDefaultPushSource()
        {
            this.defaultSettings.Setup(x => x.GetSection(Stage.ConfigText)).Returns(
                (SettingSection)typeof(VirtualSettingSection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .OrderByDescending(x => x.GetParameters().Length).First().Invoke(
                        new object[]
                        {
                            Stage.ConfigText,
                            new Dictionary<string, string>(),
                            new List<SettingItem> { new AddItem(StageSelector.DefaultPushSourceText, ExpectedDefaultPushSource) },
                        }));
        }
    }
}