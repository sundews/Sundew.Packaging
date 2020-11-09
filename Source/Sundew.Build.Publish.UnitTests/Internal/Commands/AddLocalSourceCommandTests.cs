// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddLocalSourceCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using NSubstitute;
    using NuGet.Configuration;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;
    using Sundew.Build.Publish.Internal.NuGet.Configuration;
    using Xunit;

    public class AddLocalSourceCommandTests
    {
        private const string ASolutionDirText = "ASolutionDirText";
        private const string ALocalSourceNameText = "ALocalSourceName";
        private const string ADefaultLocalSourceText = "ADefaultLocalSourceText";
        private const string ExpectedLocalSourceText = "ExpectedLocalSourceText";
        private readonly ISettings settings;
        private readonly IFileSystem fileSystem;
        private readonly ISettingsFactory settingsFactory;
        private readonly AddLocalSourceCommand testee;
        private readonly ISettings defaultSettings;

        public AddLocalSourceCommandTests()
        {
            this.settings = Substitute.For<ISettings>();
            this.defaultSettings = Substitute.For<ISettings>();
            this.fileSystem = Substitute.For<IFileSystem>();
            this.settingsFactory = Substitute.For<ISettingsFactory>();
            this.testee = new AddLocalSourceCommand(this.fileSystem, this.settingsFactory);
            this.settingsFactory.Create(Arg.Any<string>(), Arg.Any<string>(), false).Returns(this.settings);
            this.settingsFactory.LoadSpecificSettings(ASolutionDirText, AddLocalSourceCommand.NuGetConfigFileName).Returns(this.settings);
            this.settingsFactory.LoadDefaultSettings(ASolutionDirText).Returns(this.defaultSettings);
        }

        [Fact]
        public void Add_When_LocalSourceNameDoesNotExist_Then_AddOrUpdateAndSaveToDiskShouldBeCalled()
        {
            var result = this.testee.Add(ASolutionDirText, ALocalSourceNameText, ADefaultLocalSourceText);

            this.settings.Received(1).AddOrUpdate(AddLocalSourceCommand.PackageSourcesText, Arg.Is<AddItem>(x => x.Key == ALocalSourceNameText && x.Value == ADefaultLocalSourceText));
            this.settings.Received(1).SaveToDisk();
            result.Path.Should().Be(ADefaultLocalSourceText);
        }

        [Fact]
        public void Add_When_LocalSourceNameDoesExist_Then_AddOrUpdateAndSaveToDiskShouldNotBeCalled()
        {
            this.ArrangeLocalSourceSetting();

            var result = this.testee.Add(ASolutionDirText, ALocalSourceNameText, ADefaultLocalSourceText);

            this.settings.Received(0).AddOrUpdate(Arg.Any<string>(), Arg.Any<AddItem>());
            this.settings.Received(0).SaveToDisk();
            result.Path.Should().Be(ExpectedLocalSourceText);
        }

        [Fact]
        public void Add_When_WorkingDirectoryIsUndefinedAndCurrentDirectoryIsARoot_Then_ArgumentExceptionShouldBeThrown()
        {
            this.fileSystem.GetCurrentDirectory().Returns("c:\\");
            Action act = () => this.testee.Add("*Undefined*", ALocalSourceNameText, ADefaultLocalSourceText);

            act.Should().ThrowExactly<ArgumentException>();
        }

        private void ArrangeLocalSourceSetting()
        {
            this.defaultSettings.GetSection(AddLocalSourceCommand.PackageSourcesText).Returns(
                typeof(VirtualSettingSection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                    .OrderByDescending(x => x.GetParameters().Length).First().Invoke(
                        new object[]
                        {
                            AddLocalSourceCommand.PackageSourcesText,
                            new Dictionary<string, string>(),
                            new List<SettingItem> { new AddItem(ALocalSourceNameText, ExpectedLocalSourceText) },
                        }));
        }
    }
}