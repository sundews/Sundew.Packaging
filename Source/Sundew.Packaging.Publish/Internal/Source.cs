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
            string uri,
            string? apiKey,
            string? symbolsUri,
            string? symbolsApiKey,
            string stage,
            bool isStableRelease,
            string feedSource,
            string? prereleaseFormat,
            IReadOnlyList<string>? additionalFeedSources,
            bool isFallback = false,
            bool isEnabled = true)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.ApiKey = apiKey;
            this.SymbolsUri = symbolsUri;
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

        public string Uri { get; }

        public string FeedSource { get; }

        public string? PrereleaseFormat { get; }

        public IReadOnlyList<string>? AdditionalFeedSources { get; }

        public string? ApiKey { get; }

        public string? SymbolsUri { get; }

        public string? SymbolsApiKey { get; }

        public string Stage { get; }

        public bool IsStableRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source? Parse(string? sourceText, string defaultStage, bool isStableRelease, string? fallbackPrereleaseFormat, IReadOnlyList<string>? feedSources)
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
                string? apiKey = null;
                var apiKeyGroup = match.Groups[ApiKeyText];
                if (apiKeyGroup.Success)
                {
                    apiKey = apiKeyGroup.Value;
                }

                var uri = match.Groups[UriText].Value;
                string? symbolsUri = null;
                var symbolsUriGroup = match.Groups[SymbolsUriText];
                if (symbolsUriGroup.Success)
                {
                    symbolsUri = symbolsUriGroup.Value;
                }

                string? symbolsApiKey = null;
                var symbolsApiKeyGroup = match.Groups[SymbolsApiKeyText];
                if (symbolsApiKeyGroup.Success)
                {
                    symbolsApiKey = symbolsApiKeyGroup.Value;
                }

                var stage = defaultStage;
                if (stageNameGroup.Success)
                {
                    stage = stageNameGroup.Value;
                }

                var feedUri = uri;
                var feedUriGroup = match.Groups[FeedUriText];
                if (feedUriGroup.Success)
                {
                    feedUri = feedUriGroup.Value;
                }

                var prereleaseFormat = fallbackPrereleaseFormat;
                var prereleaseFormatGroup = match.Groups[PrereleasFormatText];
                if (prereleaseFormatGroup.Success)
                {
                    prereleaseFormat = prereleaseFormatGroup.Value;
                }

                return new Source(name, uri, apiKey, symbolsUri, symbolsApiKey, stage, isStableRelease, feedUri, prereleaseFormat, feedSources);
            }

            return default;
        }
    }
}