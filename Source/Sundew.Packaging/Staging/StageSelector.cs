// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StageSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Staging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::NuGet.Configuration;

    /// <summary>
    /// Selects a source based on the given source name.
    /// </summary>
    public static class StageSelector
    {
        internal const string DefaultPushSourceText = "defaultPushSource";
        internal const string DefaultLocalPackageStage = "local";
        internal const string DefaultDevelopmentPackageStage = "dev";
        internal const string DefaultIntegrationPackageStage = "ci";
        internal const string DefaultProductionPackageStage = "prod";
        internal const string DefaultDevelopmentStage = "development";
        internal const string DefaultIntegrationStage = "integration";
        internal const string DefaultProductionStage = "production";
        private const string DefaultStageNameText = "default";
        private const string DefaultStableSourceNameText = "default-stable";
        private const string LocalStableSourceNameText = "local-stable";
        private const string NoDefaultPushSourceHasBeenConfiguredText = "No default push source has been configured.";
        private const string PrefixGroupName = "Prefix";
        private const string PostfixGroupName = "Postfix";

        /// <summary>
        /// Selects the source.
        /// </summary>
        /// <param name="stage">Name of the source.</param>
        /// <param name="production">The production source.</param>
        /// <param name="integration">The integration source.</param>
        /// <param name="development">The development source.</param>
        /// <param name="localSource">The local source.</param>
        /// <param name="fallbackPrereleaseFormat">The fallback prerelease format.</param>
        /// <param name="fallbackApiKey">The fallback API key.</param>
        /// <param name="fallbackSymbolsApiKey">The fallback symbols API key.</param>
        /// <param name="localPackageStage">The local package stage.</param>
        /// <param name="prereleasePrefix">The prerelease prefix.</param>
        /// <param name="prereleasePostfix">The prerelease postfix.</param>
        /// <param name="defaultSettings">The default settings.</param>
        /// <param name="allowLocalSource">if set to <c>true</c> [allow local source].</param>
        /// <param name="isSourcePublishEnabled">if set to <c>true</c> [is source publish enabled].</param>
        /// <returns>The selected source.</returns>
        /// <exception cref="InvalidOperationException">Thrown if not default push source has been configured.</exception>
        public static SelectedStage Select(
            string? stage,
            string? production,
            string? integration,
            string? development,
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
            if (stage != null && !string.IsNullOrEmpty(stage))
            {
                if (stage.StartsWith(DefaultStageNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    var defaultSource = defaultSettings.GetSection(Stage.ConfigText)?.Items.OfType<AddItem>()
                        .FirstOrDefault(x =>
                            x.Key.Equals(DefaultPushSourceText, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (defaultSource == null)
                    {
                        throw new InvalidOperationException(NoDefaultPushSourceHasBeenConfiguredText);
                    }

                    if (stage.Equals(DefaultStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new SelectedStage(new Stage(default, defaultSource, default, default, default, DefaultProductionStage, DefaultProductionPackageStage, true, defaultSource, fallbackPrereleaseFormat, Array.Empty<string>(), null, isSourcePublishEnabled), prereleasePrefix, prereleasePostfix);
                    }

                    return new SelectedStage(new Stage(default, defaultSource, default, default, default, DefaultLocalPackageStage, string.IsNullOrEmpty(localPackageStage) ? DefaultLocalPackageStage : localPackageStage!, false, defaultSource, fallbackPrereleaseFormat, Array.Empty<string>(), null, isSourcePublishEnabled, true), prereleasePrefix, prereleasePostfix);
                }

                if (stage.Equals(LocalStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SelectedStage(new Stage(default, localSource, default, default, default, DefaultProductionStage, DefaultProductionPackageStage, true, localSource, fallbackPrereleaseFormat, Array.Empty<string>(), null, isSourcePublishEnabled), prereleasePrefix, prereleasePostfix);
                }

                var productionStage = Stage.Parse(production, DefaultProductionStage, DefaultProductionPackageStage, true, null, fallbackApiKey, fallbackSymbolsApiKey, null, isSourcePublishEnabled);
                var integrationFeedSources = new List<string>();
                TryAddFeedSource(integrationFeedSources, productionStage);

                var integrationStage = Stage.Parse(integration, DefaultIntegrationStage, DefaultIntegrationPackageStage, false, fallbackPrereleaseFormat, fallbackApiKey, fallbackSymbolsApiKey, integrationFeedSources, isSourcePublishEnabled);
                var developmentFeedSources = integrationFeedSources.ToList();
                TryAddFeedSource(developmentFeedSources, integrationStage);

                var developmentStage = Stage.Parse(development, DefaultDevelopmentStage, DefaultDevelopmentPackageStage, false, fallbackPrereleaseFormat, fallbackApiKey, fallbackSymbolsApiKey, developmentFeedSources, isSourcePublishEnabled);
                var sources = new[] { productionStage, integrationStage, developmentStage };

                var (source, match) = sources.Select(x => (source: x, match: x?.StageRegex?.Match(stage))).FirstOrDefault(x => x.match?.Success ?? false);
                if (source != null)
                {
                    var prefixGroup = match?.Groups[PrefixGroupName];
                    var postfixGroup = match?.Groups[PostfixGroupName];
                    return new SelectedStage(
                        source,
                        prefixGroup?.Success ?? false ? prefixGroup.Value : prereleasePrefix ?? string.Empty,
                        postfixGroup?.Success ?? false ? postfixGroup.Value : prereleasePostfix ?? string.Empty);
                }
            }

            return new SelectedStage(
                new Stage(
                    null,
                    localSource,
                    default,
                    default,
                    default,
                    DefaultLocalPackageStage,
                    string.IsNullOrEmpty(localPackageStage) ? DefaultLocalPackageStage : localPackageStage!,
                    false,
                    localSource,
                    fallbackPrereleaseFormat,
                    Array.Empty<string>(),
                    null,
                    allowLocalSource,
                    allowLocalSource && isSourcePublishEnabled),
                prereleasePrefix,
                prereleasePostfix);
        }

        private static void TryAddFeedSource(List<string> feedSources, Stage? stage)
        {
            if (stage != null && !string.IsNullOrEmpty(stage.FeedSource))
            {
                feedSources.Add(stage.FeedSource);
            }
        }
    }
}