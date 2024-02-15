// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageExistsCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System.Collections.Generic;
using System.Threading.Tasks;
using global::NuGet.Configuration;
using global::NuGet.Versioning;

/// <summary>
/// Interface for implementing a command that tests if a package exists.
/// </summary>
public interface IPackageExistsCommand
{
    /// <summary>
    /// Checks if the specified package exists.
    /// </summary>
    /// <param name="packageId">The package identifier.</param>
    /// <param name="semanticVersion">The semantic version.</param>
    /// <param name="packageSources">The package sources.</param>
    /// <returns>An async task with a value indicating whether the package exists.</returns>
    Task<bool> ExistsAsync(string packageId, SemanticVersion semanticVersion, IReadOnlyList<PackageSource> packageSources);
}