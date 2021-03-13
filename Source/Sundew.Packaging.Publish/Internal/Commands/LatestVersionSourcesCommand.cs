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
        private readonly IFileSystem fileSystem;

        public LatestVersionSourcesCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IReadOnlyList<string> GetLatestVersionSources(string? latestVersionSourcesText, SelectedSource selectedSource, NuGetSettings nuGetSettings, bool addDefaultPushSource)
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

            var defaultPushSource = new PackageSourceProvider(nuGetSettings.DefaultSettings).DefaultPushSource;
            if (addDefaultPushSource && !string.IsNullOrEmpty(defaultPushSource) && this.IsRemoteSourceOrDoesLocalSourceExists(defaultPushSource))
            {
                latestVersionSources.Add(defaultPushSource);
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