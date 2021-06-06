// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateVerbTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests
{
    using FluentAssertions;
    using NUnit.Framework;
    using Sundew.Base.Primitives.Computation;
    using Sundew.CommandLine;
    using Sundew.Packaging.Tool.Update;
    using Sundew.Packaging.Tool.Update.MsBuild.NuGet;

    [TestFixture]
    public class UpdateVerbTests
    {
        [TestCase(@"u -id Sundew.Base", "Sundew.Base", null)]
        [TestCase(@"u -id Sundew.Base.6.0.0", "Sundew.Base", "6.0.0")]
        [TestCase(@"u -id ""Sundew.Base 6.0.0""", "Sundew.Base", "6.0.0")]
        [TestCase(@"u -id ""Sundew.Base 6.0.0-pre""", "Sundew.Base", "6.0.0-pre")]
        [TestCase(@"u -id WithNumber.6.6.0.0-pre", "WithNumber", "6.6.0.0-pre")]
        [TestCase(@"u -id ""WithIllegalNumber.6 6.0.0-pre""", "WithIllegalNumber.6", "6.0.0-pre")]
        [TestCase(@"u -id ""WithIllegalNumber.6d 6.0.0-pre""", "WithIllegalNumber.6d", "6.0.0-pre")]
        [TestCase(@"u -id WithIllegal.6.Number.6.0.0-pre", "WithIllegal.6.Number", "6.0.0-pre")]
        [TestCase(@"u -id WithIllegal.6d.Number.6.0.0-pre", "WithIllegal.6d.Number", "6.0.0-pre")]
        [TestCase(@"u -id WithIllegal.6.Number.16.0.0-pre", "WithIllegal.6.Number", "16.0.0-pre")]
        [TestCase(@"u -id WithIllegal.6.Number.6.10.0-pre", "WithIllegal.6.Number", "6.10.0-pre")]
        [TestCase(@"u -id WithIllegal.6.Number.6.0.10-pre", "WithIllegal.6.Number", "6.0.10-pre")]
        [TestCase(@"u -id Sundew.Base.6.0", "Sundew.Base", "6.0")]
        [TestCase(@"u -id ""Sundew.Base 6.0""", "Sundew.Base", "6.0")]
        [TestCase(@"u -id ""Sundew.Base 6.0-pre""", "Sundew.Base", "6.0-pre")]
        [TestCase(@"u -id ""Sundew.Base 6.0.*-pre""", "Sundew.Base", "6.0.*-pre")]
        public void Parse_When_PackageIdIsSpecifiedWithVersion_Then_VersionShouldBeParsedSuccessfully(string input, string expectedId, string? expectedVersion)
        {
            var commandLineParser = new CommandLineParser<int, int>();
            var arguments = commandLineParser.AddVerb(new UpdateVerb(), updateVerb => Result.Success(0));

            commandLineParser.Parse(input);

            arguments.PackageIds.Should().Equal(new[] { new PackageId(expectedId, expectedVersion) });
        }

        [TestCase(@"u -id Sundew.Base", "Sundew.Base", null)]
        [TestCase(@"u -id ""Sundew.Base"" --version  6.0.0", "Sundew.Base", "6.0.0")]
        [TestCase(@"u -id ""Sundew.Base"" --version  6.0.0-pre", "Sundew.Base", "6.0.0-pre")]
        [TestCase(@"u -id ""Sundew.Base"" --version 6.0", "Sundew.Base", "6.0")]
        [TestCase(@"u -id ""Sundew.Base"" --version 6.0-pre", "Sundew.Base", "6.0-pre")]
        [TestCase(@"u -id ""Sundew.Base"" --version 6.0.*-pre", "Sundew.Base", "6.0.*-pre")]
        public void Parse_When_PackageIdAndVersionAreSpecifiedSeparately_Then_VersionShouldBeParsedSuccessfully(string input, string expectedId, string? expectedVersion)
        {
            var commandLineParser = new CommandLineParser<int, int>();
            var arguments = commandLineParser.AddVerb(new UpdateVerb(), updateVerb => Result.Success(0));

            commandLineParser.Parse(input);

            arguments.PackageIds.Should().Equal(new[] { new PackageId(expectedId, null) });
            arguments.VersionPattern.Should().Be(expectedVersion);
        }
    }
}