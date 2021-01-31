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
        internal const string DefaultPushSourceText = "defaultPushSource";
        private const string StageRegexText = "StageRegex";
        private const string UriText = "Uri";
        private const string SymbolsUriText = "SymbolsUri";
        private const string EscapedPipeText = "||";
        private const string PipeText = "|";
        private static readonly Regex SourceRegex = new Regex($@"((?<{StageRegexText}>([^\|\s]|\|\|)+)\|)(?<{UriText}>[^\|\s]+)(\|(?<{SymbolsUriText}>[^\|\s]+))?");

        public Source(Regex? stageRegex, string uri, string? symbolsUri, string? packagePrefix, bool isStableRelease, bool isFallback = false, bool isEnabled = true)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.SymbolsUri = symbolsUri;
            this.PackagePrefix = packagePrefix;
            this.IsStableRelease = isStableRelease;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
        }

        public Regex? StageRegex { get; }

        public string Uri { get; }

        public string? SymbolsUri { get; }

        public string? PackagePrefix { get; }

        public bool IsStableRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source Parse(string? pushSource, string? packagePrefix, bool isStableRelease)
        {
            if (pushSource == null || string.IsNullOrEmpty(pushSource))
            {
                return default;
            }

            var match = SourceRegex.Match(pushSource);
            if (match.Success)
            {
                var name = new Regex(match.Groups[StageRegexText].Value.Replace(EscapedPipeText, PipeText));
                var uri = match.Groups[UriText].Value;
                var symbolsUriGroup = match.Groups[SymbolsUriText];
                string? symbolsUri = null;
                if (symbolsUriGroup.Success)
                {
                    symbolsUri = symbolsUriGroup.Value;
                }

                return new Source(name, uri, symbolsUri, packagePrefix, isStableRelease);
            }

            return new Source(default, pushSource, default, packagePrefix, isStableRelease);
        }

        public bool Equals(Source other)
        {
            return Equals(this.StageRegex, other.StageRegex)
                   && this.Uri == other.Uri
                   && this.SymbolsUri == other.SymbolsUri
                   && this.IsStableRelease == other.IsStableRelease
                   && this.IsFallback == other.IsFallback
                   && this.IsEnabled == other.IsEnabled;
        }
    }
}