// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Moq;
    using NuGet.Common;
    using NuGet.Versioning;
    using Sundew.Base.Primitives.Time;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Xunit;

    public class PackageVersionerTests
    {
        private const string AnyPackageId = "Package.Id";
        private const string AnyWorkingDirectory = @"c:\Working\Dir";
        private const string AnyPushSource = @"Ignored => c:\temp\ignored";
        private static readonly DateTime BuildDateTime = new DateTime(2016, 01, 08, 17, 36, 13);
        private readonly IDateTime dateTime = New.Mock<IDateTime>();
        private readonly PackageVersioner testee;
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand;

        public PackageVersionerTests()
        {
            this.latestPackageVersionCommand = New.Mock<ILatestPackageVersionCommand>();
            this.testee = new PackageVersioner(New.Mock<IPackageExistsCommand>(), this.latestPackageVersionCommand);
            this.dateTime.SetupGet(x => x.UtcNow).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
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
                versioningMode,
                new SelectedSource(Source.Parse(AnyPushSource, stage, false, null, null, null, null, true)!),
                new[] { AnyPushSource },
                BuildDateTime,
                string.Empty,
                New.Mock<ILogger>());

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
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<NuGetVersion>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<ILogger>()))
                .ReturnsAsync(latestVersion == null ? null : NuGetVersion.Parse(latestVersion));

            var result = this.testee.GetVersion(
                AnyPackageId,
                NuGetVersion.Parse(versionNumber),
                versioningMode,
                new SelectedSource(Source.Parse(AnyPushSource, stage, false, null, null, null, null, true)!),
                new[] { AnyPushSource },
                BuildDateTime,
                string.Empty,
                New.Mock<ILogger>());

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
                    It.IsAny<IReadOnlyList<string>>(),
                    It.IsAny<NuGetVersion>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<ILogger>()))
                .ReturnsAsync(latestVersion == null ? null : NuGetVersion.Parse(latestVersion));

            var result = this.testee.GetVersion(
                AnyPackageId,
                NuGetVersion.Parse(versionNumber),
                versioningMode,
                new SelectedSource(Source.Parse(AnyPushSource, "ci", true, null, null, null, null, true)!),
                new[] { AnyPushSource },
                BuildDateTime,
                string.Empty,
                New.Mock<ILogger>());

            result.ToNormalizedString().Should().Be(expectedResult);
        }
    }
}