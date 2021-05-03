// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::NuGet.Configuration;

    internal static class SourceSelector
    {
        internal const string DefaultPushSourceText = "defaultPushSource";
        internal const string DefaultLocalPackageStage = "local";
        internal const string DefaultDevelopmentPackageStage = "dev";
        internal const string DefaultIntegrationPackageStage = "ci";
        internal const string DefaultProductionPackageStage = "prod";
        private const string DefaultSourceNameText = "default";
        private const string DefaultStableSourceNameText = "default-stable";
        private const string LocalStableSourceNameText = "local-stable";
        private const string NoDefaultPushSourceHasBeenConfiguredText = "No default push source has been configured.";
        private const string PrefixGroupName = "Prefix";
        private const string PostfixGroupName = "Postfix";

        public static SelectedSource SelectSource(
            string? sourceName,
            string? productionSource,
            string? integrationSource,
            string? developmentSource,
            string localSource,
            string? fallbackPrereleaseFormat,
            string? fallbackApiKey,
            string? fallbackSymbolsApiKey,
            string? localPackageStage,
            string? prereleasePrefix,
            string? prereleasePostfix,
            ISettings defaultSettings,
            bool allowLocalSource,
            bool isSourcePublishEnabled)
        {
            if (sourceName != null && !string.IsNullOrEmpty(sourceName))
            {
                if (sourceName.StartsWith(DefaultSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    var defaultSource = defaultSettings.GetSection(Source.ConfigText)?.Items.OfType<AddItem>()
                        .FirstOrDefault(x =>
                            x.Key.Equals(DefaultPushSourceText, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (defaultSource == null)
                    {
                        throw new InvalidOperationException(NoDefaultPushSourceHasBeenConfiguredText);
                    }

                    if (sourceName.Equals(DefaultStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new SelectedSource(new Source(default, defaultSource, default, default, default, DefaultProductionPackageStage, true, defaultSource, fallbackPrereleaseFormat, Array.Empty<string>(), isSourcePublishEnabled), prereleasePrefix, prereleasePostfix);
                    }

                    return new SelectedSource(new Source(default, defaultSource, default, default, default, string.IsNullOrEmpty(localPackageStage) ? DefaultLocalPackageStage : localPackageStage!, false, defaultSource, fallbackPrereleaseFormat, Array.Empty<string>(), isSourcePublishEnabled, true), prereleasePrefix, prereleasePostfix);
                }

                if (sourceName.Equals(LocalStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SelectedSource(new Source(default, localSource, default, default, default, DefaultProductionPackageStage, true, localSource, fallbackPrereleaseFormat, Array.Empty<string>(), isSourcePublishEnabled), prereleasePrefix, prereleasePostfix);
                }

                var production = Source.Parse(productionSource, DefaultProductionPackageStage, true, null, fallbackApiKey, fallbackSymbolsApiKey, null, isSourcePublishEnabled);
                var integrationFeedSources = new List<string>();
                TryAddFeedSource(integrationFeedSources, production);

                var integration = Source.Parse(integrationSource, DefaultIntegrationPackageStage, false, fallbackPrereleaseFormat, fallbackApiKey, fallbackSymbolsApiKey, integrationFeedSources, isSourcePublishEnabled);
                var developmentFeedSources = integrationFeedSources.ToList();
                TryAddFeedSource(developmentFeedSources, integration);

                var development = Source.Parse(developmentSource, DefaultDevelopmentPackageStage, false, fallbackPrereleaseFormat, fallbackApiKey, fallbackSymbolsApiKey, developmentFeedSources, isSourcePublishEnabled);
                var sources = new[] { production, integration, development };

                var (source, match) = sources.Select(x => (source: x, match: x?.StageRegex?.Match(sourceName))).FirstOrDefault(x => x.match?.Success ?? false);
                if (source != null)
                {
                    var prefixGroup = match?.Groups[PrefixGroupName];
                    var postfixGroup = match?.Groups[PostfixGroupName];
                    return new SelectedSource(
                        source,
                        prefixGroup?.Success ?? false ? prefixGroup.Value : prereleasePrefix ?? string.Empty,
                        postfixGroup?.Success ?? false ? postfixGroup.Value : prereleasePostfix ?? string.Empty);
                }
            }

            return new SelectedSource(
                new Source(
                    null,
                    localSource,
                    default,
                    default,
                    default,
                    string.IsNullOrEmpty(localPackageStage) ? DefaultLocalPackageStage : localPackageStage!,
                    false,
                    localSource,
                    fallbackPrereleaseFormat,
                    Array.Empty<string>(),
                    allowLocalSource,
                    allowLocalSource && isSourcePublishEnabled),
                prereleasePrefix,
                prereleasePostfix);
        }

        private static void TryAddFeedSource(List<string> feedSources, Source? source)
        {
            if (source != null && !string.IsNullOrEmpty(source.FeedSource))
            {
                feedSources.Add(source.FeedSource);
            }
        }
    }
}