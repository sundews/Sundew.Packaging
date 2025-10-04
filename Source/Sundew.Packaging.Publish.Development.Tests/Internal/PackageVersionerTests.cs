// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionerTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Development.Tests.Internal;

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Moq;
using NuGet.Configuration;
using NuGet.Versioning;
using Sundew.Base.Time;
using Sundew.Packaging.Staging;
using Sundew.Packaging.Versioning;
using Sundew.Packaging.Versioning.Commands;
using Sundew.Packaging.Versioning.Logging;
using Xunit;

public class PackageVersionerTests
{
    private const string AnyPackageId = "Package.Id";
    private const string AnyPushSource = @"Ignored => c:\temp\ignored";
    private static readonly DateTime BuildDateTime = new(2016, 01, 08, 17, 36, 13);
    private readonly IDateTime dateTime = New.Mock<IDateTime>();
    private readonly PackageVersioner testee;
    private readonly ILatestPackageVersionCommand latestPackageVersionCommand;

    public PackageVersionerTests()
    {
        this.latestPackageVersionCommand = New.Mock<ILatestPackageVersionCommand>();
        this.testee = new PackageVersioner(New.Mock<IPackageExistsCommand>(), this.latestPackageVersionCommand, New.Mock<ILogger>());
        this.dateTime.SetupGet(x => x.UtcNow).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
    }

