// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetVersionProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System.IO;
    using Sundew.Packaging.Versioning.IO;
    using Sundew.Packaging.Versioning.Logging;

    /// <summary>
    /// Stores the NuGet version in a file at the output path.
    /// </summary>
    internal class NuGetVersionProvider : INuGetVersionProvider
    {
        private readonly IFileSystem fileSystem;
        private readonly ILogger logger;

        internal NuGetVersionProvider(IFileSystem fileSystem, ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.logger = logger;
        }

        /// <summary>
        /// Saves the specified version.
        /// </summary>
        /// <param name="versionFilePath">The output path.</param>
        /// <param name="referencedPackageVersionFilePath">The referenced package version file path.</param>
        /// <param name="version">The version.</param>
        public void Save(string versionFilePath, string referencedPackageVersionFilePath, string version)
        {
            var directoryPath = Path.GetDirectoryName(versionFilePath);
            if (!this.fileSystem.DirectoryExists(directoryPath))
            {
                this.fileSystem.CreateDirectory(directoryPath);
            }

            this.fileSystem.WriteAllText(versionFilePath, version);
            this.fileSystem.Copy(versionFilePath, referencedPackageVersionFilePath, true);
            this.logger.LogInfo($"Wrote version: {version} to {versionFilePath}");
        }

        /// <summary>
        /// Reads the specified version file path.
        /// </summary>
        /// <param name="versionFilePath">The version file path.</param>
        /// <param name="version">The version.</param>
        /// <returns>
        /// The persisted NuGet version.
        /// </returns>
        public bool Read(string versionFilePath, out string? version)
        {
            if (this.fileSystem.FileExists(versionFilePath))
            {
                version = this.fileSystem.ReadAllText(versionFilePath);
                this.logger.LogInfo($"Using cached version: {version}");
                return true;
            }

            version = null;
            return false;
        }
    }
}