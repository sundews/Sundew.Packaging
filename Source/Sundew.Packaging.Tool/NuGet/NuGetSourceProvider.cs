// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuGetSourceProvider.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Tool.NuGet
{
    using System;
    using System.Linq;
    using global::NuGet.Common;
    using global::NuGet.Configuration;

    public class NuGetSourceProvider : INuGetSourceProvider
    {
        internal const string PackageSourcesText = "packageSources";

        public SourceSettings GetSourceSettings(string rootDirectory, string? source)
        {
            var defaultSettings = Settings.LoadDefaultSettings(rootDirectory);
            var packageSourceProvider = new PackageSourceProvider(defaultSettings);
            var sourcesSettings = FindSourcesSettings(source, defaultSettings);
            source = sourcesSettings.Source ?? packageSourceProvider.DefaultPushSource ?? throw new InvalidOperationException($"A source for: {source} was not found.");
            return new SourceSettings(source, sourcesSettings.PackageSourcesSection);
        }

        public string GetDefaultSource(string source)
        {
            var settings = Settings.LoadDefaultSettings(NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory));
            var sourcesSettings = FindSourcesSettings(source, settings);
            return sourcesSettings.Source ?? throw new InvalidOperationException($"A source for: {source} was not found.");
        }

        private static (string? Source, SettingSection? PackageSourcesSection) FindSourcesSettings(string? source, ISettings settings)
        {
            var packageSourcesSection = settings.GetSection(PackageSourcesText);
            source = packageSourcesSection?.Items.OfType<AddItem>().FirstOrDefault(x => x.Key == source)?.Value ?? source;
            return (source, packageSourcesSection);
        }
    }

    public record SourceSettings(string Source, SettingSection? PackageSourcesSection);
}