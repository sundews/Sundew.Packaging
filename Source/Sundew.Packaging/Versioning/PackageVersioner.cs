// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using global::NuGet.Versioning;
    using Sundew.Base.Text;
    using Sundew.Packaging.Staging;
    using Sundew.Packaging.Versioning.Commands;

    /// <summary>
    /// Determines the next version of a package.
    /// </summary>
    /// <seealso cref="Sundew.Packaging.Versioning.IPackageVersioner" />
    public class PackageVersioner : IPackageVersioner
    {
        internal const string PrereleasePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private const string Dash = "-";
        private static readonly Regex PrefixPostfixReplacement = new(@"[^0-9A-Za-z-\.]");
        private static readonly Regex RemoveDuplicates = new(@"\.\.+|\-\-+");
        private static readonly string[] VersionFormatNames = new[] { "Major", "Minor", "Patch", "Revision" };
        private static readonly string[] MetadataAndPrereleaseFormatNames = new[] { "Stage", "DateTime", "DateTimeValue", "Prefix", "Postfix", "Metadata", "Parameter" };
        private readonly IPackageExistsCommand packageExistsCommand;
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand;
        private readonly Versioning.Logging.ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageVersioner"/> class.
        /// </summary>
        /// <param name="packageExistsCommand">The package exists command.</param>
        /// <param name="latestPackageVersionCommand">The latest package version command.</param>
        /// <param name="logger">The logger.</param>
        public PackageVersioner(
            IPackageExistsCommand packageExistsCommand,
            ILatestPackageVersionCommand latestPackageVersionCommand,
            Versioning.Logging.ILogger logger)
        {
            this.packageExistsCommand = packageExistsCommand;
            this.latestPackageVersionCommand = latestPackageVersionCommand;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="nuGetVersion">The nu get version.</param>
        /// <param name="versionFormat">The version format.</param>
        /// <param name="forceVersion">The force version.</param>
        /// <param name="versioningMode">The versioning mode.</param>
        /// <param name="selectedSource">The selected source.</param>
        /// <param name="latestVersionSources">The latest version sources.</param>
        /// <param name="buildDateTime">The build date time.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="metadataFormat">The metadata format.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The version.</returns>
        public SemanticVersion GetVersion(
            string packageId,
            NuGetVersion nuGetVersion,
            string? versionFormat,
            string? forceVersion,
            VersioningMode versioningMode,
            SelectedStage selectedSource,
            IReadOnlyList<string> latestVersionSources,
            DateTime buildDateTime,
            string? metadata,
            string? metadataFormat,
            string parameter)
        {
            if (!forceVersion.IsNullOrEmpty())
            {
                var versionFormatter = new NamedFormatString(forceVersion, VersionFormatNames);
                if (NuGetVersion.TryParse(versionFormatter.Format(nuGetVersion.Major, nuGetVersion.Minor, nuGetVersion.Patch, nuGetVersion.Revision), out NuGetVersion forcedVersion))
                {
                    this.logger.LogImportant($"SPP: Forced version to: {forcedVersion}");
                    return forcedVersion;
                }
            }

            if (!versionFormat.IsNullOrEmpty())
            {
                var versionFormatter = new NamedFormatString(versionFormat, VersionFormatNames);
                if (NuGetVersion.TryParse(versionFormatter.Format(nuGetVersion.Major, nuGetVersion.Minor, nuGetVersion.Patch, nuGetVersion.Revision), out NuGetVersion formattedNuGetVersion))
                {
                    nuGetVersion = formattedNuGetVersion;
                    versioningMode = VersioningMode.NoChange;
                    this.logger.LogImportant($"SPP: Formatted version to: {nuGetVersion}");
                }
            }

            metadata = !string.IsNullOrEmpty(nuGetVersion.Metadata) ? nuGetVersion.Metadata : metadata ?? string.Empty;
            var versionMetadata = this.GetMetadata(metadata, metadataFormat, selectedSource, buildDateTime, parameter);
            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, metadata, parameter, versionMetadata),
                VersioningMode.AutomaticLatestRevision => this.GetAutomaticLatestRevisionVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, metadata, parameter, versionMetadata),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(buildDateTime, packageId, nuGetVersion, selectedSource, metadata, parameter, versionMetadata),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(buildDateTime, nuGetVersion, selectedSource, metadata, parameter, versionMetadata),
                VersioningMode.NoChange => this.GetNoChangeVersion(buildDateTime, nuGetVersion, selectedSource, metadata, parameter, versionMetadata),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion nugetVersion,
            SelectedStage selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string metadata,
            string parameter,
            string versionMetadata)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, nugetVersion, false, false);
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

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetAutomaticLatestRevisionVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion nugetVersion,
            SelectedStage selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string metadata,
            string parameter,
            string versionMetadata)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, nugetVersion, true, false);
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
            SelectedStage selectedSource,
            string metadata,
            string parameter,
            string versionMetadata)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, default(string), versionMetadata);
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, nugetVersion, selectedSource.FeedSource);
            packageExistsTask.Wait();
            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + (packageExistsTask.Result ? 1 : 0), this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetIncrementPatchVersion(DateTime buildDateTime, NuGetVersion nugetVersion, SelectedStage selectedSource, string metadata, string parameter, string versionMetadata)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + 1, default(string), versionMetadata);
            }

            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch + 1, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private SemanticVersion GetNoChangeVersion(DateTime buildDateTime, NuGetVersion nugetVersion, SelectedStage selectedSource, string metadata, string parameter, string versionMetadata)
        {
            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, nugetVersion.Revision, default(string), versionMetadata);
            }

            return new NuGetVersion(nugetVersion.Major, nugetVersion.Minor, nugetVersion.Patch, nugetVersion.Revision, this.GetPrereleasePostfix(buildDateTime, selectedSource, metadata, parameter), versionMetadata);
        }

        private string GetMetadata(string metadata, string? metadataFormat, SelectedStage selectedSource, DateTime dateTime, string parameter)
        {
            if (!string.IsNullOrEmpty(metadataFormat) && metadataFormat != null)
            {
                var metadataFormatter = new NamedFormatString(metadataFormat, MetadataAndPrereleaseFormatNames);
                return RemoveDuplicates.Replace(
                    metadataFormatter.Format(selectedSource.VersionStageName, dateTime.ToString(PrereleasePackageDateTimeFormat), dateTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, metadata, parameter),
                    match => match.Value[0].ToString()).Trim('-');
            }

            return metadata;
        }

        private string GetPrereleasePostfix(DateTime dateTime, SelectedStage selectedSource, string metadata, string parameter)
        {
            if (!string.IsNullOrEmpty(selectedSource.PrereleaseFormat) && selectedSource.PrereleaseFormat != null)
            {
                var prereleaseFormatter = new NamedFormatString(selectedSource.PrereleaseFormat, MetadataAndPrereleaseFormatNames);
                return RemoveDuplicates.Replace(
                    prereleaseFormatter.Format(selectedSource.VersionStageName, dateTime.ToString(PrereleasePackageDateTimeFormat), dateTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, metadata, parameter),
                    match => match.Value[0].ToString()).Trim('-');
            }

            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(selectedSource.PackagePrefix))
            {
                stringBuilder.Append(selectedSource.PackagePrefix).Append('-');
            }

            stringBuilder.Append('u');
            stringBuilder.Append(dateTime.ToString(PrereleasePackageDateTimeFormat));
            if (!string.IsNullOrEmpty(selectedSource.VersionStageName))
            {
                stringBuilder.Append('-').Append(selectedSource.VersionStageName);
            }

            if (!string.IsNullOrEmpty(selectedSource.PackagePostfix))
            {
                stringBuilder.Append('-').Append(selectedSource.PackagePostfix);
            }

            return PrefixPostfixReplacement.Replace(stringBuilder.ToString(), Dash).Trim('-');
        }
    }
}