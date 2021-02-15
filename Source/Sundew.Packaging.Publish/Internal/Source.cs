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
        private const string SymbolsUriText = "SymbolsUri";
        private const string EscapedPipeText = "||";
        private const string PipeText = "|";
        private static readonly Regex SourceRegex = new Regex($@"(?:(?<StageRegex>(?:[^\|\s=]|\|\|)+)(?:=\>(?<StageName>[^\|\s]*))?\|)(?<Uri>[^\|\s]+)(?:\|(?<SymbolsUri>[^\|\s]+))?");

        public Source(Regex? stageRegex, string uri, string? symbolsUri, string? stage, bool isStableRelease, bool isFallback = false, bool isEnabled = true)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.SymbolsUri = symbolsUri;
            this.Stage = stage;
            this.IsStableRelease = isStableRelease;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
            this.PackagePrefix = string.Empty;
            this.PackagePostfix = string.Empty;
        }

        public Source(Source source, string packagePrefix, string packagePostfix)
            : this(source.StageRegex, source.Uri, source.SymbolsUri, source.Stage, source.IsStableRelease, source.IsFallback, source.IsEnabled)
        {
            this.PackagePrefix = packagePrefix;
            this.PackagePostfix = packagePostfix;
        }

        public Regex? StageRegex { get; }

        public string Uri { get; }

        public string? SymbolsUri { get; }

        public string? PackagePrefix { get; }

        public string? Stage { get; }

        public string? PackagePostfix { get; }

        public bool IsStableRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source Parse(string? pushSource, string? defaultStage, bool isStableRelease)
        {
            if (pushSource == null || string.IsNullOrEmpty(pushSource))
            {
                return default;
            }

            var match = SourceRegex.Match(pushSource);
            if (match.Success)
            {
                var name = new Regex(match.Groups[StageRegexText].Value.Replace(EscapedPipeText, PipeText));
                var stageNameGroup = match.Groups[StageNameText];
                var uri = match.Groups[UriText].Value;
                var symbolsUriGroup = match.Groups[SymbolsUriText];
                var stage = defaultStage;
                string? symbolsUri = null;
                if (symbolsUriGroup.Success)
                {
                    symbolsUri = symbolsUriGroup.Value;
                }

                if (stageNameGroup.Success)
                {
                    stage = stageNameGroup.Value;
                }

                return new Source(name, uri, symbolsUri, stage, isStableRelease);
            }

            return new Source(default, pushSource, default, defaultStage, isStableRelease);
        }

        public bool Equals(Source other)
        {
            return Equals(this.StageRegex, other.StageRegex)
                   && this.Uri == other.Uri
                   && this.SymbolsUri == other.SymbolsUri
                   && this.PackagePrefix == other.PackagePrefix
                   && this.Stage == other.Stage
                   && this.PackagePostfix == other.PackagePostfix
                   && this.IsStableRelease == other.IsStableRelease
                   && this.IsFallback == other.IsFallback
                   && this.IsEnabled == other.IsEnabled;
        }
    }
}