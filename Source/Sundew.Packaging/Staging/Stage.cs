// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Stage.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Staging
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Defines how packages should be versioned for a source.
    /// </summary>
    public record Stage
    {
        internal const string ConfigText = "config";
        private const string StageRegexText = "StageRegex";
        private const string StageNameText = "StageName";
        private const string UriText = "Uri";
        private const string ApiKeyText = "ApiKey";
        private const string SymbolsApiKeyText = "SymbolsApiKey";
        private const string SymbolsUriText = "SymbolsUri";
        private const string FeedUriText = "FeedUri";
        private const string PrereleasFormatText = "PrereleaseFormat";
        private const string PropertyName = "PropertyName";
        private const string PropertyValue = "PropertyValue";
        private static readonly Regex StageMatcherRegex = new($@"(?<StageRegex>.+?)(?=\s*=\>)\s*=\>\s*(?:#\s*(?<StageName>\w*))?\s*(?:\&(?<PrereleaseFormat>\S+))?\s+(?:(?:(?<ApiKey>[^@\|\s]*)@)?(?<Uri>[^\|\s|\{{]+))(?:\s*\{{\s*(?<FeedUri>[^\|\s]+)\s*\}}\s*)?(?:\s*\|\s*(?:(?<SymbolsApiKey>[^@\|\s]*)@)?(?<SymbolsUri>[^\|\s]+))?(?:\|(?:\|(?<PropertyName>[^\|\=]+)\=(?<PropertyValue>[^\|\=]+))+)?");

        /// <summary>
        /// Initializes a new instance of the <see cref="Staging.Stage" /> class.
        /// </summary>
        /// <param name="stageRegex">The stage regex.</param>
        /// <param name="pushSource">The push source.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="symbolsPushSource">The symbols push source.</param>
        /// <param name="symbolsApiKey">The symbols API key.</param>
        /// <param name="stage">The stage.</param>
        /// <param name="versionStage">The version stage.</param>
        /// <param name="isStableRelease">if set to <c>true</c> [is stable release].</param>
        /// <param name="feedSource">The feed source.</param>
        /// <param name="prereleaseFormat">The prerelease format.</param>
        /// <param name="additionalFeedSources">The additional feed sources.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
        /// <param name="isFallback">if set to <c>true</c> [is fallback].</param>
        public Stage(
            Regex? stageRegex,
            string pushSource,
            string? apiKey,
            string? symbolsPushSource,
            string? symbolsApiKey,
            string stage,
            string versionStage,
            bool isStableRelease,
            string feedSource,
            string? prereleaseFormat,
            IReadOnlyList<string>? additionalFeedSources,
            IReadOnlyDictionary<string, string>? properties,
            bool isEnabled,
            bool isFallback = false)
        {
            this.StageRegex = stageRegex;
            this.PushSource = pushSource;
            this.ApiKey = apiKey;
            this.SymbolsPushSource = symbolsPushSource;
            this.SymbolsApiKey = symbolsApiKey;
            this.StageName = stage;
            this.VersionStageName = versionStage;
            this.IsStableRelease = isStableRelease;
            this.FeedSource = feedSource;
            this.PrereleaseFormat = prereleaseFormat;
            this.AdditionalFeedSources = additionalFeedSources;
            this.Properties = properties;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Gets the stage regex.
        /// </summary>
        /// <value>
        /// The stage regex.
        /// </value>
        public Regex? StageRegex { get; }

        /// <summary>
        /// Gets the push source.
        /// </summary>
        /// <value>
        /// The push source.
        /// </value>
        public string PushSource { get; }

        /// <summary>
        /// Gets the feed source.
        /// </summary>
        /// <value>
        /// The feed source.
        /// </value>
        public string FeedSource { get; }

        /// <summary>
        /// Gets the prerelease format.
        /// </summary>
        /// <value>
        /// The prerelease format.
        /// </value>
        public string? PrereleaseFormat { get; }

        /// <summary>
        /// Gets the additional feed sources.
        /// </summary>
        /// <value>
        /// The additional feed sources.
        /// </value>
        public IReadOnlyList<string>? AdditionalFeedSources { get; }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public string? ApiKey { get; }

        /// <summary>
        /// Gets the symbols push source.
        /// </summary>
        /// <value>
        /// The symbols push source.
        /// </value>
        public string? SymbolsPushSource { get; }

        /// <summary>
        /// Gets the symbols API key.
        /// </summary>
        /// <value>
        /// The symbols API key.
        /// </value>
        public string? SymbolsApiKey { get; }

        /// <summary>
        /// Gets the stage name.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public string StageName { get; }

        /// <summary>
        /// Gets the version stage name.
        /// </summary>
        /// <value>
        /// The stage.
        /// </value>
        public string VersionStageName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is stable release.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is stable release; otherwise, <c>false</c>.
        /// </value>
        public bool IsStableRelease { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is fallback.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fallback; otherwise, <c>false</c>.
        /// </value>
        public bool IsFallback { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IReadOnlyDictionary<string, string>? Properties { get; }

        /// <summary>
        /// Parses the specified source text.
        /// </summary>
        /// <param name="sourceText">The source text.</param>
        /// <param name="defaultStage">The default stage.</param>
        /// <param name="defaultVersionStage">The default version stage.</param>
        /// <param name="isStableRelease">if set to <c>true</c> [is stable release].</param>
        /// <param name="fallbackPrereleaseFormat">The fallback prerelease format.</param>
        /// <param name="fallbackApiKey">The fallback API key.</param>
        /// <param name="fallbackSymbolsApiKey">The fallback symbols API key.</param>
        /// <param name="feedSources">The feed sources.</param>
        /// <param name="isSourcePublishEnabled">if set to <c>true</c> [is source publish enabled].</param>
        /// <returns>
        /// The source.
        /// </returns>
        public static Stage? Parse(
            string? sourceText,
            string defaultStage,
            string defaultVersionStage,
            bool isStableRelease,
            string? fallbackPrereleaseFormat,
            string? fallbackApiKey,
            string? fallbackSymbolsApiKey,
            IReadOnlyList<string>? feedSources,
            bool isSourcePublishEnabled)
        {
            if (sourceText == null || string.IsNullOrEmpty(sourceText))
            {
                return default;
            }

            var match = StageMatcherRegex.Match(sourceText);
            if (match.Success)
            {
                var name = new Regex(match.Groups[StageRegexText].Value);
                var stageNameGroup = match.Groups[StageNameText];
                var apiKeyGroup = match.Groups[ApiKeyText];
                var apiKey = apiKeyGroup.Success ? apiKeyGroup.Value : fallbackApiKey;
                if (apiKey == string.Empty)
                {
                    apiKey = null;
                }

                var sourceUri = match.Groups[UriText].Value;
                var symbolsUriGroup = match.Groups[SymbolsUriText];
                string? symbolsUri = null;
                string? symbolsApiKey = null;
                if (symbolsUriGroup.Success)
                {
                    symbolsUri = symbolsUriGroup.Value;
                    var symbolsApiKeyGroup = match.Groups[SymbolsApiKeyText];
                    symbolsApiKey = symbolsApiKeyGroup.Success ? symbolsApiKeyGroup.Value : fallbackSymbolsApiKey ?? apiKey;
                    if (symbolsApiKey == string.Empty)
                    {
                        symbolsApiKey = null;
                    }
                }

                var versionStage = defaultVersionStage;
                if (stageNameGroup.Success)
                {
                    versionStage = stageNameGroup.Value;
                }

                var feedSource = sourceUri;
                var feedUriGroup = match.Groups[FeedUriText];
                if (feedUriGroup.Success)
                {
                    feedSource = feedUriGroup.Value;
                }

                var prereleaseFormat = fallbackPrereleaseFormat;
                var prereleaseFormatGroup = match.Groups[PrereleasFormatText];
                if (prereleaseFormatGroup.Success)
                {
                    prereleaseFormat = prereleaseFormatGroup.Value;
                }

                Dictionary<string, string> properties = new Dictionary<string, string>();
                FillPropertiesFromMatch(properties, match);

                return new Stage(name, sourceUri, apiKey, symbolsUri, symbolsApiKey, defaultStage, versionStage, isStableRelease, feedSource, prereleaseFormat, feedSources, properties, isSourcePublishEnabled);
            }

            return default;
        }

        internal static void FillPropertiesFromMatch(Dictionary<string, string> properties, Match match)
        {
            var propertyNameGroup = match.Groups[PropertyName];
            var propertyValueGroup = match.Groups[PropertyValue];
            if (propertyNameGroup.Success && propertyValueGroup.Success)
            {
                foreach (var pair in propertyNameGroup.Captures.Cast<Capture>().Select(x => x.Value).Zip(propertyValueGroup.Captures.Cast<Capture>().Select(x => x.Value), (x, y) => (x, y)))
                {
                    properties.Add(pair.x, pair.y);
                }
            }
        }
    }
}