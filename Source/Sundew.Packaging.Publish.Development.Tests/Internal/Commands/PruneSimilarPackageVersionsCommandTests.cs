// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PruneSimilarPackageVersionsCommandTests.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Development.Tests.Internal.Commands;

using System.IO;
using System.Linq;
using Moq;
using Sundew.Packaging.Publish.Internal.Commands;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;
using Xunit;

public class PruneSimilarPackageVersionsCommandTests
{
    private const string AnyPackageId = "Sundew.Packaging.Publish";
    private const string AnyPackagePath = @"c:\AnyPackagePath\Sundew.Packaging.Publish.5.1.0-u20210325-221048-pre.nupkg";
    private const string AnyVersion = "5.1.0-u20210325-221048-pre";
    private readonly IFileSystem fileSystem = New.Mock<IFileSystem>();
    private readonly ILogger logger = New.Mock<ILogger>();
    private readonly PruneSimilarPackageVersionsCommand testee;

    public PruneSimilarPackageVersionsCommandTests()
    {
        this.testee = new PruneSimilarPackageVersionsCommand(this.fileSystem, this.logger);
    }

    [Fact]
    public void Prune_Then_ExpectedFilesShouldBeDeleted()
    {
        var expectedFilesNotToBeDeleted = new[] { @"c:\AnyPackagePath\Sundew.Packaging.Publish.5.1.0-u20210325-221048-pre.nupkg", @"c:\AnyPackagePath\Sundew.Packaging.Publish.6.1.0-u20210325-181048-pre.nupkg" };
        var expectedFilesToBeDeleted = new[] { @"c:\AnyPackagePath\Sundew.Packaging.Publish.5.1.0-u20210325-201048-pre.nupkg", @"c:\AnyPackagePath\Sundew.Packaging.Publish.5.1.0-u20210325-181048-pre.nupkg" };
        this.fileSystem
            .Setup(x => x.EnumerableFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
            .Returns(expectedFilesToBeDeleted.Concat(expectedFilesNotToBeDeleted));

        this.testee.Prune(AnyPackagePath, AnyPackageId, AnyVersion);

        foreach (var s in expectedFilesToBeDeleted)
        {
            this.fileSystem.Verify(x => x.DeleteFile(s), Times.Once);
        }

        foreach (var s in expectedFilesNotToBeDeleted)
        {
            this.fileSystem.Verify(x => x.DeleteFile(s), Times.Never);
        }
    }
}