// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPackageToLocalSourceCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Commands
{
    using System.IO;
    using Sundew.Build.Publish.Internal.IO;

    /// <summary>Adds a NuGet package to an offline feed.</summary>
    /// <seealso cref="ICopyPackageToLocalSourceCommand" />
    public class CopyPackageToLocalSourceCommand : ICopyPackageToLocalSourceCommand
    {
        private readonly IFileSystem fileSystem;

        /// <summary>Initializes a new instance of the <see cref="CopyPackageToLocalSourceCommand"/> class.</summary>
        public CopyPackageToLocalSourceCommand()
        : this(new FileSystem())
        {
        }

        internal CopyPackageToLocalSourceCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>Adds the asynchronous.</summary>
        /// <param name="packageId">The package id.</param>
        /// <param name="packagePath">The package path.</param>
        /// <param name="source">The source.</param>
        /// <param name="skipDuplicate">Skips duplicate packages.</param>
        /// <param name="commandLogger">The command logger.</param>
        /// <returns>The destination path.</returns>
        public string Add(string packageId, string packagePath, string source, bool skipDuplicate, ICommandLogger commandLogger)
        {
            source = Path.Combine(source, packageId);
            if (!this.fileSystem.DirectoryExists(source))
            {
                this.fileSystem.CreateDirectory(source);
            }

            var destinationPath = Path.Combine(source, Path.GetFileName(packagePath));
            this.fileSystem.Copy(packagePath, destinationPath, !skipDuplicate);
            commandLogger.LogInfo($"Successfully copied package to: {destinationPath}");
            return destinationPath;
        }
    }
}