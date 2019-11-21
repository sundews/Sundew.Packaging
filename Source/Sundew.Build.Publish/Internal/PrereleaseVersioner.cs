// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System;
    using global::NuGet.Common;
    using global::NuGet.Versioning;
    using Sundew.Base.Time;
    using Sundew.Build.Publish.Commands;
    using Sundew.Build.Publish.Internal.Commands;

    internal class PrereleaseVersioner : IPrereleaseVersioner
    {
        internal const string PrePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private readonly IDateTime dateTime;
        private readonly IAutomaticPackageVersioner automaticPackageVersioner;

        public PrereleaseVersioner(IDateTime dateTime, IAutomaticPackageVersioner automaticPackageVersioner)
        {
            this.dateTime = dateTime;
            this.automaticPackageVersioner = automaticPackageVersioner;
        }

        public SemanticVersion GetPrereleaseVersion(string packageId, SemanticVersion semanticVersion, PrereleaseVersioningMode prereleaseVersioningMode, Source source, ILogger logger)
        {
            return prereleaseVersioningMode switch
            {
                PrereleaseVersioningMode.Automatic => this.GetPrereleaseVersion(this.automaticPackageVersioner.GetSemanticVersion(packageId, semanticVersion, source.Uri, logger).Result, source),
                PrereleaseVersioningMode.IncrementPatch => new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1, this.GetPrereleasePostfix(source)),
                PrereleaseVersioningMode.NoChange => this.GetPrereleaseVersion(semanticVersion, source),
                _ => throw new ArgumentOutOfRangeException("prereleaseVersioningMode", prereleaseVersioningMode, $"Unsupported prerelease versioning mode: {prereleaseVersioningMode}")
            };
        }

        private SemanticVersion GetPrereleaseVersion(SemanticVersion semanticVersion, Source source)
        {
            return new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, this.GetPrereleasePostfix(source));
        }

        private string GetPrereleasePostfix(Source source)
        {
            return source.PackagePrefix + this.dateTime.UtcTime.ToString(PrePackageDateTimeFormat);
        }
    }
}