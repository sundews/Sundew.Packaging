// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    internal record Source
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
        private static readonly Regex SourceRegex = new($@"(?:(?<StageRegex>(?:[^#\s])+)\s*(?:#\s*(?<StageName>\w*))?\s*(?:\$(?<PrereleaseFormat>\S+))?\s*=\>\s*)(?:(?:(?<ApiKey>[^@\|\s]*)@)?(?<Uri>[^\|\s|\{{]+))(?:\s*\{{\s*(?<FeedUri>[^\|\s]+)\s*\}}\s*)?(?:\s*\|\s*(?:(?<SymbolsApiKey>[^@\|\s]*)@)?(?<SymbolsUri>[^\|\s]+))?");

        public Source(
            Regex? stageRegex,
            string pushSource,
            string? apiKey,
            string? symbolsPushSource,
            string? symbolsApiKey,
            string stage,
            bool isStableRelease,
            string feedSource,
            string? prereleaseFormat,
            IReadOnlyList<string>? additionalFeedSources,
            bool isEnabled,
            bool isFallback = false)
        {
            this.StageRegex = stageRegex;
            this.PushSource = pushSource;
            this.ApiKey = apiKey;
            this.SymbolsPushSource = symbolsPushSource;
            this.SymbolsApiKey = symbolsApiKey;
            this.Stage = stage;
            this.IsStableRelease = isStableRelease;
            this.FeedSource = feedSource;
            this.PrereleaseFormat = prereleaseFormat;
            this.AdditionalFeedSources = additionalFeedSources;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
        }

        public Regex? StageRegex { get; }

        public string PushSource { get; }

        public string FeedSource { get; }

        public string? PrereleaseFormat { get; }

        public IReadOnlyList<string>? AdditionalFeedSources { get; }

        public string? ApiKey { get; }

        public string? SymbolsPushSource { get; }

        public string? SymbolsApiKey { get; }

        public string Stage { get; }

        public bool IsStableRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source? Parse(
            string? sourceText,
            string defaultStage,
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

            var match = SourceRegex.Match(sourceText);
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

                var stage = defaultStage;
                if (stageNameGroup.Success)
                {
                    stage = stageNameGroup.Value;
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

                return new Source(name, sourceUri, apiKey, symbolsUri, symbolsApiKey, stage, isStableRelease, feedSource, prereleaseFormat, feedSources, isSourcePublishEnabled);
            }

            return default;
        }
    }
}