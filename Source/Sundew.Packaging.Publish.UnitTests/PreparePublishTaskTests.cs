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
    using System.Windows.Input;
    using FluentAssertions;
    using Moq;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class PreparePublishTaskTests
    {
        private const string APackageId = "Package.Id";
        private const string LatestVersion = "1.0.5";
        private const string LatestPrereleaseVersion = "1.0.5-{0}-u20140101-101010";
        private const string ExpectedDefaultPushSource = "http://nuget.org";
        private static readonly Dictionary<string, string> UriPrereleasePrefixMap = new()
        {
            { @"c:\temp", "pre" },
            { @"c:\temp\packages", string.Empty },
            { @"c:\dev\packages", "dev" },
            { PreparePublishTask.DefaultLocalSource, "pre" },
            { ExpectedDefaultPushSource, "pre" },
        };

        private readonly PreparePublishTask testee;
        private readonly IAddLocalSourceCommand addLocalSourceCommand = New.Mock<IAddLocalSourceCommand>();
        private readonly IPackageExistsCommand packageExistsCommand = New.Mock<IPackageExistsCommand>();
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand = New.Mock<ILatestPackageVersionCommand>();
        private readonly IDateTime dateTime = New.Mock<IDateTime>();
        private readonly ISettings settings = New.Mock<ISettings>();
        private readonly NuGetVersion nuGetVersion = new(1, 0, 1);

        public PreparePublishTaskTests()
        {
            this.testee = new PreparePublishTask(this.addLocalSourceCommand, new PackageVersioner(this.dateTime, this.packageExistsCommand, this.latestPackageVersionCommand), New.Mock<ICommandLogger>())
            {
                Version = "1.0",
                AllowLocalSource = true,
                PackageId = APackageId,
            };

            this.dateTime.SetupGet(x => x.UtcTime).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
            this.latestPackageVersionCommand.Setup(
                    x => x.GetLatestVersion(APackageId, It.IsAny<string>(), It.IsAny<NuGetVersion>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
                .ReturnsAsync<string, string, NuGetVersion, bool, ILogger, ILatestPackageVersionCommand, NuGetVersion?>(
                    (_, sourceUri, _, allowPrerelease, _) => NuGetVersion.Parse(string.Format(allowPrerelease ? LatestPrereleaseVersion : LatestVersion, UriPrereleasePrefixMap[sourceUri])));
            this.addLocalSourceCommand.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((_, _, localSource) => new LocalSource(localSource, this.settings));
        }

        [Theory]
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_Then_PackageVersionShouldBeExpectedResult(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
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
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_When_DefaultIsSet_Then_PackageVersionShouldBeExpectedPreVersion(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
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
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_SourceNameIsDefaultStable_Then_PackageVersionShouldBeTesteeVersion(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
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
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_SourceNameIsLocalStable_Then_PackageVersionShouldBeTesteeVersion(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
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
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-pre-u20160108-173613")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2-pre-u20160108-173613")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1-pre-u20160108-173613")]
        public void Execute_When_LocalPushSourceIsSet_Then_PushSourceShouldBeEqual(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
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
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.1")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1")]
        public void Execute_When_ProductionPushSourceIsSetAndPushSourceSelectorMatches_Then_SourceShouldBeExpectedSource(VersioningMode versioningMode, bool stableReleaseExists, string expectedVersion)
        {
            const string ExpectedPushSource = @"c:\temp\packages";
            const string ExpectedSymbolsPushSource = @"c:\temp\symbols";
            this.testee.VersioningMode = versioningMode.ToString();
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);

            this.testee.ProductionSource = $"master|{ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "master";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedVersion);
        }

        [Fact]
        public void Execute_When_DevelopmentSourceIsDisableAndOtherDidNotMatch_Then_IsPublishEnabledShouldBeFalse()
        {
            this.testee.ProductionSource = @"master|c:\temp\packages|c:\temp\symbols";
            this.testee.AllowLocalSource = false;
            this.testee.SourceName = "feature/featureX";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.PublishPackages.Should().BeFalse();
        }

        [Theory]
        [InlineData(VersioningMode.AutomaticLatestPatch, false, "1.0.6-dev-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, true, "1.0.2-dev-u20160108-173613")]
        [InlineData(VersioningMode.IncrementPatchIfStableExistForPrerelease, false, "1.0.1-dev-u20160108-173613")]
        [InlineData(VersioningMode.AlwaysIncrementPatch, false, "1.0.2-dev-u20160108-173613")]
        [InlineData(VersioningMode.NoChange, false, "1.0.1-dev-u20160108-173613")]
        public void Execute_When_DeveloperPushSourceIsSetAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual(VersioningMode versioningMode, bool stableReleaseExists, string expectedPackageVersion)
        {
            this.packageExistsCommand.Setup(
                    x => x.ExistsAsync(APackageId, It.IsAny<SemanticVersion>(), It.IsAny<string>(), It.IsAny<ILogger>()))
                .ReturnsAsync(stableReleaseExists);
            const string ExpectedPushSource = @"c:\dev\packages";
            const string ExpectedSymbolsPushSource = @"c:\dev\symbols";
            this.testee.ProductionSource = "master|https://production.com|https://production.com/symbols";
            this.testee.DevelopmentSource = $@"\w+|{ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/NewFeature";
            this.testee.VersioningMode = versioningMode.ToString();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(expectedPackageVersion);
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
                            new List<SettingItem> { new AddItem(Source.DefaultPushSourceText, ExpectedDefaultPushSource) },
                        }));
        }
    }
}