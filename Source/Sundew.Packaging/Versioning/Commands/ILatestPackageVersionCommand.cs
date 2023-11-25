// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILatestPackageVersionCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System.Collections.Generic;
using System.Threading.Tasks;
using global::NuGet.Versioning;

/// <summary>
/// Interface for implementing a command that gets the latest version of a package.
/// </summary>
public interface ILatestPackageVersionCommand
{
    /// <summary>
    /// Gets the latest major minor version.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="sources">The sources.</param>
    /// <param name="nuGetVersion">The nu get version.</param>
    /// <param name="includePatchInMatch">if set to <c>true</c> [include patch in match].</param>
    /// <param name="allowPrerelease">if set to <c>true</c> [allow prerelease].</param>
    /// <returns>An async task with the latest version.</returns>
    Task<NuGetVersion?> GetLatestMajorMinorVersion(
        string packageId,
        IReadOnlyList<string> sources,
        NuGetVersion nuGetVersion,
        bool includePatchInMatch,
        bool allowPrerelease);
}