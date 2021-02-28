// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishTaskTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests
{
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Moq;
    using NuGet.Configuration;
    using Sundew.Packaging.Publish;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Sundew.Packaging.Publish.Internal.NuGet.Configuration;
    using Xunit;

    public class PublishTaskTests
    {
        private const string ExpectedPackageId = "PackageId";
        private const string ProjectDir = @"c:\sdb";
        private const string PackageOutputPath = @"c:\sdb\bin";
        private const string OutputPath = @"c:\sdb\bin\Debug";
        private const string Version = "1.0.0";
        private const int TimeoutInSeconds = 300;
        private static readonly string ExpectedPackagePath = Path.Combine(PackageOutputPath, $"{ExpectedPackageId}.{Version}.nupkg");
        private static readonly string ExpectedSymbolsPackagePath = Path.Combine(PackageOutputPath, $"{ExpectedPackageId}.{Version}.snupkg");
        private static readonly string ExpectedSymbolsPackagePathLong = Path.Combine(PackageOutputPath, $"{ExpectedPackageId}.{Version}.symbols.nupkg");
        private static readonly string ExpectedPdbPath = Path.Combine(PackageOutputPath, $"{ExpectedPackageId}.{Version}.pdb");
        private readonly IPushPackageCommand pushPackageCommand = New.Mock<IPushPackageCommand>();
        private readonly ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand = New.Mock<ICopyPackageToLocalSourceCommand>();
        private readonly ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand = New.Mock<ICopyPdbToSymbolCacheCommand>();
        private readonly IFileSystem fileSystem = New.Mock<IFileSystem>().SetDefaultValue(DefaultValue.Mock);
        private readonly ISettingsFactory settingsFactory = New.Mock<ISettingsFactory>();
        private readonly IBuildEngine buildEngine = New.Mock<IBuildEngine>();
        private readonly IPersistNuGetVersionCommand persistNuGetVersionCommand = New.Mock<IPersistNuGetVersionCommand>();
        private readonly ICommandLogger commandLogger = New.Mock<ICommandLogger>();
        private readonly PublishTask testee;

        public PublishTaskTests()
        {
            this.testee = new PublishTask(
                this.pushPackageCommand,
                this.copyPackageToLocalSourceCommand,
                this.copyPdbToSymbolCacheCommand,
                this.fileSystem,
                this.settingsFactory,
                this.persistNuGetVersionCommand,
                this.commandLogger)
            {
                BuildEngine = this.buildEngine,
                PackageId = ExpectedPackageId,
                ProjectDir = ProjectDir,
                PackageOutputPath = PackageOutputPath,
                OutputPath = OutputPath,
                Version = Version,
                PublishPackages = true,
                TimeoutInSeconds = TimeoutInSeconds,
                WorkingDirectory = @"Any\LocalSourcePath",
            };

            this.fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        }

        [Fact]
        public void Execute_When_SourceIsLocalFile_Then_CopyPackageToLocalSourceCommandShouldBeExecuted()
        {
            this.testee.Source = @"c:\temp\packages";

            this.testee.Execute();

            this.copyPackageToLocalSourceCommand.Verify(x => x.Add(ExpectedPackageId, ExpectedPackagePath, this.testee.Source, false, It.IsAny<ICommandLogger>()), Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsRemote_Then_PushPackageCommandShouldBeExecuted()
        {
            this.testee.Source = "http://nuget.org";

            this.testee.Execute();

            this.pushPackageCommand.Verify(
                x => x.PushAsync(
                ExpectedPackagePath,
                this.testee.Source,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                TimeoutInSeconds,
                It.IsAny<ISettings>(),
                false,
                false,
                It.IsAny<NuGet.Common.ILogger>(),
                It.IsAny<ICommandLogger>()),
                Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsLocalAndCopyPdbToSymbolCacheIsSet_Then_PushPackageCommandShouldBeExecuted()
        {
            this.testee.PackInputs = new ITaskItem[] { new TaskItem(ExpectedPdbPath) };
            this.testee.Source = @"c:\temp\packages";
            this.testee.SymbolCacheDir = @"c:\temp\symbol-cache";
            this.testee.CopyLocalSourcePdbToSymbolCache = true;

            this.testee.Execute();

            this.copyPdbToSymbolCacheCommand.Verify(x => x.AddAndCleanCache(ExpectedPdbPath, this.testee.SymbolCacheDir, It.IsAny<ISettings>(), It.IsAny<ICommandLogger>()), Times.Once);
        }

        [Fact]
        public void Execute_Then_PackagePathsShouldBeExpectedResult()
        {
            this.testee.Execute();

            this.testee.PackagePaths!.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath, ExpectedSymbolsPackagePath });
        }

        [Fact]
        public void Execute_Then_PersistNuGetVersionShouldBeCalled()
        {
            this.testee.Execute();

            this.persistNuGetVersionCommand.Verify(x => x.Save(Version, OutputPath, It.IsAny<string>(), It.IsAny<ICommandLogger>()), Times.Once);
        }

        [Fact]
        public void Execute_When_PackagePushLogFormatIsSet_Then_MessageShouldBeLoggedWithFormat()
        {
            this.testee.CommandPrefix = "##";
            this.testee.Source = "http://nuget.org";
            this.testee.PublishLogFormats = "{0}vso[task.setvariable variable=package_{1}]{3}-{4}-{2}";

            this.testee.Execute();

            this.commandLogger.Verify(x => x.LogImportant($"##vso[task.setvariable variable=package_{ExpectedPackageId}]{ExpectedPackagePath}-{this.testee.Source}-{Version}"), Times.Once);
        }

        [Fact]
        public void Execute_When_PackagePushLogFormatIsSetNotSet_Then_NothingIsLogged()
        {
            this.testee.Execute();

            this.commandLogger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_SourceIsRemoteAndLocalPackageIsNotAllowed_Then_PublishPackageLoggingShouldBeDisabled()
        {
            this.testee.Source = "http://nuget.org";
            this.testee.PublishLogFormats = "{1}";
            this.testee.AllowLocalSource = false;

            this.testee.Execute();

            this.commandLogger.Verify(x => x.LogImportant(ExpectedPackageId), Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsLocalAndLocalPackageIsNotAllowed_Then_PublishPackageLoggingShouldBeDisabled()
        {
            this.testee.Source = @"c:\temp\packages";
            this.testee.PublishLogFormats = "{0}";
            this.testee.AllowLocalSource = false;

            this.testee.Execute();

            this.commandLogger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_SourceIsNull_Then_PublishPackageLoggingShouldBeDisabled()
        {
            this.testee.PublishLogFormats = "{0}";

            this.testee.Execute();

            this.commandLogger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_NoSymbolPackageExists_Then_PackagePathsShouldBeExpectedResult()
        {
            this.fileSystem.Setup(x => x.FileExists(ExpectedSymbolsPackagePath)).Returns(false);
            this.fileSystem.Setup(x => x.FileExists(ExpectedSymbolsPackagePathLong)).Returns(false);

            this.testee.Execute();

            this.testee.PackagePaths!.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath });
        }
    }
}