﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyPackageToLocalSourceCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands;

using System.IO;
using Sundew.Packaging.Versioning.IO;
using Sundew.Packaging.Versioning.Logging;

/// <summary>Adds a NuGet package to an offline feed.</summary>
/// <seealso cref="ICopyPackageToLocalSourceCommand" />
public class CopyPackageToLocalSourceCommand : ICopyPackageToLocalSourceCommand
{
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyPackageToLocalSourceCommand"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    /// <param name="logger">The logger.</param>
    internal CopyPackageToLocalSourceCommand(IFileSystem fileSystem, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.logger = logger;
    }

    /// <summary>
    /// Adds the asynchronous.
    /// </summary>
    /// <param name="packageId">The package id.</param>
    /// <param name="packagePath">The package path.</param>
    /// <param name="source">The source.</param>
    /// <param name="skipDuplicate">Skips duplicate packages.</param>
    /// <returns>
    /// The destination path.
    /// </returns>
    public string Add(string packageId, string packagePath, string source, bool skipDuplicate)
    {
        source = Path.Combine(source, packageId);
        if (!this.fileSystem.DirectoryExists(source))
        {
            this.fileSystem.CreateDirectory(source);
        }

        var destinationPath = Path.Combine(source, Path.GetFileName(packagePath));
        this.fileSystem.Copy(packagePath, destinationPath, !skipDuplicate);
        this.logger.LogImportant($"Successfully copied package to: {destinationPath}");
        return destinationPath;
    }
}