// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Source.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using global::NuGet.Configuration;

    internal readonly struct Source : IEquatable<Source>
    {
        internal const string DevelopmentPackagePrefix = "dev-u";
        internal const string IntegrationPackagePrefix = "int-u";
        internal const string PrePackagePrefix = "pre-u";
        internal const string ConfigText = "config";
        internal const string DefaultPushSourceText = "defaultPushSource";
        private const string StageRegexText = "StageRegex";
        private const string UriText = "Uri";
        private const string SymbolsUriText = "SymbolsUri";
        private const string DefaultSourceNameText = "default";
        private const string DefaultStableSourceNameText = "default-stable";
        private const string LocalStableSourceNameText = "local-stable";
        private const string EscapedPipeText = "||";
        private const string PipeText = "|";
        private const string NoDefaultPushSourceHasBeenConfiguredText = "No default push source has been configured.";
        private static readonly Regex SourceRegex = new Regex($@"((?<{StageRegexText}>([^\|\s]|\|\|)+)\|)(?<{UriText}>[^\|\s]+)(\|(?<{SymbolsUriText}>[^\|\s]+))?");

        public Source(Regex stageRegex, string uri, string symbolsUri, string packagePrefix, bool isRelease)
        {
            this.StageRegex = stageRegex;
            this.Uri = uri;
            this.SymbolsUri = symbolsUri;
            this.PackagePrefix = packagePrefix;
            this.IsRelease = isRelease;
        }

        public Regex StageRegex { get; }

        public string Uri { get; }

        public string SymbolsUri { get; }

        public string PackagePrefix { get; }

        public bool IsRelease { get; }

        public static Source SelectSource(
            string sourceName,
            string productionSource,
            string integrationSource,
            string developmentSource,
            string localSource,
            ISettings defaultSettings)
        {
            if (!string.IsNullOrEmpty(sourceName))
            {
                if (sourceName.StartsWith(DefaultSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    var defaultSource = defaultSettings.GetSection(ConfigText)?.Items.OfType<AddItem>()
                        .FirstOrDefault(x =>
                            x.Key.Equals(DefaultPushSourceText, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (defaultSource == null)
                    {
                        throw new InvalidOperationException(NoDefaultPushSourceHasBeenConfiguredText);
                    }

                    if (sourceName.Equals(DefaultStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new Source(default, defaultSource, default, string.Empty, true);
                    }

                    return new Source(default, defaultSource, default, PrePackagePrefix, false);
                }

                if (sourceName.Equals(LocalStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Source(default, localSource, default, string.Empty, true);
                }

                var sources = new[]
                {
                    Parse(productionSource, null, true),
                    Parse(integrationSource, IntegrationPackagePrefix, false),
                    Parse(developmentSource, DevelopmentPackagePrefix, false),
                };

                var source = sources.FirstOrDefault(x => x.StageRegex != null && x.StageRegex.IsMatch(sourceName));
                if (!source.Equals(default))
                {
                    return source;
                }
            }

            return new Source(null, localSource, default, PrePackagePrefix, false);
        }

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
                   && this.IsRelease == other.IsRelease;
        }
    }
}