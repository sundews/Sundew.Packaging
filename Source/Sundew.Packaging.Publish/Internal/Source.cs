// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Text.RegularExpressions;

    internal readonly struct Source : IEquatable<Source>
    {
        internal const string ConfigText = "config";
        private const string StageRegexText = "StageRegex";
        private const string StageNameText = "StageName";
        private const string UriText = "Uri";
        private const string ApiKeyText = "ApiKey";
        private const string SymbolsApiKeyText = "SymbolsApiKey";
        private const string SymbolsUriText = "SymbolsUri";
        private const string LatestVersionUriText = "LatestVersionUri";
        private static readonly Regex SourceRegex = new($@"(?:(?<StageRegex>(?:[^#\s])+)\s*(?:#\s*(?<StageName>\w*))?\s*=\>\s*)(?:(?:(?<ApiKey>[^@\|\s]*)@)?(?<Uri>[^\|\s|\{{]+))(?:\s*\{{\s*(?<LatestVersionUri>[^\|\s]+)\s*\}}\s*)?(?:\s*\|\s*(?:(?<SymbolsApiKey>[^@\|\s]*)@)?(?<SymbolsUri>[^\|\s]+))?");

        public Source(Regex? stageRegex, string uri, string? apiKey, string? symbolsUri, string? symbolsApiKey, string stage, bool isStableRelease, string latestVersionUri, bool isFallback = false, bool isEnabled = true)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.ApiKey = apiKey;
            this.SymbolsUri = symbolsUri;
            this.SymbolsApiKey = symbolsApiKey;
            this.Stage = stage;
            this.IsStableRelease = isStableRelease;
            this.LatestVersionUri = latestVersionUri;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
            this.PackagePrefix = string.Empty;
            this.PackagePostfix = string.Empty;
        }

        public Source(Source source, string packagePrefix, string packagePostfix)
            : this(source.StageRegex, source.Uri, source.ApiKey, source.SymbolsUri, source.SymbolsApiKey, source.Stage, source.IsStableRelease, source.LatestVersionUri, source.IsFallback, source.IsEnabled)
        {
            this.PackagePrefix = packagePrefix;
            this.PackagePostfix = packagePostfix;
        }

        public Regex? StageRegex { get; }

        public string Uri { get; }

        public string LatestVersionUri { get; }

        public string? ApiKey { get; }

        public string? SymbolsUri { get; }

        public string? SymbolsApiKey { get; }

        public string? PackagePrefix { get; }

        public string Stage { get; }

        public string? PackagePostfix { get; }

        public bool IsStableRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source Parse(string? pushSource, string defaultStage, bool isStableRelease)
        {
            if (pushSource == null || string.IsNullOrEmpty(pushSource))
            {
                return default;
            }

            var match = SourceRegex.Match(pushSource);
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

                var latestVersionUri = uri;
                var latestVersionUriGroup = match.Groups[LatestVersionUriText];
                if (latestVersionUriGroup.Success)
                {
                    latestVersionUri = latestVersionUriGroup.Value;
                }

                return new Source(name, uri, apiKey, symbolsUri, symbolsApiKey, stage, isStableRelease, latestVersionUri);
            }

            return new Source(default, pushSource, default, default, default, defaultStage, isStableRelease, pushSource);
        }

        public bool Equals(Source other)
        {
            return Equals(this.StageRegex, other.StageRegex)
                   && this.Uri == other.Uri
                   && this.ApiKey == other.ApiKey
                   && this.SymbolsUri == other.SymbolsUri
                   && this.SymbolsApiKey == other.SymbolsApiKey
                   && this.PackagePrefix == other.PackagePrefix
                   && this.Stage == other.Stage
                   && this.PackagePostfix == other.PackagePostfix
                   && this.IsStableRelease == other.IsStableRelease
                   && this.IsFallback == other.IsFallback
                   && this.IsEnabled == other.IsEnabled;
        }
    }
}