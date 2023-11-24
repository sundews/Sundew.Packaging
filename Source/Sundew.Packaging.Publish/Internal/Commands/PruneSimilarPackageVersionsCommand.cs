// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PruneSimilarPackageVersionsCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using System;
using System.IO;
using System.Text;
using global::NuGet.Versioning;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;

internal class PruneSimilarPackageVersionsCommand
{
    private const string AllFiles = "*.*";
    private const string Nupkg = ".nupkg";
    private const string Snupkg = ".snupkg";
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;

    public PruneSimilarPackageVersionsCommand(IFileSystem fileSystem, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.logger = logger;
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
                && !fileName.EndsWith(version, StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith($"{version}.symbols", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    this.fileSystem.DeleteFile(filePath);
                }
                catch (IOException e)
                {
                    this.logger.LogInfo($"Could not prune: {filePath} due to: {e}");
                }
            }
        }
    }
}