    [Theory]
    [InlineData("1.2.3")]
    [InlineData("1.2.3-pre")]
    public void GetVersion_When_ForceVersionIsSet_Then_ResultShouldBeExpectedVersion(string expectedVersion)
    {
        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse("4.5.6"),
            null,
            expectedVersion,
            VersioningMode.AlwaysIncrementPatch,
            new SelectedStage(Stage.Parse(AnyPushSource, "beta", "beta", false, null, null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            null,
            null,
            string.Empty);

        result.ToNormalizedString().Should().Be(expectedVersion);
    }

    [Theory]
    [InlineData("1.0.1", VersioningMode.NoChange, "dev", "1.0.1-u20160108-173613-dev")]
    [InlineData("2.0.0", VersioningMode.NoChange, "pre", "2.0.0-u20160108-173613-pre")]
    [InlineData("3.0.2", VersioningMode.NoChange, "ci", "3.0.2-u20160108-173613-ci")]
    [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, "dev", "1.0.2-u20160108-173613-dev")]
    [InlineData("2.0.0", VersioningMode.AlwaysIncrementPatch, "pre", "2.0.1-u20160108-173613-pre")]
    [InlineData("3.0.2", VersioningMode.AlwaysIncrementPatch, "ci", "3.0.3-u20160108-173613-ci")]
    [InlineData("3.0", VersioningMode.AlwaysIncrementPatch, "ci", "3.0.1-u20160108-173613-ci")]
    public void GetVersion_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, VersioningMode versioningMode, string stage, string expectedResult)
    {
        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse(versionNumber),
            null,
            null,
            versioningMode,
            new SelectedStage(Stage.Parse(AnyPushSource, stage, stage, false, null, null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            null,
            null,
            string.Empty);

        result.ToNormalizedString().Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, "ci", null, "3.0.0-u20160108-173613-ci")]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, "ci", "3.0.2", "3.0.3-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", null, "3.0.1-u20160108-173613-ci")]
    [InlineData("3.0", VersioningMode.AutomaticLatestRevision, "ci", "3.0.2", "3.0.2.1-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", "3.0.1.10", "3.0.1.11-u20160108-173613-ci")]
    public void GetVersion_When_UsingAutomaticVersioning_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, VersioningMode versioningMode, string stage, string? latestVersion, string expectedResult)
    {
        this.latestPackageVersionCommand.Setup(x => x.GetLatestMajorMinorVersion(
                AnyPackageId,
                It.IsAny<IReadOnlyList<PackageSource>>(),
                It.IsAny<NuGetVersion>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(latestVersion == null ? null : NuGetVersion.Parse(latestVersion));

        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse(versionNumber),
            null,
            null,
            versioningMode,
            new SelectedStage(Stage.Parse(AnyPushSource, stage, stage, false, null, null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            null,
            null,
            string.Empty);

        result.ToNormalizedString().Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, null, "3.0.0")]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, "3.0.2", "3.0.3")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, null, "3.0.1")]
    [InlineData("3.0", VersioningMode.AutomaticLatestRevision, "3.0.3", "3.0.3.1")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "3.0.1.10", "3.0.1.11")]
    public void GetVersion_When_UsingAutomaticVersioningAndIsStable_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, VersioningMode versioningMode, string? latestVersion, string expectedResult)
    {
        this.latestPackageVersionCommand.Setup(x => x.GetLatestMajorMinorVersion(
                AnyPackageId,
                It.IsAny<IReadOnlyList<PackageSource>>(),
                It.IsAny<NuGetVersion>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(latestVersion == null ? null : NuGetVersion.Parse(latestVersion));

        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse(versionNumber),
            null,
            null,
            versioningMode,
            new SelectedStage(Stage.Parse(AnyPushSource, "ci", "ci", true, null, null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            null,
            null,
            string.Empty);

        result.ToNormalizedString().Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, "ci", null, "3.0.0-u20160108-173613-ci")]
    [InlineData("3.0", VersioningMode.AutomaticLatestPatch, "ci", "3.0.2", "3.0.3-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", null, "3.0.1-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", "3.0.1.0", "3.0.1.1-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", "3.0.1", "3.0.1.1-u20160108-173613-ci")]
    [InlineData("3.0", VersioningMode.AutomaticLatestRevision, "ci", "3.0.2", "3.0.2.1-u20160108-173613-ci")]
    [InlineData("3.0.1", VersioningMode.AutomaticLatestRevision, "ci", "3.0.1.10", "3.0.1.11-u20160108-173613-ci")]
    public void GetVersion_When_UsingFallbackPrereleaseFormat_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, VersioningMode versioningMode, string stage, string? latestVersion, string expectedResult)
    {
        this.latestPackageVersionCommand.Setup(x => x.GetLatestMajorMinorVersion(
                AnyPackageId,
                It.IsAny<IReadOnlyList<PackageSource>>(),
                It.IsAny<NuGetVersion>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(latestVersion == null ? null : NuGetVersion.Parse(latestVersion));

        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse(versionNumber),
            null,
            null,
            versioningMode,
            new SelectedStage(Stage.Parse(AnyPushSource, stage, stage, false, "u{1}-{3}-{4}-{5}-{0}", null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            null,
            null,
            string.Empty);

        result.ToNormalizedString().Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{1}-{3}-{4}-{5}-{0}", "metadata", "3.0.1.11-u20160108-173613-metadata-ci+20160108-173613-metadata-ci")]
    [InlineData("{1}-{3}-{4}-{5}-{0}", "", "3.0.1.11-u20160108-173613-ci+20160108-173613-ci")]
    [InlineData("{1}-{3}-{4}-{5}-{0}", null, "3.0.1.11-u20160108-173613-ci+20160108-173613-ci")]
    public void GetVersion_When_UsingFallbackPrereleaseFormat1_Then_ResultToFullStringShouldBeExpectedResult(string metadataFormat, string? metadata, string expectedResult)
    {
        this.latestPackageVersionCommand.Setup(x => x.GetLatestMajorMinorVersion(
                AnyPackageId,
                It.IsAny<IReadOnlyList<PackageSource>>(),
                It.IsAny<NuGetVersion>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(NuGetVersion.Parse("3.0.1.10"));
        var result = this.testee.GetVersion(
            AnyPackageId,
            NuGetVersion.Parse("3.0.1"),
            null,
            null,
            VersioningMode.AutomaticLatestRevision,
            new SelectedStage(Stage.Parse(AnyPushSource, "ci", "ci", false, "u{1}-{3}-{4}-{5}-{0}", null, null, null, true, BuildPromotion.None)!),
            new[] { new PackageSource(AnyPushSource) },
            BuildDateTime,
            metadata,
            metadataFormat,
            string.Empty);

        result.ToFullString().Should().Be(expectedResult);
    }
}