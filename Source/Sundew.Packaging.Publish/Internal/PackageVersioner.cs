// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using global::NuGet.Common;
    using global::NuGet.Versioning;
    using Sundew.Packaging.Publish.Internal.Commands;

    internal class PackageVersioner : IPackageVersioner
    {
        internal const string PrereleasePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private const string Dash = "-";
        private const string Dot = ".";
        private static readonly Regex PrefixPostfixReplacement = new(@"[^0-9A-Za-z-\.]");
        private static readonly Regex RemoveDuplicates = new Regex(@"\.\.+|\-\-+");
        private readonly IPackageExistsCommand packageExistsCommand;
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand;

        public PackageVersioner(IPackageExistsCommand packageExistsCommand, ILatestPackageVersionCommand latestPackageVersionCommand)
        {
            this.packageExistsCommand = packageExistsCommand;
            this.latestPackageVersionCommand = latestPackageVersionCommand;
        }

        public SemanticVersion GetVersion(
            string packageId,
            NuGetVersion nuGetVersion,
            string? combinedVersion,
            string? forceVersion,
            VersioningMode versioningMode,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            DateTime buildDateTime,
            string? metadata,
            string? metadataFormat,
            string parameter,
            ILogger nuGetLogger,
            Logging.ILogger logger)
        {
            if (!string.IsNullOrEmpty(forceVersion))
            {
                if (NuGetVersion.TryParse(string.Format(forceVersion, nuGetVersion.Major, nuGetVersion.Minor, nuGetVersion.Patch, nuGetVersion.Revision), out NuGetVersion forcedVersion))
                {
                    logger.LogImportant($"SPP: Forced version to: {forcedVersion}");
                    return forcedVersion;
                }
            }

            if (!string.IsNullOrEmpty(combinedVersion))
            {
                if (NuGetVersion.TryParse(string.Format(combinedVersion, nuGetVersion.Major, nuGetVersion.Minor, nuGetVersion.Patch, nuGetVersion.Revision), out NuGetVersion combinedNuGetVersion))
                {
                    nuGetVersion = combinedNuGetVersion;
                    versioningMode = VersioningMode.NoChange;
                    logger.LogImportant($"SPP: Combined version into: {nuGetVersion}");
                }
            }

            metadata = !string.IsNullOrEmpty(nuGetVersion.Metadata) ? nuGetVersion.Metadata : metadata ?? string.Empty;
            var versionMetadata = this.GetMetadata(metadata, metadataFormat, selectedSource, buildDateTime, parameter);
            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, metadata, parameter, versionMetadata, nuGetLogger, logger),
                VersioningMode.AutomaticLatestRevision => this.GetAutomaticLatestRevisionVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, metadata, parameter, versionMetadata, nuGetLogger, logger),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(buildDateTime, packageId, nuGetVersion, selectedSource, metadata, parameter, versionMetadata, nuGetLogger),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(buildDateTime, nuGetVersion, selectedSource, metadata, parameter, versionMetadata),
                VersioningMode.NoChange => this.GetNoChangeVersion(buildDateTime, nuGetVersion, selectedSource, metadata, parameter, versionMetadata),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion nugetVersion,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string metadata,
            string parameter,
            string versionMetadata,
            ILogger nuGetLogger,
            Logging.ILogger logger)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, nugetVersion, false, false, nuGetLogger, logger);
            latestVersionTask.Wait();
            var latestVersion = latestVersionTask.Result;
            var patchIncrement = 1;
            if (latestVersion == null)
            {
                patchIncrement = 0;
                latestVersion = nugetVersion;
            }

            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, default(string), versionMetadata);
            }

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter, metadata), versionMetadata);
        }

        private SemanticVersion GetAutomaticLatestRevisionVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion nugetVersion,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string metadata,
            string parameter,
            string versionMetadata,
            ILogger nuGetLogger,
            Logging.ILogger logger)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, nugetVersion, true, false, nuGetLogger, logger);
            latestVersionTask.Wait();
            var latestVersion = latestVersionTask.Result;
            var revisionIncrement = 1;
            if (latestVersion == null)
            {
                revisionIncrement = 0;
                latestVersion = nugetVersion;
            }

            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement, default(string), versionMetadata);
            }

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetIncrementPatchIfStableExistForPrereleaseVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion nugetVersion,
            SelectedSource selectedSource,
            string metadata,
            string parameter,
            string versionMetadata,
            ILogger logger)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, default(string), versionMetadata);
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, nugetVersion, selectedSource.FeedSource, logger);
            packageExistsTask.Wait();
            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + (packageExistsTask.Result ? 1 : 0), this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetIncrementPatchVersion(DateTime buildDateTime, NuGetVersion nugetVersion, SelectedSource selectedSource, string metadata, string parameter, string versionMetadata)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + 1, default(string), versionMetadata);
            }

            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + 1, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetNoChangeVersion(DateTime buildDateTime, NuGetVersion nugetVersion, SelectedSource selectedSource, string metadata, string parameter, string versionMetadata)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, nugetVersion.Revision, default(string), versionMetadata);
            }

            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, nugetVersion.Revision, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private string GetMetadata(string metadata, string? metadataFormat, SelectedSource selectedSource, DateTime dateTime, string parameter)
        {
            if (!string.IsNullOrEmpty(metadataFormat) && metadataFormat != null)
            {
                return RemoveDuplicates.Replace(
                    string.Format(metadataFormat, selectedSource.Stage, dateTime.ToString(PrereleasePackageDateTimeFormat), dateTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, metadata, parameter).Trim('-'),
                    match => match.Value[0].ToString());
            }

            return metadata;
        }

        private string GetPrereleasePostfix(DateTime dateTime, SelectedSource selectedSource, string metadata, string parameter)
        {
            if (!string.IsNullOrEmpty(selectedSource.PrereleaseFormat) && selectedSource.PrereleaseFormat != null)
            {
                return RemoveDuplicates.Replace(
                    string.Format(selectedSource.PrereleaseFormat, selectedSource.Stage, dateTime.ToString(PrereleasePackageDateTimeFormat), dateTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, metadata, parameter).Trim('-'),
                    match => match.Value[0].ToString());
            }

            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(selectedSource.PackagePrefix))
            {
                stringBuilder.Append(PrefixPostfixReplacement.Replace(selectedSource.PackagePrefix, Dash)).Append('-');
            }

            stringBuilder.Append('u');
            stringBuilder.Append(dateTime.ToString(PrereleasePackageDateTimeFormat));
            if (!string.IsNullOrEmpty(selectedSource.Stage))
            {
                stringBuilder.Append('-').Append(selectedSource.Stage);
            }

            if (!string.IsNullOrEmpty(selectedSource.PackagePostfix))
            {
                stringBuilder.Append('-').Append(PrefixPostfixReplacement.Replace(selectedSource.PackagePostfix, Dash));
            }

            return stringBuilder.ToString();
        }
    }
}