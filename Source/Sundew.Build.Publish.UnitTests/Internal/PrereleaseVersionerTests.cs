// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseVersionerTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal
{
    using System;
    using FluentAssertions;
    using NSubstitute;
    using NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Build.Publish.Internal;
    using Xunit;

    public class PrereleaseVersionerTests
    {
        private const string AnyPushSource = @"Ignored|c:\temp\ignored";
        private readonly IDateTime dateTime = Substitute.For<IDateTime>();
        private readonly PrereleaseVersioner testee;

        public PrereleaseVersionerTests()
        {
            this.testee = new PrereleaseVersioner(this.dateTime);
            this.dateTime.UtcTime.Returns(new DateTime(2016, 01, 08, 17, 36, 13));
        }

        [Theory]
        [InlineData("1.0.1", PrereleaseVersioningMode.NoChange, "dev-u", "1.0.1-dev-u20160108-173613")]
        [InlineData("2.0.0", PrereleaseVersioningMode.NoChange, "pre-u", "2.0.0-pre-u20160108-173613")]
        [InlineData("3.0.2", PrereleaseVersioningMode.NoChange, "int-u", "3.0.2-int-u20160108-173613")]
        [InlineData("1.0.1", PrereleaseVersioningMode.IncrementPatch, "dev-u", "1.0.2-dev-u20160108-173613")]
        [InlineData("2.0.0", PrereleaseVersioningMode.IncrementPatch, "pre-u", "2.0.1-pre-u20160108-173613")]
        [InlineData("3.0.2", PrereleaseVersioningMode.IncrementPatch, "int-u", "3.0.3-int-u20160108-173613")]
        public void GetPrereleaseVersion_Then_ResultToFullStringShouldBeExpectedResult(string versionNumber, PrereleaseVersioningMode prereleaseVersioningMode, string prefix, string expectedResult)
        {
            var result = this.testee.GetPrereleaseVersion(SemanticVersion.Parse(versionNumber), prereleaseVersioningMode, Source.Parse(AnyPushSource, prefix, false));

            result.ToFullString().Should().Be(expectedResult);
        }
    }
}