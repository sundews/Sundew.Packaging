// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPdbToSymbolCacheCommandTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.UnitTests.Internal.Commands
{
    using System.IO;
    using System.Reflection;
    using NSubstitute;
    using NuGet.Configuration;
    using Sundew.Build.Publish.Internal.Commands;
    using Sundew.Build.Publish.Internal.IO;
    using Xunit;

    public class CopyPdbToSymbolCacheCommandTests
    {
        private const string APdbFilePathPdbText = "A_PDB_file_path.pdb";
        private const string ASymbolCacheDirectoryPathText = "A_symbol_cache_path";
        private const string PdbId = "AB8B0DC75B5744449D425DA7B2A42E98ffffffff";
        private static readonly string ExpectedSbpFilePath = Path.Combine(ASymbolCacheDirectoryPathText, APdbFilePathPdbText, PdbId, ".sbp");
        private static readonly string ExpectedDestinationPdbPathText = Path.Combine(ASymbolCacheDirectoryPathText, APdbFilePathPdbText, PdbId, APdbFilePathPdbText);
        private readonly IFileSystem fileSystem;
        private readonly ICommandLogger commandLogger;
        private readonly ISettings settings;
        private readonly CopyPdbToSymbolCacheCommand testee;

        public CopyPdbToSymbolCacheCommandTests()
        {
            this.fileSystem = Substitute.For<IFileSystem>();
            this.testee = new CopyPdbToSymbolCacheCommand(this.fileSystem);
            this.commandLogger = Substitute.For<ICommandLogger>();
            this.settings = Substitute.For<ISettings>();
            this.fileSystem.ReadAllBytes(Arg.Any<string>()).Returns(GetBytes(Assembly.GetExecutingAssembly().GetManifestResourceStream("Sundew.Build.Publish.UnitTests.Internal.Commands.Sundew.Build.Publish.pdb")));
        }

        [Fact]
        public void AddAndCleanCache_Then_FileSystemCopyShouldBeCalled()
        {
            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Received(1).Copy(APdbFilePathPdbText, ExpectedDestinationPdbPathText, true);
        }

        [Fact]
        public void AddAndCleanCache_When_SbpFileExists_Then_FileSystemDeleteDirectoryShouldBeCalled()
        {
            var expectedSbpDirectoryPath = Path.GetDirectoryName(ExpectedSbpFilePath);
            this.fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);
            this.fileSystem.EnumerableFiles(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new[] { ExpectedSbpFilePath });

            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Received(1).DeleteDirectory(expectedSbpDirectoryPath, true);
        }

        [Fact]
        public void AddAndCleanCache_Then_SbpFileShouldBeWrittenToSymbolCacheDirectory()
        {
            this.fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);
            this.fileSystem.EnumerableFiles(Arg.Any<string>(), Arg.Any<string>(), SearchOption.AllDirectories).Returns(new[] { ExpectedSbpFilePath });

            this.testee.AddAndCleanCache(APdbFilePathPdbText, ASymbolCacheDirectoryPathText, this.settings, this.commandLogger);

            this.fileSystem.Received(1).WriteAllText(ExpectedSbpFilePath, string.Empty);
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