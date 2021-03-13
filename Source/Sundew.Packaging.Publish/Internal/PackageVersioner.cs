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
    using global::NuGet.Common;
    using global::NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish.Internal.Commands;

    internal class PackageVersioner : IPackageVersioner
    {
        internal const string PrereleasePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private readonly IDateTime dateTime;
        private readonly IPackageExistsCommand packageExistsCommand;
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand;

        public PackageVersioner(IDateTime dateTime, IPackageExistsCommand packageExistsCommand, ILatestPackageVersionCommand latestPackageVersionCommand)
        {
            this.dateTime = dateTime;
            this.packageExistsCommand = packageExistsCommand;
            this.latestPackageVersionCommand = latestPackageVersionCommand;
        }

        public SemanticVersion GetVersion(
            string packageId,
            NuGetVersion nuGetVersion,
            VersioningMode versioningMode,
            SelectedSource selectedSource,
            IReadOnlyList<string> latestVersionSources,
            string parameter,
            ILogger logger)
        {
            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(packageId, nuGetVersion, selectedSource, latestVersionSources, parameter, logger),
                VersioningMode.AutomaticLatestRevision => this.GetAutomaticLatestRevisionVersion(packageId, nuGetVersion, selectedSource, latestVersionSources, parameter, logger),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(packageId, nuGetVersion, selectedSource, parameter, logger),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(nuGetVersion, selectedSource, parameter),
                VersioningMode.NoChange => this.GetNoChangeVersion(nuGetVersion, selectedSource, parameter),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(string packageId, NuGetVersion semanticVersion, SelectedSource selectedSource, IReadOnlyList<string> latestVersionSources, string parameter, ILogger logger)
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

            return new SemanticVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, this.GetPrereleasePostfix(selectedSource, parameter));
        }

        private SemanticVersion GetAutomaticLatestRevisionVersion(string packageId, NuGetVersion semanticVersion, SelectedSource selectedSource, IReadOnlyList<string> latestVersionSources, string parameter, ILogger logger)
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

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement, this.GetPrereleasePostfix(selectedSource, parameter), null);
        }

        private SemanticVersion GetIncrementPatchIfStableExistForPrereleaseVersion(string packageId, SemanticVersion semanticVersion, SelectedSource selectedSource, string parameter, ILogger logger)
        {
            if (selectedSource.IsStableRelease)
            {
                return semanticVersion;
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, semanticVersion, selectedSource.FeedSource, logger);
            packageExistsTask.Wait();
            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + (packageExistsTask.Result ? 1 : 0), this.GetPrereleasePostfix(selectedSource, parameter));
        }

        private SemanticVersion GetIncrementPatchVersion(SemanticVersion semanticVersion, SelectedSource selectedSource, string parameter)
        {
            if (selectedSource.IsStableRelease)
            {
                return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1);
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1, this.GetPrereleasePostfix(selectedSource, parameter));
        }

        private SemanticVersion GetNoChangeVersion(SemanticVersion semanticVersion, SelectedSource selectedSource, string parameter)
        {
            if (selectedSource.IsStableRelease)
            {
                return semanticVersion;
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, this.GetPrereleasePostfix(selectedSource, parameter));
        }

        private string GetPrereleasePostfix(SelectedSource selectedSource, string parameter)
        {
            if (!string.IsNullOrEmpty(selectedSource.PrereleaseFormat) && selectedSource.PrereleaseFormat != null)
            {
                return string.Format(selectedSource.PrereleaseFormat, selectedSource.Stage, this.dateTime.UtcTime.ToString(PrereleasePackageDateTimeFormat), this.dateTime.UtcTime, selectedSource.PackagePrefix, selectedSource.PackagePostfix, parameter).Trim('-');
            }

            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(selectedSource.PackagePrefix))
            {
                stringBuilder.Append(selectedSource.PackagePrefix).Append('-');
            }

            stringBuilder.Append('u');
            stringBuilder.Append(this.dateTime.UtcTime.ToString(PrereleasePackageDateTimeFormat));
            if (!string.IsNullOrEmpty(selectedSource.Stage))
            {
                stringBuilder.Append('-').Append(selectedSource.Stage);
            }

            if (!string.IsNullOrEmpty(selectedSource.PackagePostfix))
            {
                stringBuilder.Append('-').Append(selectedSource.PackagePostfix);
            }

            return stringBuilder.ToString();
        }
    }
}