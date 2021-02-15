// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersionerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal
{
    using System;
    using FluentAssertions;
    using Moq;
    using NuGet.Common;
    using NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Xunit;

    public class PackageVersionerTests
    {
        private const string AnyPackageId = "Package.Id";
        private const string AnyPushSource = @"Ignored|c:\temp\ignored";
        private readonly IDateTime dateTime = New.Mock<IDateTime>();
        private readonly PackageVersioner testee;

        public PackageVersionerTests()
        {
            this.testee = new PackageVersioner(this.dateTime, New.Mock<IPackageExistsCommand>(), New.Mock<ILatestPackageVersionCommand>());
            this.dateTime.SetupGet(x => x.UtcTime).Returns(new DateTime(2016, 01, 08, 17, 36, 13));
        }

        [Theory]
        [InlineData("1.0.1", VersioningMode.NoChange, "dev", "1.0.1-dev-u20160108-173613")]
        [InlineData("2.0.0", VersioningMode.NoChange, "pre", "2.0.0-pre-u20160108-173613")]
        [InlineData("3.0.2", VersioningMode.NoChange, "ci", "3.0.2-ci-u20160108-173613")]
        [InlineData("1.0.1", VersioningMode.AlwaysIncrementPatch, "dev", "1.0.2-dev-u20160108-173613")]
        [InlineData("2.0.0", VersioningMode.AlwaysIncrementPatch, "pre", "2.0.1-pre-u20160108-173613")]
        [InlineData("3.0.2", VersioningMode.AlwaysIncrementPatch, "ci", "3.0.3-ci-u20160108-173613")]
        [InlineData("3.0", VersioningMode.AlwaysIncrementPatch, "ci", "3.0.1-ci-u20160108-173613")]
        public void GetVersion_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, VersioningMode versioningMode, string stage, string expectedResult)
        {
            var result = this.testee.GetVersion(
                AnyPackageId,
                NuGetVersion.Parse(versionNumber),
                versioningMode,
                false,
                Source.Parse(AnyPushSource, stage, false),
                new[] { AnyPushSource },
                New.Mock<ILogger>());

            result.ToFullString().Should().Be(expectedResult);
        }
    }
}