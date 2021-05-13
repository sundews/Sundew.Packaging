// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LatestVersionSourcesCommand.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::NuGet.Common;
    using global::NuGet.Configuration;
    using Sundew.Packaging.Publish.Internal.IO;

    internal class LatestVersionSourcesCommand : ILatestVersionSourcesCommand
    {
        private const string NuGetOrg = @"nuget.org";
        private readonly IFileSystem fileSystem;

        public LatestVersionSourcesCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IReadOnlyList<string> GetLatestVersionSources(
            string? latestVersionSourcesText,
            SelectedSource selectedSource,
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
                if (!string.IsNullOrEmpty(nuGetOrgSource) && nuGetOrgSource != null)
                {
                    latestVersionSources.Add(nuGetOrgSource);
                }
            }

            if (addAllSources)
            {
                foreach (var item in nuGetSettings.PackageSourcesSection?.Items.OfType<AddItem>() ?? Enumerable.Empty<AddItem>())
                {
                    var sourceUrl = item?.Value;
                    if (!string.IsNullOrEmpty(sourceUrl) && sourceUrl != null)
                    {
                        latestVersionSources.Add(sourceUrl);
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
            return !UriUtility.TryCreateSourceUri(sourceUri, UriKind.Absolute).IsFile || this.fileSystem.DirectoryExists(sourceUri);
        }
    }
}