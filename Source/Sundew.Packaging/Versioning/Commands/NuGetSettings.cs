// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettings.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System.Collections.Generic;
using global::NuGet.Configuration;

/// <summary>Contains information about local source and local symbols source.</summary>
public readonly struct NuGetSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetSettings" /> struct.
    /// </summary>
    /// <param name="localPackageSource">The localPackageSource.</param>
    /// <param name="defaultSettings">The default settings.</param>
    /// <param name="packageSources">The package sources section.</param>
    public NuGetSettings(PackageSource localPackageSource, ISettings defaultSettings, IReadOnlyList<PackageSource>? packageSources)
    {
        this.LocalPackageSource = localPackageSource;
        this.DefaultSettings = defaultSettings;
        this.PackageSources = packageSources;
    }

    /// <summary>Gets the localPackageSource.</summary>
    /// <value>The localPackageSource.</value>
    public PackageSource LocalPackageSource { get; }

    /// <summary>Gets the default settings.</summary>
    /// <value>The default settings.</value>
    public ISettings DefaultSettings { get; }

    /// <summary>
    /// Gets the package sources.
    /// </summary>
    /// <value>
    /// The package sources.
    /// </value>
    public IReadOnlyList<PackageSource>? PackageSources { get; }
}