// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPackageVersioner.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Packaging.Versioning
{
    using System;
    using System.Collections.Generic;
    using global::NuGet.Versioning;
    using Sundew.Packaging.Staging;

    public interface IPackageVersioner
    {
        SemanticVersion GetVersion(
            string packageId,
            NuGetVersion nuGetVersion,
            string? combiningVersion,
            string? forceVersion,
            VersioningMode versioningMode,
            SelectedStage selectedSource,
            IReadOnlyList<string> latestVersionSources,
            DateTime buildDateTime,
            string? metadata,
            string? metadataFormat,
            string parameter);
    }
}