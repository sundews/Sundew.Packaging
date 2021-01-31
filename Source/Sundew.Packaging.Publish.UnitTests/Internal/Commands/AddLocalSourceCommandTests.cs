// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddLocalSourceCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using Moq;
    using NuGet.Configuration;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;
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
            this.settings = New.Mock<ISettings>();
            this.defaultSettings = New.Mock<ISettings>();
            this.fileSystem = New.Mock<IFileSystem>();
            this.settingsFactory = New.Mock<ISettingsFactory>();
            this.testee = new AddLocalSourceCommand(this.fileSystem, this.settingsFactory);
            this.settingsFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(this.settings);
            this.settingsFactory.Setup(x => x.LoadSpecificSettings(ASolutionDirText, AddLocalSourceCommand.NuGetConfigFileName)).Returns(this.settings);
            this.settingsFactory.Setup(x => x.LoadDefaultSettings(ASolutionDirText)).Returns(this.defaultSettings);
        }

        [Fact]
        public void Add_When_LocalSourceNameDoesNotExist_Then_AddOrUpdateAndSaveToDiskShouldBeCalled()
        {
            var result = this.testee.Add(ASolutionDirText, ALocalSourceNameText, ADefaultLocalSourceText);

            this.settings.Verify(x => x.AddOrUpdate(AddLocalSourceCommand.PackageSourcesText, It.Is<AddItem>(x => x.Key == ALocalSourceNameText && x.Value == ADefaultLocalSourceText)), Times.Once);
            this.settings.Verify(x => x.SaveToDisk(), Times.Once);
            result.Path.Should().Be(ADefaultLocalSourceText);
        }

        [Fact]
        public void Add_When_LocalSourceNameDoesExist_Then_AddOrUpdateAndSaveToDiskShouldNotBeCalled()
        {
            this.ArrangeLocalSourceSetting();

            var result = this.testee.Add(ASolutionDirText, ALocalSourceNameText, ADefaultLocalSourceText);

            this.settings.Verify(x => x.AddOrUpdate(It.IsAny<string>(), It.IsAny<AddItem>()), Times.Never);
            this.settings.Verify(x => x.SaveToDisk(), Times.Never);
            result.Path.Should().Be(ExpectedLocalSourceText);
        }

        [Fact]
        public void Add_When_WorkingDirectoryIsUndefinedAndCurrentDirectoryIsARoot_Then_ArgumentExceptionShouldBeThrown()
        {
            this.fileSystem.Setup(x => x.GetCurrentDirectory()).Returns("c:\\");
            Action act = () => this.testee.Add("*Undefined*", ALocalSourceNameText, ADefaultLocalSourceText);

            act.Should().ThrowExactly<ArgumentException>();
        }

        private void ArrangeLocalSourceSetting()
        {
            this.defaultSettings.Setup(x => x.GetSection(AddLocalSourceCommand.PackageSourcesText)).Returns(
                (SettingSection)typeof(VirtualSettingSection).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
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