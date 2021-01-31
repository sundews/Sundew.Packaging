// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPackageToLocalSourceCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal.Commands
{
    using System;
    using System.IO;
    using FluentAssertions;
    using Moq;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Xunit;

    public class CopyPackageToLocalSourceCommandTests
    {
        private const string APackageIdText = "PackageId";
        private const string APackagePathText = "A_package_path.nupkg";
        private const string ASourceText = "A_source_path";
        private static readonly string ExpectedDestinationPath = Path.Combine(ASourceText, APackageIdText, APackagePathText);
        private readonly IFileSystem fileSystem;
        private readonly CopyPackageToLocalSourceCommand testee;
        private readonly ICommandLogger commandLogger;

        public CopyPackageToLocalSourceCommandTests()
        {
            this.fileSystem = New.Mock<IFileSystem>();
            this.testee = new CopyPackageToLocalSourceCommand(this.fileSystem);
            this.commandLogger = New.Mock<ICommandLogger>();
        }

        [Fact]
        public void Add_Then_ResultShouldBeExpectedDestinationPath()
        {
            var result = this.testee.Add(APackageIdText, APackagePathText, ASourceText, false, this.commandLogger);

            result.Should().Be(ExpectedDestinationPath);
        }

        [Fact]
        public void Add_Then_FileSystemCopyShouldBeCalledWithExpectedDestinationPath()
        {
            this.testee.Add(APackageIdText, APackagePathText, ASourceText, false, this.commandLogger);

            this.fileSystem.Verify(x => x.Copy(APackagePathText, ExpectedDestinationPath, true), Times.Once);
        }
    }
}