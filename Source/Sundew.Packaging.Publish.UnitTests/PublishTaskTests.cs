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
    using Sundew.Packaging.Publish.Internal;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Versioning;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.NuGet.Configuration;
    using Xunit;
    using ILogger = Sundew.Packaging.Versioning.Logging.ILogger;

    public class PublishTaskTests
    {
        private const string ExpectedPackageId = "PackageId";
        private const string Version = "1.0.0";
        private const int TimeoutInSeconds = 300;
        private static readonly string ProjectDir = Paths.EnsurePlatformPath(@"c:\sdb");
        private static readonly string PackageOutputPath = Paths.EnsurePlatformPath(@"c:\sdb\bin");
        private static readonly string AnyPackageInfoFilePath = Paths.EnsurePlatformPath(@"c:\anypublishinfo\path");
        private static readonly string OutputPath = Paths.EnsurePlatformPath(@"c:\sdb\bin\Debug");
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
        private readonly IPublishInfoProvider publishInfoProvider = New.Mock<IPublishInfoProvider>();
        private readonly ILogger logger = New.Mock<ILogger>();
        private readonly PublishTask testee;

        public PublishTaskTests()
        {
            this.testee = new PublishTask(
                this.fileSystem,
                this.publishInfoProvider,
                this.pushPackageCommand,
                this.copyPackageToLocalSourceCommand,
                this.copyPdbToSymbolCacheCommand,
                this.settingsFactory,
                this.logger)
            {
                BuildEngine = this.buildEngine,
                PublishInfoFilePath = AnyPackageInfoFilePath,
                PackageId = ExpectedPackageId,
                ProjectDir = ProjectDir,
                PackageOutputPath = PackageOutputPath,
                OutputPath = OutputPath,
                TimeoutInSeconds = TimeoutInSeconds,
                SolutionDir = @"Any/LocalSourcePath",
            };

            this.fileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
        }

        [Fact]
        public void Execute_When_SourceIsLocalFile_Then_CopyPackageToLocalSourceCommandShouldBeExecuted()
        {
            var publishInfo = this.ArrangePublishInfo(@"c:/temp/packages", Version);

            this.testee.Execute();

            this.copyPackageToLocalSourceCommand.Verify(x => x.Add(ExpectedPackageId, ExpectedPackagePath, publishInfo.PushSource, false), Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsRemote_Then_PushPackageCommandShouldBeExecuted()
        {
            var publishInfo = this.ArrangePublishInfo("http://nuget.org", Version);

            this.testee.Execute();

            this.pushPackageCommand.Verify(
                x => x.PushAsync(
                ExpectedPackagePath,
                publishInfo.PushSource,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                TimeoutInSeconds,
                It.IsAny<ISettings>(),
                false,
                false),
                Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsLocalAndCopyPdbToSymbolCacheIsSet_Then_PushPackageCommandShouldBeExecuted()
        {
            var publishInfo = this.ArrangePublishInfo(Paths.EnsurePlatformPath(@"c:/temp/packages"), Version);
            this.testee.PackInputs = new ITaskItem[] { new TaskItem(ExpectedPdbPath) };
            this.testee.SymbolCacheDir = @"c:/temp/symbol-cache";
            this.testee.CopyLocalSourcePdbToSymbolCache = true;

            this.testee.Execute();

            this.copyPdbToSymbolCacheCommand.Verify(x => x.AddAndCleanCache(new[] { ExpectedPdbPath }, this.testee.SymbolCacheDir, It.IsAny<ISettings>()), Times.Once);
        }

        [Fact]
        public void Execute_Then_PackagePathsShouldBeExpectedResult()
        {
            var publishInfo = this.ArrangePublishInfo(Paths.EnsurePlatformPath(@"c:/temp/packages"), Version);

            this.testee.Execute();

            this.testee.PackagePaths!.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath, ExpectedSymbolsPackagePath });
        }

        [Fact]
        public void Execute_When_PackagePushLogFormatIsSet_Then_MessageShouldBeLoggedWithFormat()
        {
            this.testee.Parameter = "##";
            var publishInfo = this.ArrangePublishInfo("http://nuget.org", Version);
            this.testee.PublishLogFormats = "{14}vso[task.setvariable variable=package_{0}]{3}-{6}-{1}";

            this.testee.Execute();

            this.logger.Verify(x => x.LogImportant($"##vso[task.setvariable variable=package_{ExpectedPackageId}]{ExpectedPackagePath}-{publishInfo.PushSource}-{Version}"), Times.Once);
        }

        [Fact]
        public void Execute_When_PackagePushLogFormatIsSetNotSet_Then_NothingIsLogged()
        {
            var publishInfo = this.ArrangePublishInfo(Paths.EnsurePlatformPath(@"c:/temp/packages"), Version);

            this.testee.Execute();

            this.logger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_SourceIsRemoteAndLocalPackageIsNotAllowed_Then_PublishPackageLoggingShouldBeEnabled()
        {
            var publishInfo = this.ArrangePublishInfo("http://nuget.org", Version);
            this.testee.PublishLogFormats = "{0}";
            this.testee.AllowLocalSource = false;

            this.testee.Execute();

            this.logger.Verify(x => x.LogImportant(ExpectedPackageId), Times.Once);
        }

        [Theory]
        [InlineData("{0} > file.txt", ExpectedPackageId, "file.txt")]
        [InlineData("{0}  > file.txt", ExpectedPackageId + " ", "file.txt")]
        public void Execute_When_SourceIsRemoteAndLocalPackageIsNotAllowed_Then_AppendPublishPackageLoggingShouldBeEnabled(string appendPublishFileLogFormats, string expectedContext, string expectedFile)
        {
            var publishInfo = this.ArrangePublishInfo("http://nuget.org", Version);
            this.testee.AppendPublishFileLogFormats = appendPublishFileLogFormats;
            this.testee.AllowLocalSource = false;

            this.testee.Execute();

            this.fileSystem.Verify(x => x.AppendAllText(It.Is<string>(x => x.EndsWith(expectedFile)), expectedContext), Times.Once);
        }

        [Fact]
        public void Execute_When_SourceIsLocalAndLocalPackageIsNotAllowed_Then_PublishPackageLoggingShouldBeDisabled()
        {
            var publishInfo = this.ArrangePublishInfo(Paths.EnsurePlatformPath(@"c:/temp/packages"), Version);
            this.testee.PublishLogFormats = "{0}";
            this.testee.AllowLocalSource = false;

            this.testee.Execute();

            this.logger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_SourceIsEmpty_Then_PublishPackageLoggingShouldBeDisabled()
        {
            var publishInfo = this.ArrangePublishInfo(string.Empty, Version);
            this.testee.PublishLogFormats = "{0}";

            this.testee.Execute();

            this.logger.Verify(x => x.LogImportant(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Execute_When_NoSymbolPackageExists_Then_PackagePathsShouldBeExpectedResult()
        {
            var publishInfo = this.ArrangePublishInfo(Paths.EnsurePlatformPath(@"c:/temp/packages"), Version);
            this.fileSystem.Setup(x => x.FileExists(ExpectedSymbolsPackagePath)).Returns(false);
            this.fileSystem.Setup(x => x.FileExists(ExpectedSymbolsPackagePathLong)).Returns(false);

            this.testee.Execute();

            this.testee.PackagePaths!.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath });
        }

        private PublishInfo ArrangePublishInfo(string pushSource, string version)
        {
            var publishInfo = new PublishInfo(string.Empty, string.Empty, string.Empty, pushSource, null, null, null, true, version, version, null);
            this.publishInfoProvider.Setup(x => x.Read(It.IsAny<string>())).Returns(publishInfo);
            return publishInfo;
        }
    }
}