// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PruneSimilarPackageVersionsCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System;
    using System.IO;
    using System.Text;
    using global::NuGet.Versioning;
    using Sundew.Packaging.Publish.Internal.IO;

    internal class PruneSimilarPackageVersionsCommand
    {
        private const string AllFiles = "*.*";
        private const string Nupkg = ".nupkg";
        private const string Snupkg = ".snupkg";
        private readonly IFileSystem fileSystem;

        public PruneSimilarPackageVersionsCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void Prune(string packagePath, string packageId, string version)
        {
            var nuGetVersion = NuGetVersion.Parse(version);
            var stringBuilder = new StringBuilder(packageId).Append('.').Append(nuGetVersion.Major).Append('.').Append(nuGetVersion.Minor).Append('.').Append(nuGetVersion.Patch);
            if (nuGetVersion.IsLegacyVersion)
            {
                stringBuilder.Append('.').Append(nuGetVersion.Revision);
            }

            var similarVersion = stringBuilder.ToString();
            foreach (var filePath in this.fileSystem.EnumerableFiles(Path.GetDirectoryName(packagePath), AllFiles, SearchOption.TopDirectoryOnly))
            {
                var extension = Path.GetExtension(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if ((extension.Equals(Nupkg, StringComparison.OrdinalIgnoreCase) || extension.Equals(Snupkg, StringComparison.OrdinalIgnoreCase))
                    && fileName.StartsWith(similarVersion, StringComparison.OrdinalIgnoreCase)
                    && !fileName.EndsWith(version, StringComparison.OrdinalIgnoreCase))
                {
                    this.fileSystem.DeleteFile(filePath);
                }
            }
        }
    }
}