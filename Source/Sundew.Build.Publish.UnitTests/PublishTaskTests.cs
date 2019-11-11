// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublishTaskTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests
{
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NSubstitute;
    using NuGet.Configuration;
    using Sundew.Build.Publish.Commands;
    using Sundew.Build.Publish.Internal.IO;
    using Sundew.Build.Publish.Internal.NuGet.Configuration;
    using Xunit;

    public class PublishTaskTests
    {
        private const string ExpectedPackageId = "PackageId";
        private const string ProjectDir = @"c:\sdb";
        private const string OutputPath = @"c:\sdb\bin";
        private const string Version = "1.0.0";
        private const int TimeoutInSeconds = 300;
        private static readonly string ExpectedPackagePath = Path.Combine(OutputPath, $"{ExpectedPackageId}.{Version}.nupkg");
        private static readonly string ExpectedSymbolsPackagePath = Path.Combine(OutputPath, $"{ExpectedPackageId}.{Version}.snupkg");
        private static readonly string ExpectedSymbolsPackagePathLong = Path.Combine(OutputPath, $"{ExpectedPackageId}.{Version}.symbols.nupkg");
        private static readonly string ExpectedPdbPath = Path.Combine(OutputPath, $"{ExpectedPackageId}.{Version}.pdb");
        private readonly IPushPackageCommand pushPackageCommand = Substitute.For<IPushPackageCommand>();
        private readonly ICopyPackageToLocalSourceCommand copyPackageToLocalSourceCommand = Substitute.For<ICopyPackageToLocalSourceCommand>();
        private readonly ICopyPdbToSymbolCacheCommand copyPdbToSymbolCacheCommand = Substitute.For<ICopyPdbToSymbolCacheCommand>();
        private readonly IFileSystem fileSystem = Substitute.For<IFileSystem>();
        private readonly ISettingsFactory settingsFactory = Substitute.For<ISettingsFactory>();
        private readonly IBuildEngine buildEngine = Substitute.For<IBuildEngine>();
        private readonly PublishTask testee;

        public PublishTaskTests()
        {
            this.testee = new PublishTask(
                this.pushPackageCommand,
                this.copyPackageToLocalSourceCommand,
                this.copyPdbToSymbolCacheCommand,
                this.fileSystem,
                this.settingsFactory)
            {
                BuildEngine = this.buildEngine,
                PackageId = ExpectedPackageId,
                ProjectDir = ProjectDir,
                OutputPath = OutputPath,
                Version = Version,
                PublishPackages = true,
                TimeoutInSeconds = TimeoutInSeconds,
            };

            this.fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        }

        [Fact]
        public void Execute_When_SourceIsLocalFile_Then_CopyPackageToLocalSourceCommandShouldBeExecuted()
        {
            this.testee.Source = @"c:\temp\packages";

            this.testee.Execute();

            this.copyPackageToLocalSourceCommand.Received(1).Add(ExpectedPackageId, ExpectedPackagePath, this.testee.Source, false, Arg.Any<ICommandLogger>());
        }

        [Fact]
        public void Execute_When_SourceIsRemote_Then_PushPackageCommandShouldBeExecuted()
        {
            this.testee.Source = @"http://nuget.org";

            this.testee.Execute();

            this.pushPackageCommand.Received(1).PushAsync(
                ExpectedPackagePath,
                this.testee.Source,
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                TimeoutInSeconds,
                Arg.Any<ISettings>(),
                false,
                false,
                Arg.Any<NuGet.Common.ILogger>(),
                Arg.Any<ICommandLogger>());
        }

        [Fact]
        public void Execute_When_SourceIsLocalAndCopyPdbToSymbolCacheIsSet_Then_PushPackageCommandShouldBeExecuted()
        {
            this.testee.PackInputs = new ITaskItem[] { new TaskItem(ExpectedPdbPath) };
            this.testee.Source = @"c:\temp\packages";
            this.testee.SymbolCacheDir = @"c:\temp\symbol-cache";
            this.testee.CopyLocalSourcePdbToSymbolCache = true;

            this.testee.Execute();

            this.copyPdbToSymbolCacheCommand.Received(1).AddAndCleanCache(ExpectedPdbPath, this.testee.SymbolCacheDir, Arg.Any<ISettings>(), Arg.Any<ICommandLogger>());
        }

        [Fact]
        public void Execute_Then_PackagePathsShouldBeExpectedResult()
        {
            this.testee.Execute();

            this.testee.PackagePaths.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath, ExpectedSymbolsPackagePath });
        }

        [Fact]
        public void Execute_When_NoSymbolPackageExists_Then_PackagePathsShouldBeExpectedResult()
        {
            this.fileSystem.FileExists(ExpectedSymbolsPackagePath).Returns(false);
            this.fileSystem.FileExists(ExpectedSymbolsPackagePathLong).Returns(false);

            this.testee.Execute();

            this.testee.PackagePaths.Select(x => x.ItemSpec).Should().Equal(new[] { ExpectedPackagePath });
        }
    }
}