// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
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

        public Source(Regex stageRegex, string uri, string symbolsUri, string packagePrefix, bool isRelease, bool isFallback = false, bool isEnabled = true)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.SymbolsUri = symbolsUri;
            this.PackagePrefix = packagePrefix;
            this.IsRelease = isRelease;
            this.IsFallback = isFallback;
            this.IsEnabled = isEnabled;
        }

        public Regex StageRegex { get; }

        public string Uri { get; }

        public string SymbolsUri { get; }

        public string PackagePrefix { get; }

        public bool IsRelease { get; }

        public bool IsFallback { get; }

        public bool IsEnabled { get; }

        public static Source Parse(string pushSource, string packagePrefix, bool isRelease)
        {
            if (string.IsNullOrEmpty(pushSource))
            {
                return new Source(default, default, default, packagePrefix, isRelease);
            }

            var match = SourceRegex.Match(pushSource);
            if (match.Success)
            {
                Regex name = null;
                string uri = null;
                string symbolsUri = null;
                var nameGroup = match.Groups[StageRegexText];
                var uriGroup = match.Groups[UriText];
                var symbolsUriGroup = match.Groups[SymbolsUriText];
                if (nameGroup.Success)
                {
                    name = new Regex(nameGroup.Value.Replace(EscapedPipeText, PipeText));
                }

                if (uriGroup.Success)
                {
                    uri = uriGroup.Value;
                }

                if (symbolsUriGroup.Success)
                {
                    symbolsUri = symbolsUriGroup.Value;
                }

                return new Source(name, uri, symbolsUri, packagePrefix, isRelease);
            }

            return new Source(default, pushSource, default, packagePrefix, isRelease);
        }

        public bool Equals(Source other)
        {
            return Equals(this.StageRegex, other.StageRegex)
                   && this.Uri == other.Uri
                   && this.SymbolsUri == other.SymbolsUri
                   && this.IsRelease == other.IsRelease
                   && this.IsFallback == other.IsFallback
                   && this.IsEnabled == other.IsEnabled;
        }
    }
}