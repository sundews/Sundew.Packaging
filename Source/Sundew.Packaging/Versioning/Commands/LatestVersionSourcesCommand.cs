// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestVersionSourcesCommand.cs" company="Sundews">
// Copyright (c) Sundews. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using global::NuGet.Configuration;
using Sundew.Base;
using Sundew.Base.Collections.Linq;
using Sundew.Packaging.Staging;
using Sundew.Packaging.Versioning.IO;

/// <summary>
/// Command that collects the sources used to determine the latest version.
/// </summary>
/// <seealso cref="Sundew.Packaging.Versioning.Commands.ILatestVersionSourcesCommand" />
public class LatestVersionSourcesCommand : ILatestVersionSourcesCommand
{
    private const string NuGetOrg = @"nuget.org";
    private readonly IFileSystem fileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="LatestVersionSourcesCommand"/> class.
    /// </summary>
    /// <param name="fileSystem">The file system.</param>
    public LatestVersionSourcesCommand(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    /// <summary>
    /// Gets the latest version sources.
    /// </summary>
    /// <param name="latestVersionSourcesText">The latest version sources text.</param>
    /// <param name="selectedSource">The selected source.</param>
    /// <param name="nuGetSettings">The nu get settings.</param>
    /// <param name="addNuGetOrgSource">if set to <c>true</c> [add nu get org source].</param>
    /// <param name="addAllSources">if set to <c>true</c> [add all sources].</param>
    /// <returns>The latest version.</returns>
    public IReadOnlyList<PackageSource> GetLatestVersionSources(
        string? latestVersionSourcesText,
        SelectedStage selectedSource,
        NuGetSettings nuGetSettings,
        bool addNuGetOrgSource,
        bool addAllSources)
    {
        var latestVersionSources = new List<PackageSource>();
        this.TryAddFeedSource(latestVersionSources, new PackageSource(selectedSource.FeedSource));

        if (selectedSource.AdditionalFeedSources != null)
        {
            foreach (var additionalFeedSource in selectedSource.AdditionalFeedSources)
            {
                this.TryAddFeedSource(latestVersionSources, new PackageSource(additionalFeedSource));
            }
        }

        if (addNuGetOrgSource)
        {
            var nuGetOrgSource = nuGetSettings.PackageSources?.FirstOrDefault(x => x.Source == NuGetOrg);
            if (nuGetOrgSource.HasValue)
            {
                latestVersionSources.Add(nuGetOrgSource);
            }
        }

        if (addAllSources)
        {
            foreach (var packageSource in nuGetSettings.PackageSources ?? Enumerable.Empty<PackageSource>())
            {
                this.TryAddFeedSource(latestVersionSources, packageSource);
            }
        }

        if (latestVersionSourcesText == null)
        {
            return latestVersionSources.Distinct().ToList();
        }

        var packageSources = nuGetSettings.PackageSources;
        var sources = latestVersionSourcesText
            .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(
                sourceUriOrName =>
                {
                    var name = sourceUriOrName;
                    var packageSource =
                        packageSources?.TryFindSourceByNameOrSource(name) ?? new PackageSource(sourceUriOrName);
                    return packageSource;
                })
            .WhereNotNull();
        latestVersionSources.AddRange(sources);
        return latestVersionSources.Distinct().ToList();
    }

    private void TryAddFeedSource(List<PackageSource> latestVersionSources, PackageSource packageSource)
    {
        if (!string.IsNullOrEmpty(packageSource.Source) && this.IsRemoteSourceOrDoesLocalSourceExists(packageSource))
        {
            latestVersionSources.Add(packageSource);
        }
    }

    private bool IsRemoteSourceOrDoesLocalSourceExists(PackageSource packageSource)
    {
        var uri = packageSource.TrySourceAsUri;
        return (uri != null && !uri.IsFile) || this.fileSystem.DirectoryExists(packageSource.Source);
    }
}