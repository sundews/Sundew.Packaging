// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwaitPublishVerbTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests
{
    using FluentAssertions;
    using global::NuGet.Versioning;
    using NUnit.Framework;
    using Sundew.Base.Primitives.Computation;
    using Sundew.CommandLine;
    using Sundew.Packaging.Tool.AwaitPublish;

    [TestFixture]
    public class AwaitPublishVerbTests
    {
        [TestCase(@"a Sundew.Base", "Sundew.Base", "0.0.0")]
        [TestCase(@"a Sundew.Base.6.0.0", "Sundew.Base", "6.0.0")]
        [TestCase(@"a ""Sundew.Base 6.0.0""", "Sundew.Base", "6.0.0")]
        [TestCase(@"a ""Sundew.Base 6.0.0-pre""", "Sundew.Base", "6.0.0-pre")]
        [TestCase(@"a WithNumber.6.6.0.0-pre", "WithNumber", "6.6.0.0-pre")]
        [TestCase(@"a ""WithIllegalNumber.6 6.0.0-pre""", "WithIllegalNumber.6", "6.0.0-pre")]
        [TestCase(@"a WithIllegal.6.Number.6.0.0-pre", "WithIllegal.6.Number", "6.0.0-pre")]
        [TestCase(@"a WithIllegal.6.Number.16.0.0-pre", "WithIllegal.6.Number", "16.0.0-pre")]
        [TestCase(@"a WithIllegal.6.Number.6.10.0-pre", "WithIllegal.6.Number", "6.10.0-pre")]
        [TestCase(@"a WithIllegal.6.Number.6.0.10-pre", "WithIllegal.6.Number", "6.0.10-pre")]
        [TestCase(@"a Sundew.Base.6.0", "Sundew.Base", "6.0.0")]
        [TestCase(@"a ""Sundew.Base 6.0""", "Sundew.Base", "6.0.0")]
        [TestCase(@"a ""Sundew.Base 6.0-pre""", "Sundew.Base", "6.0.0-pre")]
        public void Parse_When_PackageIdIsSpecifiedWithVersion_Then_VersionShouldBeParsedSuccessfully(string input, string expectedId, string expectedVersion)
        {
            var commandLineParser = new CommandLineParser<int, int>();
            var awaitPublishVerb = commandLineParser.AddVerb(new AwaitPublishVerb(), updateVerb => Result.Success(0));

            commandLineParser.Parse(input);

            awaitPublishVerb.PackageIdAndVersion.Id.Should().Be(expectedId);
            awaitPublishVerb.PackageIdAndVersion.NuGetVersion.Should().Be(NuGetVersion.Parse(expectedVersion));
        }
    }
}