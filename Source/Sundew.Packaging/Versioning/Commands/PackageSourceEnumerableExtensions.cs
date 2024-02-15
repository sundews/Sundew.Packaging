// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageSourceEnumerableExtensions.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using global::NuGet.Configuration;

/// <summary>
/// Extends <see cref="IEnumerable{PackageSource}"/> with methods for finding packages sources.
/// </summary>
public static class PackageSourceEnumerableExtensions
{
    /// <summary>
    /// Finds a <see cref="PackageSource"/> based on its name.
    /// </summary>
    /// <param name="packageSources">The package sources.</param>
    /// <param name="sourceName">The source name.</param>
    /// <returns>The optional package source.</returns>
    public static PackageSource? TryFindSourceByName(this IEnumerable<PackageSource>? packageSources, string sourceName)
    {
        return packageSources?.FirstOrDefault(x => x.Name.Equals(sourceName, StringComparison.InvariantCulture));
    }

    /// <summary>
    /// Finds a <see cref="PackageSource"/> based on its name.
    /// </summary>
    /// <param name="packageSources">The package sources.</param>
    /// <param name="nameOrSource">The name or source.</param>
    /// <returns>The optional package source.</returns>
    public static PackageSource? TryFindSourceByNameOrSource(this IEnumerable<PackageSource>? packageSources, string nameOrSource)
    {
        return packageSources?.FirstOrDefault(x => x.Name.Equals(nameOrSource, StringComparison.InvariantCulture) || x.Source.Equals(nameOrSource, StringComparison.InvariantCulture));
    }
}
