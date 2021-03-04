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

        public SemanticVersion GetVersion(string packageId, NuGetVersion nuGetVersion, VersioningMode versioningMode, bool isStableRelease, Source source, IReadOnlyList<string> latestVersionSources, ILogger logger)
        {
            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(packageId, nuGetVersion, isStableRelease, source, latestVersionSources, logger),
                VersioningMode.AutomaticLatestRevision => this.GetAutomaticLatestRevisionVersion(packageId, nuGetVersion, isStableRelease, source, latestVersionSources, logger),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(packageId, nuGetVersion, isStableRelease, source, logger),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(nuGetVersion, isStableRelease, source),
                VersioningMode.NoChange => this.GetNoChangeVersion(nuGetVersion, isStableRelease, source),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(string packageId, NuGetVersion semanticVersion, bool isStableRelease, Source source, IReadOnlyList<string> latestVersionSources, ILogger logger)
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

            if (isStableRelease)
            {
                return new SemanticVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement);
            }

            return new SemanticVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch + patchIncrement, this.GetPrereleasePostfix(source));
        }

        private SemanticVersion GetAutomaticLatestRevisionVersion(string packageId, NuGetVersion semanticVersion, bool isStableRelease, Source source, IReadOnlyList<string> latestVersionSources, ILogger logger)
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

            if (isStableRelease)
            {
                return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement);
            }

            return new NuGetVersion(latestVersion.Major, latestVersion.Minor, latestVersion.Patch, latestVersion.Revision + revisionIncrement, this.GetPrereleasePostfix(source), null);
        }

        private SemanticVersion GetIncrementPatchIfStableExistForPrereleaseVersion(string packageId, SemanticVersion semanticVersion, bool isStableRelease, Source source, ILogger logger)
        {
            if (isStableRelease)
            {
                return semanticVersion;
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, semanticVersion, source.LatestVersionUri, logger);
            packageExistsTask.Wait();
            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + (packageExistsTask.Result ? 1 : 0), this.GetPrereleasePostfix(source));
        }

        private SemanticVersion GetIncrementPatchVersion(SemanticVersion semanticVersion, bool isStableRelease, Source source)
        {
            if (isStableRelease)
            {
                return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1);
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1, this.GetPrereleasePostfix(source));
        }

        private SemanticVersion GetNoChangeVersion(SemanticVersion semanticVersion, bool isStableRelease, Source source)
        {
            if (isStableRelease)
            {
                return semanticVersion;
            }

            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, this.GetPrereleasePostfix(source));
        }

        private string GetPrereleasePostfix(Source source)
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(source.PackagePrefix))
            {
                stringBuilder.Append(source.PackagePrefix);
                stringBuilder.Append('-');
            }

            if (!string.IsNullOrEmpty(source.Stage))
            {
                stringBuilder.Append(source.Stage);
                stringBuilder.Append('-');
            }

            stringBuilder.Append('u');
            stringBuilder.Append(this.dateTime.UtcTime.ToString(PrereleasePackageDateTimeFormat));

            if (!string.IsNullOrEmpty(source.PackagePostfix))
            {
                stringBuilder.Append('-');
                stringBuilder.Append(source.PackagePostfix);
            }

            return stringBuilder.ToString();
        }
    }
}