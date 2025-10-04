// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPackageToLocalSourceCommandTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Development.Tests.Internal.Commands;

using System.IO;
using AwesomeAssertions;
using Moq;
using Sundew.Packaging.Publish.Internal.Commands;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;
using Xunit;

public class CopyPackageToLocalSourceCommandTests
{
    private const string APackageIdText = "PackageId";
    private const string APackagePathText = "A_package_path.nupkg";
    private const string ASourceText = "A_source_path";
    private static readonly string ExpectedDestinationPath = Path.Combine(ASourceText, APackageIdText, APackagePathText);
    private readonly IFileSystem fileSystem;
    private readonly CopyPackageToLocalSourceCommand testee;
    private readonly ILogger logger;

    public CopyPackageToLocalSourceCommandTests()
    {
        this.fileSystem = New.Mock<IFileSystem>();
        this.logger = New.Mock<ILogger>();
        this.testee = new CopyPackageToLocalSourceCommand(this.fileSystem, this.logger);
    }

    [Fact]
    public void Add_Then_ResultShouldBeExpectedDestinationPath()
    {
        var result = this.testee.Add(APackageIdText, APackagePathText, ASourceText, false);

        result.Should().Be(ExpectedDestinationPath);
    }

    [Fact]
    public void Add_Then_FileSystemCopyShouldBeCalledWithExpectedDestinationPath()
    {
        this.testee.Add(APackageIdText, APackagePathText, ASourceText, false);

        this.fileSystem.Verify(x => x.Copy(APackagePathText, ExpectedDestinationPath, true), Times.Once);
    }
}