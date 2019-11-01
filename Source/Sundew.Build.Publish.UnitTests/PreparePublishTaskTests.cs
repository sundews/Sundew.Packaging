// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PreparePublishTaskTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NSubstitute;
    using NuGet.Configuration;
    using Sundew.Base.Time;
    using Sundew.Build.Publish.Commands;
    using Sundew.Build.Publish.Internal;
    using Xunit;
    using BindingFlags = System.Reflection.BindingFlags;

    public class PreparePublishTaskTests
    {
        private const string ExpectedPreVersion = "1.0.1-pre-u20160108-173613";
        private const string ExpectedDevVersion = "1.0.1-dev-u20160108-173613";
        private const string ExpectedDefaultPushSource = "http://nuget.org";

        private readonly PreparePublishTask testee;
        private readonly IAddLocalSourceCommand addLocalSourceCommand = Substitute.For<IAddLocalSourceCommand>();
        private readonly IDateTime dateTime = Substitute.For<IDateTime>();
        private readonly ISettings settings = Substitute.For<ISettings>();

        public PreparePublishTaskTests()
        {
            this.testee = new PreparePublishTask(this.addLocalSourceCommand, this.dateTime)
            {
                Version = "1.0.0",
            };

            this.dateTime.UtcTime.Returns(new DateTime(2016, 1, 8, 17, 36, 13));
            this.addLocalSourceCommand.Add(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns((callInfo) => new LocalSource((string)callInfo[2], this.settings));
        }

        [Fact]
        public void Execute_Then_PackageVersionShouldBeExpectedResult()
        {
            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(PreparePublishTask.DefaultLocalSource);
            this.testee.PackageVersion.Should().Be(ExpectedPreVersion);
        }

        [Fact]
        public void Execute_When_DefaultIsSet_Then_PackageVersionShouldBeExpectedPreVersion()
        {
            this.testee.SourceName = "default";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedDefaultPushSource);
            this.testee.SymbolsSource.Should().BeNull();
            this.testee.PackageVersion.Should().Be(ExpectedPreVersion);
        }

        [Fact]
        public void Execute_When_SourceNameIsDefaultStable_Then_PackageVersionShouldBeTesteeVersion()
        {
            this.testee.SourceName = "default-stable";
            this.ArrangeDefaultPushSource();

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedDefaultPushSource);
            this.testee.SymbolsSource.Should().BeNull();
            this.testee.PackageVersion.Should().Be(this.testee.Version);
        }

        [Fact]
        public void Execute_When_LocalPushSourceIsSet_Then_PushSourceShouldBeEqual()
        {
            this.testee.LocalSource = @"c:\temp";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(this.testee.LocalSource);
            this.testee.PackageVersion.Should().Be(ExpectedPreVersion);
        }

        [Fact]
        public void Execute_When_ProductionPushSourceIsSetAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual()
        {
            const string ExpectedPushSource = @"c:\temp\packages";
            const string ExpectedSymbolsPushSource = @"c:\temp\symbols";

            this.testee.ProductionSource = $@"master|{ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "master";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(this.testee.Version);
        }

        [Fact]
        public void Execute_When_DeveloperPushSourceIsSetAndPushSourceSelectorMatches_Then_PushSourceShouldBeEqual()
        {
            const string ExpectedPushSource = @"c:\temp\packages";
            const string ExpectedSymbolsPushSource = @"c:\temp\symbols";

            this.testee.ProductionSource = $@"master|https://production.com|https://production.com/symbols";
            this.testee.DevelopmentSource = $@"\w+|{ExpectedPushSource}|{ExpectedSymbolsPushSource}";
            this.testee.SourceName = "/feature/NewFeature";

            var result = this.testee.Execute();

            result.Should().BeTrue();
            this.testee.Source.Should().Be(ExpectedPushSource);
            this.testee.SymbolsSource.Should().Be(ExpectedSymbolsPushSource);
            this.testee.PackageVersion.Should().Be(ExpectedDevVersion);
        }

        private void ArrangeDefaultPushSource()
        {
            this.settings.GetSection(Source.ConfigText).Returns(
                typeof(VirtualSettingSection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
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