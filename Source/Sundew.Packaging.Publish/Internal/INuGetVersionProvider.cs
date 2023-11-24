// --------------------------------------------------------------------------------------------------------------------
// <copyright file="INuGetVersionProvider.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal;

/// <summary>
/// Interface for implementing the persist NuGet version command.
/// </summary>
public interface INuGetVersionProvider
{
    /// <summary>
    /// Saves the specified version.
    /// </summary>
    /// <param name="versionFilePath">The output path.</param>
    /// <param name="referencedPackageVersionFilePath">The referenced package version file path.</param>
    /// <param name="version">The version.</param>
    void Save(string versionFilePath, string referencedPackageVersionFilePath, string version);

    /// <summary>
    /// Reads the specified version file path.
    /// </summary>
    /// <param name="versionFilePath">The version file path.</param>
    /// <param name="version">The version.</param>
    /// <returns>
    /// The persisted NuGet version.
    /// </returns>
    bool Read(string versionFilePath, out string? version);
}