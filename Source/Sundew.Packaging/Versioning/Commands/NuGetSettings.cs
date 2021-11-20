// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSettings.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using global::NuGet.Configuration;

/// <summary>Contains information about local source and local symbols source.</summary>
public readonly struct NuGetSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NuGetSettings" /> struct.
    /// </summary>
    /// <param name="localSourcePath">The localSourcePath.</param>
    /// <param name="defaultSettings">The default settings.</param>
    /// <param name="packageSourcesSection">The package sources section.</param>
    public NuGetSettings(string localSourcePath, ISettings defaultSettings, SettingSection? packageSourcesSection)
    {
        this.LocalSourcePath = localSourcePath;
        this.DefaultSettings = defaultSettings;
        this.PackageSourcesSection = packageSourcesSection;
    }

    /// <summary>Gets the localSourcePath.</summary>
    /// <value>The localSourcePath.</value>
    public string LocalSourcePath { get; }

    /// <summary>Gets the default settings.</summary>
    /// <value>The default settings.</value>
    public ISettings DefaultSettings { get; }

    /// <summary>
    /// Gets the package sources section.
    /// </summary>
    /// <value>
    /// The package sources section.
    /// </value>
    public SettingSection? PackageSourcesSection { get; }
}