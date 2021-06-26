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

    /// <summary>
    /// Interface for versioning a package.
    /// </summary>
    public interface IPackageVersioner
    {
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="nuGetVersion">The nu get version.</param>
        /// <param name="combiningVersion">The combining version.</param>
        /// <param name="forceVersion">The force version.</param>
        /// <param name="versioningMode">The versioning mode.</param>
        /// <param name="selectedSource">The selected source.</param>
        /// <param name="latestVersionSources">The latest version sources.</param>
        /// <param name="buildDateTime">The build date time.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="metadataFormat">The metadata format.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>The semantic version.</returns>
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