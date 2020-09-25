// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdjustProjectReferenceVersionsTaskTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using NSubstitute;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;
    using Xunit;

    public class AdjustProjectReferenceVersionsTaskTests
    {
        private const string ProjectReference = "Reference.csproj";
        private const string DllPath = "Reference.dll";
        private const string AProjectVersion = "3.0.0";
        private readonly AdjustProjectReferenceVersionsTask testee;
        private readonly IFileSystem fileSystem = Substitute.For<IFileSystem>();
        private readonly ICommandLogger commandLogger = Substitute.For<ICommandLogger>();
        private readonly TaskItem dllTaskItem = new TaskItem(DllPath, new Dictionary<string, string> { { AdjustProjectReferenceVersionsTask.MSBuildSourceProjectFileName, ProjectReference } });
        private readonly TaskItem projectReferenceItem = new TaskItem(ProjectReference, new Dictionary<string, string> { { AdjustProjectReferenceVersionsTask.ProjectVersionName, AProjectVersion } });

        public AdjustProjectReferenceVersionsTaskTests()
        {
            this.testee = new AdjustProjectReferenceVersionsTask(this.fileSystem, this.commandLogger);
        }

        [Fact]
        public void Execute_Then_AdjustedProjectReferencesVersionShouldBeExpectedVersion()
        {
            this.testee.ResolvedProjectReferences = new ITaskItem[] { this.dllTaskItem, };
            this.testee.ProjectReferences = new ITaskItem[] { this.projectReferenceItem, };
            this.fileSystem.FileExists(Arg.Any<string>()).Returns(true);
            const string expectedVersion = "3.0.0-pre-u20201010-150729";
            this.fileSystem.ReadAllText(Arg.Any<string>()).Returns(expectedVersion);

            this.testee.Execute();

            this.testee.AdjustedProjectReferences.FirstOrDefault().GetMetadata(AdjustProjectReferenceVersionsTask.ProjectVersionName).Should().Be(expectedVersion);
            this.commandLogger.Received(1).LogInfo(Arg.Any<string>());
        }

        [Fact]
        public void Execute_When_VersionDoesNotChange_Then_LogInfoShouldNotBeCalled()
        {
            this.testee.ResolvedProjectReferences = new ITaskItem[] { this.dllTaskItem, };
            this.testee.ProjectReferences = new ITaskItem[] { this.projectReferenceItem, };
            this.fileSystem.FileExists(Arg.Any<string>()).Returns(true);
            this.fileSystem.ReadAllText(Arg.Any<string>()).Returns(AProjectVersion);

            this.testee.Execute();

            this.commandLogger.Received(0).LogInfo(Arg.Any<string>());
        }
    }
}