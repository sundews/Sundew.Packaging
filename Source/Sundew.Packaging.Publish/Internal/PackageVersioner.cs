// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Publish.Internal
{
    using System;
    using global::NuGet.Common;
    using global::NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Packaging.Publish.Internal.Commands;

    internal class PackageVersioner : IPackageVersioner
    {
        internal const string PrePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private readonly IDateTime dateTime;
        private readonly IPackageExistsCommand packageExistsCommand;
        private readonly ILatestPackageVersionCommand latestPackageVersionCommand;

        public PackageVersioner(IDateTime dateTime, IPackageExistsCommand packageExistsCommand, ILatestPackageVersionCommand latestPackageVersionCommand)
        {
            this.dateTime = dateTime;
            this.packageExistsCommand = packageExistsCommand;
            this.latestPackageVersionCommand = latestPackageVersionCommand;
        }

        public SemanticVersion GetVersion(string packageId, NuGetVersion nuGetVersion, VersioningMode versioningMode, bool isStableRelease, Source source, ILogger logger)
        {
            return versioningMode switch
            {
                VersioningMode.AutomaticLatestPatch => this.GetAutomaticLatestPatchVersion(packageId, nuGetVersion, isStableRelease, source, logger),
                VersioningMode.IncrementPatchIfStableExistForPrerelease => this.GetIncrementPatchIfStableExistForPrereleaseVersion(packageId, nuGetVersion, isStableRelease, source, logger),
                VersioningMode.AlwaysIncrementPatch => this.GetIncrementPatchVersion(nuGetVersion, isStableRelease, source),
                VersioningMode.NoChange => this.GetNoChangeVersion(nuGetVersion, isStableRelease, source),
                _ => throw new ArgumentOutOfRangeException(nameof(versioningMode), versioningMode, $"Unsupported versioning mode: {versioningMode}"),
            };
        }

        private SemanticVersion GetAutomaticLatestPatchVersion(string packageId, NuGetVersion semanticVersion, bool isStableRelease, Source source, ILogger logger)
        {
            var latestVersionTask = this.latestPackageVersionCommand.GetLatestVersion(packageId, source.Uri, semanticVersion, false, logger);
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

        private SemanticVersion GetIncrementPatchIfStableExistForPrereleaseVersion(string packageId, SemanticVersion semanticVersion, bool isStableRelease, Source source, ILogger logger)
        {
            if (isStableRelease)
            {
                return semanticVersion;
            }

            var packageExistsTask = this.packageExistsCommand.ExistsAsync(packageId, semanticVersion, source.Uri, logger);
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
            return source.PackagePrefix + this.dateTime.UtcTime.ToString(PrePackageDateTimeFormat);
        }
    }
}