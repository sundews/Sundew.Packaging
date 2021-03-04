// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPdbToSymbolCacheCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.UnitTests.Internal.Commands
{
    using System.IO;
    using System.Reflection;
    using Moq;
    using NuGet.Configuration;
    using Sundew.Packaging.Publish.Internal.Commands;
    using Sundew.Packaging.Publish.Internal.IO;
    using Xunit;

    public class CopyPdbToSymbolCacheCommandTests
    {
        private const string APdbFilePathPdbText = "A_PDB_file_path.pdb";
        private const string ASymbolCacheDirectoryPathText = "A_symbol_cache_path";
        private const string PdbId = "AB8B0DC75B5744449D425DA7B2A42E98ffffffff";
        private static readonly string ExpectedSppFilePath = Path.Combine(ASymbolCacheDirectoryPathText, APdbFilePathPdbText, PdbId, ".spp");
        private static readonly string ExpectedDestinationPdbPathText = Path.Combine(ASymbolCacheDirectoryPathText, APdbFilePathPdbText, PdbId, APdbFilePathPdbText);
        private readonly IFileSystem fileSystem;
        private readonly ICommandLogger commandLogger;
        private readonly ISettings settings;
        private readonly CopyPdbToSymbolCacheCommand testee;

        public CopyPdbToSymbolCacheCommandTests()
        {
            this.fileSystem = New.Mock<IFileSystem>();
            this.testee = new CopyPdbToSymbolCacheCommand(this.fileSystem);
            this.commandLogger = New.Mock<ICommandLogger>();
            this.settings = New.Mock<ISettings>();
            this.fileSystem.Setup(x => x.ReadAllBytes(It.IsAny<string>())).Returns(GetBytes(Assembly.GetExecutingAssembly().GetManifestResourceStream("Sundew.Packaging.Publish.UnitTests.Internal.Commands.Sundew.Packaging.Publish.pdb")!));
        }

        [Fact]
        public void AddAndCleanCache_Then_FileSystemCopyShouldBeCalled()
        {
            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Verify(x => x.Copy(APdbFilePathPdbText, ExpectedDestinationPdbPathText, true), Times.Once);
        }

        [Fact]
        public void AddAndCleanCache_When_SbpFileExists_Then_FileSystemDeleteDirectoryShouldBeCalled()
        {
            var expectedSbpDirectoryPath = Path.GetDirectoryName(ExpectedSppFilePath)!;
            this.fileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            this.fileSystem.Setup(x => x.EnumerableFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(new[] { ExpectedSppFilePath });

            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Verify(x => x.DeleteDirectory(expectedSbpDirectoryPath, true), Times.Once);
        }

        [Fact]
        public void AddAndCleanCache_Then_SbpFileShouldBeWrittenToSymbolCacheDirectory()
        {
            this.fileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            this.fileSystem.Setup(x => x.EnumerableFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(new[] { ExpectedSppFilePath });

            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Verify(x => x.WriteAllText(ExpectedSppFilePath, string.Empty), Times.Once);
        }

        private static byte[] GetBytes(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}