// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceSelector.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System;
    using System.Linq;
    using global::NuGet.Configuration;

    internal static class SourceSelector
    {
        internal const string DevelopmentPackagePrefix = "dev-u";
        internal const string IntegrationPackagePrefix = "int-u";
        internal const string PrePackagePrefix = "pre-u";
        private const string DefaultSourceNameText = "default";
        private const string DefaultStableSourceNameText = "default-stable";
        private const string LocalStableSourceNameText = "local-stable";
        private const string NoDefaultPushSourceHasBeenConfiguredText = "No default push source has been configured.";

        public static Source SelectSource(
            string sourceName,
            string productionSource,
            string integrationSource,
            string developmentSource,
            string localSource,
            ISettings defaultSettings,
            bool allowLocalSource)
        {
            if (!string.IsNullOrEmpty(sourceName))
            {
                if (sourceName.StartsWith(DefaultSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    var defaultSource = defaultSettings.GetSection(Source.ConfigText)?.Items.OfType<AddItem>()
                        .FirstOrDefault(x =>
                            x.Key.Equals(Source.DefaultPushSourceText, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    if (defaultSource == null)
                    {
                        throw new InvalidOperationException(NoDefaultPushSourceHasBeenConfiguredText);
                    }

                    if (sourceName.Equals(DefaultStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new Source(default, defaultSource, default, string.Empty, true);
                    }

                    return new Source(default, defaultSource, default, PrePackagePrefix, false, true);
                }

                if (sourceName.Equals(LocalStableSourceNameText, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Source(default, localSource, default, string.Empty, true);
                }

                var sources = new[]
                {
                    Source.Parse(productionSource, null, true),
                    Source.Parse(integrationSource, IntegrationPackagePrefix, false),
                    Source.Parse(developmentSource, DevelopmentPackagePrefix, false),
                };

                var source = sources.FirstOrDefault(x => x.StageRegex != null && x.StageRegex.IsMatch(sourceName));
                if (!source.Equals(default))
                {
                    return source;
                }
            }

            return new Source(null, localSource, default, PrePackagePrefix, false, true, allowLocalSource);
        }
    }
}