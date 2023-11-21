// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestVersionSourcesCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using global::NuGet.Common;
using global::NuGet.Configuration;
using Sundew.Base.Text;
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
    public IReadOnlyList<string> GetLatestVersionSources(
        string? latestVersionSourcesText,
        SelectedStage selectedSource,
        NuGetSettings nuGetSettings,
        bool addNuGetOrgSource,
        bool addAllSources)
    {
        var latestVersionSources = new List<string>();
        this.TryAddFeedSource(latestVersionSources, selectedSource.FeedSource);

        if (selectedSource.AdditionalFeedSources != null)
        {
            foreach (var additionalFeedSource in selectedSource.AdditionalFeedSources)
            {
                this.TryAddFeedSource(latestVersionSources, additionalFeedSource);
            }
        }

        if (addNuGetOrgSource)
        {
            var nuGetOrgSource = nuGetSettings.PackageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == NuGetOrg)?.Value;
            if (!nuGetOrgSource.IsNullOrEmpty())
            {
                latestVersionSources.Add(nuGetOrgSource);
            }
        }

        if (addAllSources)
        {
            foreach (var item in nuGetSettings.PackageSourcesSection?.Items.OfType<AddItem>() ?? Enumerable.Empty<AddItem>())
            {
                var sourceUrl = item?.Value;
                if (!sourceUrl.IsNullOrEmpty())
                {
                    this.TryAddFeedSource(latestVersionSources, sourceUrl);
                }
            }
        }

        if (latestVersionSourcesText == null)
        {
            return latestVersionSources.Distinct().ToList();
        }

        var packageSourcesSection = nuGetSettings.PackageSourcesSection;
        var sources = latestVersionSourcesText.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(
                sourceUriOrName =>
                {
                    var name = sourceUriOrName;
                    var sourceUri =
                        packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == name)?.Value ??
                        sourceUriOrName;
                    var uri = UriUtility.TryCreateSourceUri(sourceUri, UriKind.Absolute);
                    return uri?.OriginalString ?? string.Empty;
                })
            .Where(x => !string.IsNullOrEmpty(x));
        latestVersionSources.AddRange(sources);
        return latestVersionSources.Distinct().ToList();
    }

    private void TryAddFeedSource(List<string> latestVersionSources, string sourceUri)
    {
        if (!string.IsNullOrEmpty(sourceUri) && this.IsRemoteSourceOrDoesLocalSourceExists(sourceUri))
        {
            latestVersionSources.Add(sourceUri);
        }
    }

    private bool IsRemoteSourceOrDoesLocalSourceExists(string sourceUri)
    {
        var uri = UriUtility.TryCreateSourceUri(sourceUri, UriKind.Absolute);
        return (uri != null && !uri.IsFile) || this.fileSystem.DirectoryExists(sourceUri);
    }
}