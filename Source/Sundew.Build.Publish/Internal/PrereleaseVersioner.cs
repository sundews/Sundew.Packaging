// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrereleaseVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Build.Publish.Internal
{
    using System;
    using global::NuGet.Versioning;
    using Sundew.Base.Time;

    internal class PrereleaseVersioner : IPrereleaseVersioner
    {
        internal const string PrePackageDateTimeFormat = "yyyyMMdd-HHmmss";
        private readonly IDateTime dateTime;

        public PrereleaseVersioner(IDateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public SemanticVersion GetPrereleaseVersion(SemanticVersion semanticVersion, PrereleaseVersioningMode prereleaseVersioningMode, Source source)
        {
            return prereleaseVersioningMode switch
            {
                Publish.PrereleaseVersioningMode.IncrementPatch => new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch + 1, this.GetPrereleasePostfix(source)),
                Publish.PrereleaseVersioningMode.NoChange => new SemanticVersion(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, this.GetPrereleasePostfix(source)),
                //// Publish.PrereleaseVersioningMode.IncrementPatchIfStableVersionExists => throw new NotImplementedException("This mode has not yet been implemented.");
                _ => throw new ArgumentOutOfRangeException("prereleaseVersioningMode", prereleaseVersioningMode, $"Unsupported prerelease versioning mode: {prereleaseVersioningMode}")
            };
        }

        private string GetPrereleasePostfix(Source pushSource)
        {
            return pushSource.PackagePrefix + this.dateTime.UtcTime.ToString(PrePackageDateTimeFormat);
        }
    }
}