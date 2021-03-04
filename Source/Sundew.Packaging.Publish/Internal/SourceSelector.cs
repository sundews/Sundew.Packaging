// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Linq;
    using global::NuGet.Configuration;

    internal static class SourceSelector
    {
        internal const string DefaultPushSourceText = "defaultPushSource";
        internal const string DefaultLocalPackageStage = "pre";
        internal const string DefaultDevelopmentPackageStage = "dev";
        internal const string DefaultIntegrationPackageStage = "ci";
        private const string DefaultSourceNameText = "default";
        private const string DefaultStableSourceNameText = "default-stable";
        private const string LocalStableSourceNameText = "local-stable";
        private const string NoDefaultPushSourceHasBeenConfiguredText = "No default push source has been configured.";
        private const string PrefixGroupName = "Prefix";
        private const string PostfixGroupName = "Postfix";

        public static Source SelectSource(
            string? sourceName,
            string? productionSource,
            string? integrationSource,
            string? developmentSource,
            string localSource,
            ISettings defaultSettings,
            bool allowLocalSource)
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
                        return new Source(default, defaultSource, default, default, default, string.Empty, true, defaultSource);
                    }

                    return new Source(default, defaultSource, default, default, default, DefaultLocalPackageStage, false, defaultSource, true);
                }

                if (sourceName.Equals(LocalStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Source(default, localSource, default, default, default, string.Empty, true, localSource);
                }

                var sources = new[]
                {
                    Source.Parse(productionSource, string.Empty, true),
                    Source.Parse(integrationSource, DefaultIntegrationPackageStage, false),
                    Source.Parse(developmentSource, DefaultDevelopmentPackageStage, false),
                };

                var (source, match) = sources.Select(x => (source: x, match: x.StageRegex?.Match(sourceName))).FirstOrDefault(x => x.match?.Success ?? false);
                if (!source.Equals(default))
                {
                    var prefix = match?.Groups[PrefixGroupName].Value ?? string.Empty;
                    var postfix = match?.Groups[PostfixGroupName].Value ?? string.Empty;
                    return new Source(source, prefix, postfix);
                }
            }

            return new Source(null, localSource, default, default, default, DefaultLocalPackageStage, false, localSource, true, allowLocalSource);
        }
    }
}