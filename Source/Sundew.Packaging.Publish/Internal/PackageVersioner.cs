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
        private const string Replacement = "-";
        private static readonly Regex PrefixPostfixReplacement = new(@"[^0-9A-Za-z-]");
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
            string? forceVersion,
            VersioningMode versioningMode,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            DateTime buildDateTime,
            string parameter,
            ILogger logger)
        {
            if (!string.IsNullOrEmpty(forceVersion))
            {
                if (NuGetVersion.TryParse(forceVersion, out NuGetVersion forcedVersion))
                {
                    return forcedVersion;
                }
            }

            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, parameter, logger),
                VersioningMode.AutomaticLatestRevision => this.GetAutomaticLatestRevisionVersion(buildDateTime, packageId, nuGetVersion, selectedSource, latestVersionSources, parameter, logger),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(buildDateTime, packageId, nuGetVersion, selectedSource, parameter, logger),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(buildDateTime, nuGetVersion, selectedSource, parameter),
                VersioningMode.NoChange => this.GetNoChangeVersion(buildDateTime, nuGetVersion, selectedSource, parameter),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion semanticVersion,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string parameter,
            ILogger logger)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, semanticVersion, false, false, logger);
            latestVersionTask.Wait();
            var latestVersion = latestVersionTask.Result;
            var patchIncrement = 1;
            if (latestVersion == null)
            {
                patchIncrement = 0;
                latestVersion = semanticVersion;
            }

            if (selectedSource.IsStableRelease)
            {
                return new SemanticVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement);
            }

            return new SemanticVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter));
        }

        private SemanticVersion GetAutomaticLatestRevisionVersion(
            DateTime buildDateTime,
            string packageId,
            NuGetVersion semanticVersion,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string parameter,
            ILogger logger)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestMajorMinorVersion(packageId, latestVersionSources, semanticVersion, true, false, logger);
            latestVersionTask.Wait();
            var latestVersion = latestVersionTask.Result;
            var revisionIncrement = 1;
            if (latestVersion == null)
            {
                revisionIncrement = 0;
                latestVersion = semanticVersion;
            }

            if (selectedSource.IsStableRelease)
            {
                return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement);
            }

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement, this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter), null);
        }

        private SemanticVersion GetIncrementPatchIfStableExistForPrereleaseVersion(
            DateTime buildDateTime,
            string packageId,
            SemanticVersion semanticVersion,
            SelectedSource selectedSource,
            string parameter,
            ILogger logger)
        {
            if (selectedSource.IsStableRelease)
            {
                return semanticVersion;
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, semanticVersion, selectedSource.FeedSource, logger);
            packageExistsTask.Wait();
            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + (packageExistsTask.Result ? 1 : 0), this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter));
        }

        private SemanticVersion GetIncrementPatchVersion(DateTime buildDateTime, SemanticVersion semanticVersion, SelectedSource selectedSource, string parameter)
        {
            if (selectedSource.IsStableRelease)
            {
                return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1);
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1, this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter));
        }

        private SemanticVersion GetNoChangeVersion(DateTime buildDateTime, SemanticVersion semanticVersion, SelectedSource selectedSource, string parameter)
        {
            if (selectedSource.IsStableRelease)
            {
                return semanticVersion;
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, this.GetPrereleasePostfix(buildDateTime, selectedSource, parameter));
        }

        private string GetPrereleasePostfix(DateTime dateTime, SelectedSource selectedSource, string parameter)
        {
            if (!string.IsNullOrEmpty(selectedSource.PrereleaseFormat) && selectedSource.PrereleaseFormat != null)
            {
                return string.Format(selectedSource.PrereleaseFormat, selectedSource.Stage, dateTime.ToString(PrereleasePackageDateTimeFormat), dateTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, parameter).Trim('-');
            }

            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(selectedSource.PackagePrefix))
            {
                stringBuilder.Append(PrefixPostfixReplacement.Replace(selectedSource.PackagePrefix, Replacement)).Append('-');
            }

            stringBuilder.Append('u');
            stringBuilder.Append(dateTime.ToString(PrereleasePackageDateTimeFormat));
            if (!string.IsNullOrEmpty(selectedSource.Stage))
            {
                stringBuilder.Append('-').Append(selectedSource.Stage);
            }

            if (!string.IsNullOrEmpty(selectedSource.PackagePostfix))
            {
                stringBuilder.Append('-').Append(PrefixPostfixReplacement.Replace(selectedSource.PackagePostfix, Replacement));
            }

            return stringBuilder.ToString();
        }
    }
}