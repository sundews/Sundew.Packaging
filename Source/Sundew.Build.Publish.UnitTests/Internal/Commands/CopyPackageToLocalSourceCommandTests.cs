// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPackageToLocalSourceCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal.Commands
{
    using System.IO;
    using FluentAssertions;
    using NSubstitute;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;
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
            this.fileSystem = Substitute.For<IFileSystem>();
            this.testee = new CopyPackageToLocalSourceCommand(this.fileSystem);
            this.commandLogger = Substitute.For<ICommandLogger>();
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

            this.fileSystem.Received(1).Copy(APackagePathText, ExpectedDestinationPath, true);
        }
    }
}