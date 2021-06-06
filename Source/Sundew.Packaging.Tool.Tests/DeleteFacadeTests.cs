// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteFacadeTests.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Moq;
    using NUnit.Framework;
    using Sundew.Base.Collections;
    using Sundew.Packaging.Tool.Delete;

    [TestFixture]
    public class DeleteFacadeTests
    {
        private const string AnyPath = @"c:\temp";
        private const string AnyText = "AnyText.txt";
        private const string AnyImage = "AnyImage.png";
        private const string AnySubImage = @"Sub\AnyImage.png";
        private const string AnyPackage = "AnyPackage.nupkg";
        private const string SpecificPackage = "SpecificPackage.nupkg";
        private const string AnySubPackage = @"Sub\AnyPackage.nupkg";

        private static readonly List<string> Files = new List<string>
        {
            AnyText,
            AnyImage,
            AnyPackage,
            SpecificPackage,
            AnySubImage,
            AnySubPackage,
        };

        private DeleteFacade? testee;

        private IFileSystem? fileSystem;

        [SetUp]
        public void Setup()
        {
            this.fileSystem = New.Mock<IFileSystem>().SetDefaultValue(DefaultValue.Mock);
            this.testee = new DeleteFacade(this.fileSystem, New.Mock<IDeleteFacadeReporter>());
            this.fileSystem.Setup(x => x.Directory.Exists(It.IsAny<string>())).Returns(true);
        }

        [TestCase("Any*.*", new[] { AnyText, AnyImage, AnyPackage })]
        [TestCase("AnyImage.*", new[] { AnyImage })]
        [TestCase("*.nupkg", new[] { AnyPackage, SpecificPackage })]
        [TestCase("*.*", new[] { AnyText, AnyImage, AnyPackage, SpecificPackage })]
        public void Delete_Then_DeleteShouldBeCalledExpectedTimes(string glob, string[] expectedFiles)
        {
            this.fileSystem!
                .Setup(x => x.Directory.EnumerateFiles(AnyPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(Files.Select(x => Path.Combine(AnyPath, x)).ToArray());

            this.testee!.Delete(new DeleteVerb(new List<string>() { glob }, AnyPath));

            var actualExpectedFiles = expectedFiles.Select(x => Path.Combine(AnyPath, x)).ToReadOnly();
            actualExpectedFiles.ForEach(path => this.fileSystem!.File.Verify(x => x.Delete(path), Times.Once));
            Files.Except(actualExpectedFiles).ForEach(path => this.fileSystem!.File.Verify(x => x.Delete(path), Times.Never));
        }

        [TestCase(@"**\Any*.*", new[] { AnyText, AnyImage, AnyPackage, AnySubImage })]
        [TestCase(@"**\AnyImage*.*", new[] { AnyImage, AnySubImage })]
        [TestCase(@"**\*.nupkg", new[] { AnyPackage, AnySubPackage, SpecificPackage })]
        [TestCase(@"**\*.*", new[] { AnyText, AnyImage, AnyPackage, SpecificPackage, AnySubImage, AnySubPackage })]
        public void Delete_When_UsingDirectoryGlobPattern_Then_DeleteShouldBeCalledExpectedTimes(string glob, string[] expectedFiles)
        {
            this.fileSystem!
                .Setup(x => x.Directory.EnumerateFiles(AnyPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(Files.Select(x => Path.Combine(AnyPath, x)).ToArray());

            this.testee!.Delete(new DeleteVerb(new List<string>() { glob }, AnyPath, true));

            var actualExpectedFiles = expectedFiles.Select(x => Path.Combine(AnyPath, x)).ToReadOnly();
            actualExpectedFiles.ForEach(path => this.fileSystem!.File.Verify(x => x.Delete(path), Times.Once));
            Files.Except(actualExpectedFiles).ForEach(path => this.fileSystem!.File.Verify(x => x.Delete(path), Times.Never));
        }

        [Test]
        public void Delete__When_SpecifiedFile_DoesNotExist_Then_DeleteShouldNotBeCalled()
        {
            this.fileSystem!
                .Setup(x => x.Directory.EnumerateFiles(AnyPath, It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(Files.Select(x => Path.Combine(AnyPath, x)).ToArray());

            this.testee!.Delete(new DeleteVerb(new List<string> { "NonExisting.file" }, AnyPath, true));

            this.fileSystem!.File.Verify(x => x.Delete(It.IsAny<string>()), Times.Never);
        }
    }
}